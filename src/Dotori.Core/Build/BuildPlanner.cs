using Dotori.Core.Model;
using Dotori.Core.Parsing;
using Dotori.Core.Toolchain;

namespace Dotori.Core.Build;

/// <summary>
/// The primary entry point for planning a build.
/// Given a flat project model and toolchain, produces all compile + link jobs.
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

    /// <summary>
    /// Plan and return all compile jobs for this project.
    /// Handles unity batching if enabled.
    /// </summary>
    public IReadOnlyList<CompileJob> PlanCompileJobs(
        IncrementalChecker? checker = null)
    {
        // Expand source globs
        var includes = _model.Sources.Where(s => s.IsInclude).Select(s => s.Glob).ToList();
        var excludes = _model.Sources.Where(s => !s.IsInclude).Select(s => s.Glob).ToList();
        var allSources = GlobExpander.Expand(_model.ProjectDir, includes, excludes);

        // Expand module globs
        var modIncludes = _model.Modules.Where(s => s.IsInclude).Select(s => s.Glob).ToList();
        var modExcludes = _model.Modules.Where(s => !s.IsInclude).Select(s => s.Glob).ToList();
        var moduleSources = GlobExpander.Expand(_model.ProjectDir, modIncludes, modExcludes);

        List<string> filesToCompile;
        string objDir = _cacheDir;

        if (_model.UnityBuild?.Enabled == true)
        {
            var unityDir = Path.Combine(_model.ProjectDir, ".dotori-cache", "unity");
            var exclude = _model.UnityBuild.Exclude;
            int batchSize = _model.UnityBuild.BatchSize > 0 ? _model.UnityBuild.BatchSize : 8;

            var (unityFiles, nonUnity) = UnityBatcher.CreateBatches(
                allSources, moduleSources, exclude, batchSize, unityDir);

            filesToCompile = new List<string>(unityFiles);
            filesToCompile.AddRange(nonUnity);
        }
        else
        {
            filesToCompile = new List<string>(allSources);
        }

        // Generate compile jobs
        var jobs = new List<CompileJob>();

        if (_toolchain.Kind == CompilerKind.Msvc)
        {
            var flags = MsvcDriver.CompileFlags(_model, _toolchain, _config, _cacheDir);
            foreach (var src in filesToCompile)
            {
                if (checker is not null && !checker.IsChanged(src)) continue;
                jobs.Add(MsvcDriver.MakeCompileJob(src, objDir, flags));
            }
        }
        else if (_toolchain.Kind == CompilerKind.Emscripten)
        {
            var flags = EmscriptenDriver.CompileFlags(_model, _cacheDir);
            foreach (var src in filesToCompile)
            {
                if (checker is not null && !checker.IsChanged(src)) continue;
                jobs.Add(EmscriptenDriver.MakeCompileJob(src, objDir, flags));
            }
        }
        else  // Clang
        {
            var flags = ClangDriver.CompileFlags(_model, _toolchain, _config, _cacheDir);
            foreach (var src in filesToCompile)
            {
                if (checker is not null && !checker.IsChanged(src)) continue;
                jobs.Add(ClangDriver.MakeCompileJob(src, objDir, flags));
            }
        }

        return jobs;
    }

    /// <summary>
    /// Plan the link job (returns null for header-only projects).
    /// <paramref name="objFiles"/> should be all the .obj/.o files from compile jobs.
    /// </summary>
    public LinkJob? PlanLinkJob(IEnumerable<string> objFiles)
    {
        if (_model.Type == ProjectType.HeaderOnly) return null;

        var outDir  = Path.Combine(_model.ProjectDir, ".dotori-cache",
            "bin", $"{_targetId}-{_config.ToLower()}");
        Directory.CreateDirectory(outDir);

        var outName = GetOutputName();
        var outFile = Path.Combine(outDir, outName);

        if (_toolchain.Kind == CompilerKind.Msvc)
        {
            var flags = MsvcDriver.LinkFlags(_model, _toolchain, _config, outFile);
            return MsvcDriver.MakeLinkJob(objFiles, outFile, flags);
        }
        else if (_toolchain.Kind == CompilerKind.Emscripten)
        {
            var flags = EmscriptenDriver.LinkFlags(_model, outFile);
            return EmscriptenDriver.MakeLinkJob(objFiles, outFile, flags);
        }
        else
        {
            var flags = ClangDriver.LinkFlags(_model, _toolchain, outFile);
            return ClangDriver.MakeLinkJob(objFiles, outFile, flags);
        }
    }

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
