using Dotori.Core.Linker;
using Dotori.Core.Model;
using Dotori.Core.Parsing;
using Dotori.Core.Toolchain;

namespace Dotori.Core.Build;

/// <summary>
/// The primary entry point for planning a build.
/// Given a flat project model and toolchain, produces all compile + link jobs.
/// Supports PCH, C++ Modules (with ModuleScanner/ModuleSorter), Unity Build,
/// incremental build, and the Apple/LLD linker selection.
/// </summary>
public sealed class BuildPlanner
{
    private readonly FlatProjectModel _model;
    private readonly ToolchainInfo    _toolchain;
    private readonly string           _config;
    private readonly string           _targetId;
    private readonly string           _cacheDir;

    public BuildPlanner(
        FlatProjectModel model,
        ToolchainInfo    toolchain,
        string           config,
        string           targetId)
    {
        _model     = model;
        _toolchain = toolchain;
        _config    = config;
        _targetId  = targetId;

        // obj output dir: .dotori-cache/obj/<target>-<config>/
        _cacheDir = Path.Combine(
            model.ProjectDir, ".dotori-cache",
            "obj", $"{targetId}-{config.ToLower()}");
        Directory.CreateDirectory(_cacheDir);
    }

    // ─── Compile jobs ─────────────────────────────────────────────────────────

    /// <summary>
    /// Plan and return all compile jobs for this project.
    /// Handles unity batching, PCH, and incremental builds.
    /// Does NOT include module BMI compile jobs (use <see cref="PlanModuleJobsAsync"/> for those).
    /// </summary>
    /// <param name="checker">Optional incremental build checker.</param>
    /// <param name="pchPlan">Optional pre-compiled header plan.</param>
    /// <param name="noUnity">If true, bypass unity build batching.</param>
    /// <param name="bmiPaths">Optional map of logical module name → .pcm/.ifc path for import flags.</param>
    public IReadOnlyList<CompileJob> PlanCompileJobs(
        IncrementalChecker? checker = null,
        PchPlanner.PchPlan? pchPlan = null,
        bool                noUnity = false,
        IReadOnlyDictionary<string, string>? bmiPaths = null)
    {
        // Expand source globs
        var includes = _model.Sources.Where(s =>  s.IsInclude).Select(s => s.Glob).ToList();
        var excludes = _model.Sources.Where(s => !s.IsInclude).Select(s => s.Glob).ToList();
        var allSources = GlobExpander.Expand(_model.ProjectDir, includes, excludes);

        // Expand module globs
        var modIncludes = _model.Modules.Where(s =>  s.IsInclude).Select(s => s.Glob).ToList();
        var modExcludes = _model.Modules.Where(s => !s.IsInclude).Select(s => s.Glob).ToList();
        var moduleSources = GlobExpander.Expand(_model.ProjectDir, modIncludes, modExcludes);

        List<string> filesToCompile;

        if (!noUnity && _model.UnityBuild?.Enabled == true)
        {
            var unityDir  = Path.Combine(_model.ProjectDir, ".dotori-cache", "unity");
            var exclude   = _model.UnityBuild.Exclude;
            int batchSize = _model.UnityBuild.BatchSize > 0 ? _model.UnityBuild.BatchSize : 8;

            var (unityFiles, nonUnity) = UnityBatcher.CreateBatches(
                allSources, moduleSources, exclude, batchSize, unityDir);

            filesToCompile = new List<string>(unityFiles);
            filesToCompile.AddRange(nonUnity);
        }
        else
        {
            // Exclude module sources from regular compile (they go through PlanModuleJobsAsync)
            var moduleSet = new HashSet<string>(moduleSources, StringComparer.OrdinalIgnoreCase);
            filesToCompile = allSources.Where(f => !moduleSet.Contains(f)).ToList();
        }

        return BuildCompileJobs(filesToCompile, checker, pchPlan, bmiPaths);
    }

    private IReadOnlyList<CompileJob> BuildCompileJobs(
        IReadOnlyList<string> files,
        IncrementalChecker? checker,
        PchPlanner.PchPlan? pchPlan,
        IReadOnlyDictionary<string, string>? bmiPaths = null)
    {
        var jobs = new List<CompileJob>();

        if (_toolchain.Kind == CompilerKind.Msvc)
        {
            var baseFlags = MsvcDriver.CompileFlags(_model, _toolchain, _config, _cacheDir);
            if (pchPlan is not null) baseFlags = PchPlanner.AddUseFlags(baseFlags, pchPlan).ToList();
            foreach (var src in files)
            {
                if (checker is not null && !checker.IsChanged(src)) continue;
                var flags = AddModuleImportFlags(src, baseFlags, bmiPaths, _toolchain.Kind);
                jobs.Add(MsvcDriver.MakeCompileJob(src, _cacheDir, flags));
            }
        }
        else if (_toolchain.Kind == CompilerKind.Emscripten)
        {
            var baseFlags = EmscriptenDriver.CompileFlags(_model, _cacheDir);
            foreach (var src in files)
            {
                if (checker is not null && !checker.IsChanged(src)) continue;
                jobs.Add(EmscriptenDriver.MakeCompileJob(src, _cacheDir, baseFlags));
            }
        }
        else  // Clang
        {
            var baseFlags = ClangDriver.CompileFlags(_model, _toolchain, _config, _cacheDir);
            if (pchPlan is not null) baseFlags = PchPlanner.AddUseFlags(baseFlags, pchPlan).ToList();
            foreach (var src in files)
            {
                if (checker is not null && !checker.IsChanged(src)) continue;
                var flags = AddModuleImportFlags(src, baseFlags, bmiPaths, _toolchain.Kind);
                jobs.Add(ClangDriver.MakeCompileJob(src, _cacheDir, flags));
            }
        }

        return jobs;
    }

    /// <summary>
    /// Scans a source file for module imports and returns flags augmented with
    /// -fmodule-file= (Clang) or /reference (MSVC) for each imported module.
    /// </summary>
    private static IReadOnlyList<string> AddModuleImportFlags(
        string sourceFile,
        IReadOnlyList<string> baseFlags,
        IReadOnlyDictionary<string, string>? bmiPaths,
        CompilerKind kind)
    {
        if (bmiPaths is null || bmiPaths.Count == 0) return baseFlags;

        var dep = ModuleScanner.ScanByText(sourceFile);
        if (dep.Requires.Count == 0) return baseFlags;

        var importFlags = ModuleSorter.BuildImportFlags(dep.Requires, bmiPaths, kind);
        if (importFlags.Count == 0) return baseFlags;

        var combined = new List<string>(baseFlags);
        combined.AddRange(importFlags);
        return combined;
    }

    // ─── PCH planning ─────────────────────────────────────────────────────────

    /// <summary>
    /// Plan the PCH build. Returns null if no PCH is configured.
    /// Returns a plan with <see cref="PchPlanner.PchPlan.BuildJob"/> == null if PCH is up-to-date.
    /// </summary>
    public PchPlanner.PchPlan? PlanPch(IncrementalChecker? checker = null)
    {
        if (_model.Pch is null) return null;

        bool hasModules = _model.Modules.Any(m => m.IsInclude);

        IReadOnlyList<string> baseFlags = _toolchain.Kind == CompilerKind.Msvc
            ? MsvcDriver.CompileFlags(_model, _toolchain, _config, _cacheDir)
            : _toolchain.Kind == CompilerKind.Emscripten
                ? EmscriptenDriver.CompileFlags(_model, _cacheDir)
                : ClangDriver.CompileFlags(_model, _toolchain, _config, _cacheDir);

        return PchPlanner.Plan(_model, _toolchain, baseFlags, checker, hasModules);
    }

    // ─── Module jobs ──────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a map of logical module name → BMI file path from a list of module compile jobs.
    /// The job's source file is scanned to find the provided module name.
    /// </summary>
    public static IReadOnlyDictionary<string, string> ExtractBmiPaths(
        IReadOnlyList<CompileJob> moduleJobs)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var job in moduleJobs)
        {
            var dep = ModuleScanner.ScanByText(job.SourceFile);
            if (dep.Provides is not null)
                map[dep.Provides] = job.OutputFile;
        }
        return map;
    }

    /// <summary>
    /// Scan and sort module files, then produce BMI compile jobs in dependency order.
    /// </summary>
    public async Task<IReadOnlyList<CompileJob>> PlanModuleJobsAsync(
        string? compilerPathOverride = null,
        CancellationToken ct = default)
    {
        var modIncludes = _model.Modules.Where(s =>  s.IsInclude).Select(s => s.Glob).ToList();
        var modExcludes = _model.Modules.Where(s => !s.IsInclude).Select(s => s.Glob).ToList();
        var moduleFiles = GlobExpander.Expand(_model.ProjectDir, modIncludes, modExcludes);

        if (moduleFiles.Count == 0) return Array.Empty<CompileJob>();

        var compilerPath = compilerPathOverride ?? _toolchain.CompilerPath;

        IReadOnlyList<string> compileFlags = _toolchain.Kind == CompilerKind.Msvc
            ? MsvcDriver.CompileFlags(_model, _toolchain, _config, _cacheDir)
            : ClangDriver.CompileFlags(_model, _toolchain, _config, _cacheDir);

        // Scan all module files for dependencies
        var deps = await ModuleScanner.ScanAllAsync(
            compilerPath, _toolchain.Kind, moduleFiles, compileFlags, ct);

        // Topological sort
        var sorted = ModuleSorter.Sort(deps);

        // Build BMI compile jobs
        var jobs = ModuleSorter.BuildModuleJobs(sorted, _cacheDir, _toolchain.Kind, compileFlags);

        // Write module-map.json if export-map is enabled (note: jobs not yet compiled,
        // so WriteModuleMap is called after successful compilation in BuildCommand)
        return jobs;
    }

    // ─── Module map ───────────────────────────────────────────────────────────

    /// <summary>
    /// Write module-map.json to the BMI directory after module jobs have been compiled.
    /// Only written when <see cref="FlatProjectModel.ModuleExportMap"/> is true.
    /// </summary>
    public void WriteModuleMap(IReadOnlyList<CompileJob> moduleJobs)
    {
        if (!_model.ModuleExportMap) return;
        if (moduleJobs.Count == 0) return;

        var bmiDir = Path.Combine(_cacheDir, "bmi");
        ModuleMapWriter.Write(moduleJobs, _targetId, _config, bmiDir);
    }

    // ─── Link job ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Plan the link job (returns null for header-only projects).
    /// Uses the appropriate linker: MsvcLinker, AppleLinker, LldLinker, or EmscriptenDriver.
    /// For static libraries on non-MSVC targets, uses 'ar rcs'.
    /// </summary>
    public LinkJob? PlanLinkJob(IEnumerable<string> objFiles)
    {
        if (_model.Type == ProjectType.HeaderOnly) return null;

        var outDir = Path.Combine(_model.ProjectDir, ".dotori-cache",
            "bin", $"{_targetId}-{_config.ToLower()}");
        Directory.CreateDirectory(outDir);

        var outName = GetOutputName();
        var outFile = Path.Combine(outDir, outName);

        // Static library on non-MSVC: use ar rcs
        if (_model.Type == ProjectType.StaticLibrary && _toolchain.Kind != CompilerKind.Msvc)
        {
            var args = new List<string> { "rcs", $"\"{outFile}\"" };
            foreach (var obj in objFiles) args.Add($"\"{obj}\"");
            return new LinkJob
            {
                InputFiles = objFiles.ToArray(),
                OutputFile = outFile,
                Args       = args.ToArray(),
            };
        }

        if (_toolchain.Kind == CompilerKind.Msvc)
        {
            var flags = MsvcLinker.LinkFlags(_model, _toolchain, _config, outFile);
            return MsvcLinker.MakeLinkJob(objFiles, outFile, flags);
        }
        else if (_toolchain.Kind == CompilerKind.Emscripten)
        {
            var flags = EmscriptenDriver.LinkFlags(_model, outFile);
            return EmscriptenDriver.MakeLinkJob(objFiles, outFile, flags);
        }
        else if (AppleLinker.IsAppleTarget(_toolchain))
        {
            var flags = AppleLinker.LinkFlags(_model, _toolchain, outFile);
            return AppleLinker.MakeLinkJob(objFiles, outFile, flags);
        }
        else
        {
            // Linux, Android, WASM-bare: use LLD via clang++
            var flags = LldLinker.LinkFlags(_model, _toolchain, outFile);
            return LldLinker.MakeLinkJob(objFiles, outFile, flags);
        }
    }

    /// <summary>
    /// Returns the correct linker path for this project's link step.
    /// Static libraries on non-MSVC use 'ar'; others use the toolchain linker.
    /// </summary>
    public string GetLinkerPath()
    {
        if (_model.Type == ProjectType.StaticLibrary && _toolchain.Kind != CompilerKind.Msvc)
            return FindAr();
        return _toolchain.LinkerPath;
    }

    private static string FindAr()
    {
        // Try llvm-ar first (alongside clang), then plain ar
        foreach (var candidate in new[] { "llvm-ar", "ar" })
        {
            foreach (var dir in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator))
            {
                var full = Path.Combine(dir, candidate);
                if (File.Exists(full)) return full;
                if (OperatingSystem.IsWindows() && File.Exists(full + ".exe")) return full + ".exe";
            }
        }
        return "ar"; // fallback: assume ar is in PATH
    }

    // ─── Single-file build support ────────────────────────────────────────────

    /// <summary>
    /// Find the compile job for a specific source file.
    /// If Unity Build is enabled and <paramref name="noUnity"/> is false,
    /// returns the compile job for the unity batch file containing the specified file.
    /// For module files (.cppm/.ixx), returns a BMI-only job (no link implied).
    /// </summary>
    /// <param name="targetFile">Absolute path to the target source file.</param>
    /// <param name="noUnity">If true, skip Unity batch lookup and compile the file directly.</param>
    /// <returns>The matching compile job, or null if the file is not in project sources.</returns>
    public CompileJob? PlanSingleFileJob(string targetFile, bool noUnity = false)
    {
        bool isModule = IsModuleFile(targetFile);

        if (isModule)
        {
            // Module file: return a BMI-only compile job
            IReadOnlyList<string> flags = _toolchain.Kind == CompilerKind.Msvc
                ? MsvcDriver.CompileFlags(_model, _toolchain, _config, _cacheDir)
                : ClangDriver.CompileFlags(_model, _toolchain, _config, _cacheDir);

            return _toolchain.Kind == CompilerKind.Msvc
                ? MakeMsvcModuleJob(targetFile, flags)
                : ClangDriver.MakeCompileJob(targetFile, _cacheDir, flags, isModule: true);
        }

        // Check if file is in project sources
        if (!IsInSources(targetFile)) return null;

        if (!noUnity && _model.UnityBuild?.Enabled == true)
        {
            // Find the unity batch that contains this file
            var unityFile = FindUnityBatchForFile(targetFile);
            if (unityFile is not null)
                return BuildSingleCompileJob(unityFile);
        }

        return BuildSingleCompileJob(targetFile);
    }

    private static bool IsModuleFile(string file)
    {
        var ext = Path.GetExtension(file);
        return ext.Equals(".cppm", StringComparison.OrdinalIgnoreCase) ||
               ext.Equals(".ixx",  StringComparison.OrdinalIgnoreCase);
    }

    private bool IsInSources(string targetFile)
    {
        var includes = _model.Sources.Where(s =>  s.IsInclude).Select(s => s.Glob).ToList();
        var excludes = _model.Sources.Where(s => !s.IsInclude).Select(s => s.Glob).ToList();
        var allSources = GlobExpander.Expand(_model.ProjectDir, includes, excludes);

        var modIncludes = _model.Modules.Where(s =>  s.IsInclude).Select(s => s.Glob).ToList();
        var modExcludes = _model.Modules.Where(s => !s.IsInclude).Select(s => s.Glob).ToList();
        var moduleSources = GlobExpander.Expand(_model.ProjectDir, modIncludes, modExcludes);

        return allSources.Concat(moduleSources)
            .Any(f => string.Equals(f, targetFile, StringComparison.OrdinalIgnoreCase));
    }

    private string? FindUnityBatchForFile(string targetFile)
    {
        var includes = _model.Sources.Where(s =>  s.IsInclude).Select(s => s.Glob).ToList();
        var excludes = _model.Sources.Where(s => !s.IsInclude).Select(s => s.Glob).ToList();
        var allSources = GlobExpander.Expand(_model.ProjectDir, includes, excludes);

        var modIncludes = _model.Modules.Where(s =>  s.IsInclude).Select(s => s.Glob).ToList();
        var modExcludes = _model.Modules.Where(s => !s.IsInclude).Select(s => s.Glob).ToList();
        var moduleSources = GlobExpander.Expand(_model.ProjectDir, modIncludes, modExcludes);

        var unityDir  = Path.Combine(_model.ProjectDir, ".dotori-cache", "unity");
        var exclude   = _model.UnityBuild!.Exclude;
        int batchSize = _model.UnityBuild.BatchSize > 0 ? _model.UnityBuild.BatchSize : 8;

        var (unityFiles, _) = UnityBatcher.CreateBatches(
            allSources, moduleSources, exclude, batchSize, unityDir);

        var normalizedTarget = targetFile.Replace('\\', '/');
        foreach (var unityFile in unityFiles)
        {
            if (!File.Exists(unityFile)) continue;
            var content = File.ReadAllText(unityFile);
            if (content.Contains(normalizedTarget, StringComparison.OrdinalIgnoreCase))
                return unityFile;
        }

        return null;
    }

    private CompileJob BuildSingleCompileJob(string sourceFile)
    {
        if (_toolchain.Kind == CompilerKind.Msvc)
        {
            var flags = MsvcDriver.CompileFlags(_model, _toolchain, _config, _cacheDir);
            return MsvcDriver.MakeCompileJob(sourceFile, _cacheDir, flags);
        }
        else if (_toolchain.Kind == CompilerKind.Emscripten)
        {
            var flags = EmscriptenDriver.CompileFlags(_model, _cacheDir);
            return EmscriptenDriver.MakeCompileJob(sourceFile, _cacheDir, flags);
        }
        else
        {
            var flags = ClangDriver.CompileFlags(_model, _toolchain, _config, _cacheDir);
            return ClangDriver.MakeCompileJob(sourceFile, _cacheDir, flags);
        }
    }

    private CompileJob MakeMsvcModuleJob(string sourceFile, IReadOnlyList<string> compileFlags)
    {
        var bmiDir = Path.Combine(_model.ProjectDir, ".dotori-cache", "bmi");
        Directory.CreateDirectory(bmiDir);

        var bmiFile = Path.Combine(bmiDir,
            Path.GetFileNameWithoutExtension(sourceFile) + ".ifc");

        var args = new List<string>(compileFlags);
        args.RemoveAll(f => f.StartsWith("/Fo"));
        args.Remove("/c");
        args.Add("/interface");
        args.Add("/TP");
        args.Add($"\"{sourceFile}\"");
        args.Add($"/ifcOutput \"{bmiFile}\"");

        return new CompileJob
        {
            SourceFile = sourceFile,
            OutputFile = bmiFile,
            Args       = args.ToArray(),
        };
    }

    // ─── Windows RC / Manifest ────────────────────────────────────────────────

    /// <summary>
    /// Plan compile jobs for Windows resource files (.rc → .res via rc.exe).
    /// Returns an empty list if not on MSVC or if no resources are declared.
    /// The returned jobs should be executed with rc.exe
    /// (<see cref="MsvcPaths.RcPath"/>) as the tool path.
    /// </summary>
    public IReadOnlyList<CompileJob> PlanRcJobs()
    {
        if (_model.Resources.Count == 0)           return [];
        if (_toolchain.Kind != CompilerKind.Msvc)  return [];
        if (_toolchain.Msvc?.RcPath is null)       return [];

        var jobs = new List<CompileJob>();

        foreach (var rcPath in _model.Resources)
        {
            var absRc = Path.IsPathRooted(rcPath)
                ? rcPath
                : Path.GetFullPath(Path.Combine(_model.ProjectDir, rcPath));

            if (!File.Exists(absRc))
            {
                Console.Error.WriteLine($"Warning: resource file not found, skipping: {absRc}");
                continue;
            }

            var resFile = Path.Combine(_cacheDir,
                Path.GetFileNameWithoutExtension(absRc) + ".res");

            var args = new List<string> { "/nologo" };

            // Pass project include paths so .rc files can use #include
            foreach (var h in _model.Headers)
            {
                var absPath = Path.IsPathRooted(h.Path)
                    ? h.Path
                    : Path.GetFullPath(Path.Combine(_model.ProjectDir, h.Path));
                args.Add($"/I\"{absPath}\"");
            }

            // Pass defines (rc.exe supports /D like cl.exe)
            foreach (var d in _model.Defines)
                args.Add($"/D{d}");

            args.Add($"/fo\"{resFile}\"");
            args.Add($"\"{absRc}\"");

            jobs.Add(new CompileJob
            {
                SourceFile = absRc,
                OutputFile = resFile,
                Args       = args.ToArray(),
            });
        }

        return jobs;
    }

    /// <summary>
    /// Embed a .manifest file into the linked binary using mt.exe.
    /// No-op if no manifest is declared, toolchain is not MSVC, or mt.exe is not found.
    /// </summary>
    /// <param name="outputFile">Absolute path to the linked binary (.exe or .dll).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True on success or when embedding is not applicable.</returns>
    public async Task<bool> EmbedManifestAsync(string outputFile, CancellationToken ct = default)
    {
        if (_model.Manifest is null)                      return true;
        if (_toolchain.Kind != CompilerKind.Msvc)         return true;
        if (_model.Type    == ProjectType.StaticLibrary)  return true;

        if (_toolchain.Msvc?.MtPath is not { } mtPath)
        {
            Console.Error.WriteLine("Warning: mt.exe not found, skipping manifest embedding.");
            return true;
        }

        var absManifest = Path.IsPathRooted(_model.Manifest)
            ? _model.Manifest
            : Path.GetFullPath(Path.Combine(_model.ProjectDir, _model.Manifest));

        if (!File.Exists(absManifest))
        {
            Console.Error.WriteLine($"Warning: manifest file not found, skipping: {absManifest}");
            return true;
        }

        // Resource ID: 1 = executable, 2 = DLL
        var resourceId = _model.Type == ProjectType.SharedLibrary ? "2" : "1";
        var args = $"-nologo -manifest \"{absManifest}\" -outputresource:\"{outputFile};{resourceId}\"";

        var psi = new System.Diagnostics.ProcessStartInfo(mtPath, args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
            CreateNoWindow         = true,
        };

        using var proc = System.Diagnostics.Process.Start(psi)!;
        var stdoutTask = proc.StandardOutput.ReadToEndAsync(ct);
        var stderrTask = proc.StandardError.ReadToEndAsync(ct);
        await proc.WaitForExitAsync(ct);

        var stdout = await stdoutTask;
        var stderr = await stderrTask;
        if (!string.IsNullOrWhiteSpace(stdout)) Console.WriteLine(stdout.TrimEnd());
        if (!string.IsNullOrWhiteSpace(stderr)) Console.Error.WriteLine(stderr.TrimEnd());

        return proc.ExitCode == 0;
    }

    // ─── Artifact copy ────────────────────────────────────────────────────────

    /// <summary>
    /// Copy the linked artifact(s) to user-specified output directories
    /// defined in the <c>output { }</c> block of the .dotori file.
    /// Does nothing if <see cref="FlatProjectModel.Output"/> is null.
    /// </summary>
    /// <param name="linkedFile">Absolute path to the primary linked output file.</param>
    public void CopyArtifacts(string linkedFile)
    {
        if (_model.Output is null) return;
        if (!File.Exists(linkedFile)) return;

        bool isWindows = _targetId.StartsWith("windows", StringComparison.OrdinalIgnoreCase)
                      || _targetId.StartsWith("uwp",     StringComparison.OrdinalIgnoreCase);

        // Determine which output category the linked file belongs to
        switch (_model.Type)
        {
            case ProjectType.Executable:
                CopyTo(linkedFile, _model.Output.Binaries);
                // Windows PDB alongside exe
                if (isWindows) CopySymbols(linkedFile, ".pdb");
                break;

            case ProjectType.SharedLibrary:
                CopyTo(linkedFile, _model.Output.Binaries);
                // Windows import library: same stem + .lib
                if (isWindows)
                {
                    var importLib = Path.ChangeExtension(linkedFile, ".lib");
                    CopyTo(importLib, _model.Output.Libraries);
                    CopySymbols(linkedFile, ".pdb");
                }
                // macOS .dSYM bundle
                if (_targetId.StartsWith("macos", StringComparison.OrdinalIgnoreCase))
                    CopyDsym(linkedFile);
                break;

            case ProjectType.StaticLibrary:
                CopyTo(linkedFile, _model.Output.Libraries);
                break;
        }
    }

    private void CopyTo(string sourceFile, string? destDir)
    {
        if (destDir is null || !File.Exists(sourceFile)) return;

        var absDestDir = Path.IsPathRooted(destDir)
            ? destDir
            : Path.Combine(_model.ProjectDir, destDir);
        Directory.CreateDirectory(absDestDir);

        var dest = Path.Combine(absDestDir, Path.GetFileName(sourceFile));
        File.Copy(sourceFile, dest, overwrite: true);
    }

    private void CopySymbols(string linkedFile, string ext)
    {
        var symFile = Path.ChangeExtension(linkedFile, ext);
        CopyTo(symFile, _model.Output?.Symbols);
    }

    private void CopyDsym(string linkedFile)
    {
        if (_model.Output?.Symbols is null) return;
        var dsymBundle = linkedFile + ".dSYM";
        if (!Directory.Exists(dsymBundle)) return;

        var absDestDir = Path.IsPathRooted(_model.Output.Symbols)
            ? _model.Output.Symbols
            : Path.Combine(_model.ProjectDir, _model.Output.Symbols);
        Directory.CreateDirectory(absDestDir);

        CopyDirectoryRecursive(dsymBundle, Path.Combine(absDestDir, Path.GetFileName(dsymBundle)));
    }

    private static void CopyDirectoryRecursive(string src, string dest)
    {
        Directory.CreateDirectory(dest);
        foreach (var file in Directory.GetFiles(src))
            File.Copy(file, Path.Combine(dest, Path.GetFileName(file)), overwrite: true);
        foreach (var dir in Directory.GetDirectories(src))
            CopyDirectoryRecursive(dir, Path.Combine(dest, Path.GetFileName(dir)));
    }

    // ─── Output name ──────────────────────────────────────────────────────────

    private string GetOutputName()
    {
        bool isWindows = _targetId.StartsWith("windows", StringComparison.OrdinalIgnoreCase)
                      || _targetId.StartsWith("uwp",     StringComparison.OrdinalIgnoreCase);
        bool isMacos   = _targetId.StartsWith("macos",   StringComparison.OrdinalIgnoreCase);
        bool isWasm    = _targetId.StartsWith("wasm",    StringComparison.OrdinalIgnoreCase);

        return _model.Type switch
        {
            ProjectType.Executable     => isWindows ? $"{_model.Name}.exe"
                                        : isWasm    ? $"{_model.Name}.wasm"
                                        : _model.Name,
            ProjectType.StaticLibrary  => isWindows ? $"{_model.Name}.lib"
                                        : $"lib{_model.Name}.a",
            ProjectType.SharedLibrary  => isWindows ? $"{_model.Name}.dll"
                                        : isMacos   ? $"lib{_model.Name}.dylib"
                                        : $"lib{_model.Name}.so",
            ProjectType.HeaderOnly     => string.Empty,
            _                          => _model.Name,
        };
    }
}
