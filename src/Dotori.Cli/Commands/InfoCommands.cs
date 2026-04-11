using System.CommandLine;
using Dotori.Core.Build;
using Dotori.Core.Graph;
using Dotori.Core.Model;
using Dotori.Core.Parsing;
using Dotori.Core.Toolchain;

namespace Dotori.Cli.Commands;

internal static class InfoCommandFactory
{
    public static Command Create()
    {
        var infoCommand = new Command("info", "Information and diagnostic commands");

        infoCommand.Add(CreateGraphCommand());
        infoCommand.Add(CreateCheckCommand());
        infoCommand.Add(CreateTargetsCommand());
        infoCommand.Add(CreateToolchainCommand());
        infoCommand.Add(CreateFeaturesCommand());
        infoCommand.Add(CreateIncludesCommand());

        return infoCommand;
    }

    private static Command CreateGraphCommand()
    {
        var command = new Command("graph", "Print project DAG and dependency graph");

        var projectOption = new Option<string?>("--project") { Description = "Path to .dotori file or directory" };
        command.Add(projectOption);

        command.SetAction((parseResult) =>
        {
            var projectArg = parseResult.GetValue(projectOption);
            var paths = BuildContext.ResolveProjectPaths(projectArg, buildAll: true);
            if (paths.Count == 0) return 1;

            var dag = BuildContext.BuildDag(paths);
            if (dag is null) return 1;

            var order = ProjectDagBuilder.TopologicalSort(dag);
            Console.WriteLine("Project dependency graph (build order):");
            Console.WriteLine();

            foreach (var node in order)
            {
                Console.Write($"  {node.ProjectName}");
                if (node.Dependencies.Count > 0)
                {
                    var deps = string.Join(", ", node.Dependencies.Select(d => d.ProjectName));
                    Console.Write($" → depends on: {deps}");
                }
                Console.WriteLine();
            }

            var levels = ProjectDagBuilder.BuildLevels(dag);
            Console.WriteLine();
            Console.WriteLine("Parallel build levels:");
            for (int i = 0; i < levels.Count; i++)
            {
                var names = string.Join(", ", levels[i].Select(n => n.ProjectName));
                Console.WriteLine($"  Level {i + 1}: {names}");
            }

            return 0;
        });

        return command;
    }

    private static Command CreateCheckCommand()
    {
        var command = new Command("check", "Validate .dotori file");

        var projectOption = new Option<string?>("--project") { Description = "Path to .dotori file or directory" };
        command.Add(projectOption);

        command.SetAction((parseResult) =>
        {
            var projectArg = parseResult.GetValue(projectOption);
            var paths = BuildContext.ResolveProjectPaths(projectArg, buildAll: true);
            if (paths.Count == 0) return 1;

            int errors = 0;
            foreach (var path in paths)
            {
                try
                {
                    var file = DotoriParser.ParseFile(path);
                    Console.WriteLine($"  OK: {path}");
                    if (file.Project is null && file.Package is null)
                    {
                        Console.Error.WriteLine($"  Warning: '{path}' has neither project nor package block.");
                    }
                    if (file.Project is not null)
                    {
                        CheckPortability(file.Project, path);
                    }
                }
                catch (ParseException ex)
                {
                    Console.Error.WriteLine($"  Error in '{path}': {ex.Message}");
                    errors++;
                }
            }

            return errors == 0 ? 0 : 1;
        });

        return command;
    }

    private static readonly HashSet<string> CompilerAtoms =
        new(StringComparer.OrdinalIgnoreCase) { "msvc", "clang", "emscripten" };

    /// <summary>
    /// Warn when compile-flags or link-flags appear at the project top-level
    /// (not inside any condition block), which means they apply to all compilers.
    /// </summary>
    private static void CheckPortability(ProjectDecl project, string path)
    {
        foreach (var item in project.Items)
        {
            if (item is CompileFlagsBlock cf && cf.Values.Count > 0)
            {
                Console.Error.WriteLine(
                    $"  Warning: '{path}' line {cf.Location.Line}: " +
                    "compile-flags used without a compiler condition " +
                    "(e.g. [msvc], [clang]) — may reduce portability");
            }
            else if (item is LinkFlagsBlock lf && lf.Values.Count > 0)
            {
                Console.Error.WriteLine(
                    $"  Warning: '{path}' line {lf.Location.Line}: " +
                    "link-flags used without a compiler condition " +
                    "(e.g. [msvc], [clang]) — may reduce portability");
            }
        }
    }

    private static Command CreateTargetsCommand()
    {
        var command = new Command("targets", "List available build targets");

        command.SetAction((parseResult) =>
        {
            Console.WriteLine("Available build targets on this host:");
            Console.WriteLine();

            var available = ToolchainDetector.DetectAvailable();
            if (available.Count == 0)
            {
                Console.WriteLine("  (none detected — ensure compilers are in PATH)");
                return 0;
            }

            foreach (var target in available)
                Console.WriteLine($"  {target}");

            return 0;
        });

        return command;
    }

    private static Command CreateToolchainCommand()
    {
        var command = new Command("toolchain", "Show detected toolchain information");

        var targetOption = new Option<string?>("--target") { Description = "Target to inspect (default: host)" };
        command.Add(targetOption);

        command.SetAction((parseResult) =>
        {
            var targetArg = parseResult.GetValue(targetOption);
            var targetId  = BuildContext.ResolveTargetId(targetArg);
            var tc        = ToolchainDetector.Detect(targetId);

            if (tc is null)
            {
                Console.Error.WriteLine($"No toolchain detected for target '{targetId}'.");
                return 1;
            }

            Console.WriteLine($"Target:   {targetId}");
            Console.WriteLine($"Kind:     {tc.Kind}");
            Console.WriteLine($"Compiler: {tc.CompilerPath}");
            Console.WriteLine($"Linker:   {tc.LinkerPath}");
            Console.WriteLine($"Triple:   {tc.TargetTriple}");
            if (tc.SysRoot   is not null) Console.WriteLine($"SysRoot:  {tc.SysRoot}");
            if (tc.AppleSdk  is not null) Console.WriteLine($"AppleSDK: {tc.AppleSdk}");
            if (tc.Msvc is { } msvc)
            {
                Console.WriteLine($"VCTools:  {msvc.VcToolsDir}");
                Console.WriteLine($"WinSDK:   {msvc.WinSdkDir} ({msvc.WinSdkVer})");
            }

            return 0;
        });

        return command;
    }

    private static Command CreateFeaturesCommand()
    {
        var command = new Command("features", "Probe C++ feature support for the detected toolchain");

        var targetOption   = new Option<string?>("--target")   { Description = "Target to inspect (default: host)" };
        var compilerOption = new Option<string?>("--compiler") { Description = "Preferred compiler (msvc, clang, or path)" };
        var filterOption   = new Option<string?>("--filter")   { Description = "Show only features whose ID contains this substring" };
        command.Add(targetOption);
        command.Add(compilerOption);
        command.Add(filterOption);

        command.SetAction((parseResult) =>
        {
            var targetArg   = parseResult.GetValue(targetOption);
            var compilerArg = parseResult.GetValue(compilerOption);
            var filterArg   = parseResult.GetValue(filterOption);

            var targetId = BuildContext.ResolveTargetId(targetArg);
            var tc       = ToolchainDetector.Detect(targetId, compilerArg);

            if (tc is null)
            {
                Console.Error.WriteLine($"No toolchain detected for target '{targetId}'.");
                return 1;
            }

            Console.WriteLine($"Probing C++ features for: {tc.CompilerPath}  [{targetId}]");
            Console.WriteLine();

            var features = CxxFeatureProber.KnownFeatures
                .Where(f => filterArg is null ||
                            f.Id.Contains(filterArg, StringComparison.OrdinalIgnoreCase) ||
                            f.Description.Contains(filterArg, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (features.Count == 0)
            {
                Console.Error.WriteLine("No matching features.");
                return 1;
            }

            int labelWidth = features.Max(f => f.Description.Length) + 2;

            foreach (var feature in features)
            {
                Console.Write($"  {feature.Description.PadRight(labelWidth)}");
                Console.Write("... ");

                bool ok = CxxFeatureProber.Probe(tc, feature);

                if (ok)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("supported");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("not supported");
                }
                Console.ResetColor();
            }

            return 0;
        });

        return command;
    }

    private static Command CreateIncludesCommand()
    {
        var command = new Command("includes", "Print the #include tree for project source files");

        var projectOption  = new Option<string?>("--project") { Description = "Path to .dotori file or directory" };
        var fileOption     = new Option<string?>("--file")    { Description = "Analyse only this source file" };
        var noSystemOption = new Option<bool>("--no-system")  { Description = "Omit system includes (<...>) from output" };
        var depthOption    = new Option<int?>("--depth")      { Description = "Maximum include depth to display" };
        command.Add(projectOption);
        command.Add(fileOption);
        command.Add(noSystemOption);
        command.Add(depthOption);

        command.SetAction((parseResult) =>
        {
            var projectArg  = parseResult.GetValue(projectOption);
            var fileArg     = parseResult.GetValue(fileOption);
            var noSystem    = parseResult.GetValue(noSystemOption);
            var depthArg    = parseResult.GetValue(depthOption);
            int maxDepth    = depthArg ?? int.MaxValue;
            bool showSystem = !noSystem;

            // Resolve .dotori path
            var paths = BuildContext.ResolveProjectPaths(projectArg, buildAll: false);
            if (paths.Count == 0) return 1;
            var dotoriPath = paths[0];

            // Parse project
            DotoriFile dotoriFile;
            try { dotoriFile = DotoriParser.ParseFile(dotoriPath); }
            catch (ParseException ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                return 1;
            }

            if (dotoriFile.Project is null)
            {
                Console.Error.WriteLine("Error: No project block found in .dotori file.");
                return 1;
            }

            // Flatten with host context (compiler-independent static analysis)
            var targetId = BuildContext.ResolveTargetId(null);
            var context  = BuildContext.MakeTargetContext(targetId, "debug");
            var model    = ProjectFlattener.Flatten(dotoriFile.Project, dotoriPath, context);

            // Build include search paths from headers block
            var searchPaths = model.Headers
                .Select(h => Path.IsPathRooted(h.Path)
                    ? h.Path
                    : Path.GetFullPath(Path.Combine(model.ProjectDir, h.Path)))
                .ToList();

            // Collect source files
            List<string> sourceFiles;
            if (fileArg is not null)
            {
                var abs = Path.IsPathRooted(fileArg)
                    ? fileArg
                    : Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), fileArg));
                if (!File.Exists(abs))
                {
                    Console.Error.WriteLine($"Error: File not found: {abs}");
                    return 1;
                }
                sourceFiles = [abs];
            }
            else
            {
                var includes = model.Sources.Where(s =>  s.IsInclude).Select(s => s.Glob);
                var excludes = model.Sources.Where(s => !s.IsInclude).Select(s => s.Glob);
                sourceFiles  = GlobExpander.Expand(model.ProjectDir, includes, excludes).ToList();

                if (sourceFiles.Count == 0)
                {
                    Console.WriteLine("No source files found.");
                    return 0;
                }
            }

            // Track nodes already fully printed (to avoid re-expanding duplicates)
            var printed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < sourceFiles.Count; i++)
            {
                var src  = sourceFiles[i];
                var tree = IncludeScanner.BuildTree(src, searchPaths, showSystem, maxDepth);

                // Print root
                var relRoot = Path.GetRelativePath(model.ProjectDir, src);
                Console.WriteLine(relRoot);
                printed.Add(src);

                PrintChildren(tree.Children, prefix: "", model.ProjectDir, printed);

                if (i < sourceFiles.Count - 1)
                    Console.WriteLine();
            }

            return 0;
        });

        return command;
    }

    private static void PrintChildren(
        IReadOnlyList<IncludeScanner.IncludeNode> children,
        string prefix,
        string projectDir,
        HashSet<string> printed)
    {
        for (int i = 0; i < children.Count; i++)
        {
            var node   = children[i];
            bool isLast = i == children.Count - 1;

            var connector  = isLast ? "└── " : "├── ";
            var childPrefix = isLast ? "    " : "│   ";

            // Build display label
            string label;
            if (node.IsSystem)
                label = $"<{node.FilePath}>";
            else if (node.IsResolved)
                label = Path.GetRelativePath(projectDir, node.FilePath);
            else
                label = node.FilePath;

            // Suffix annotations
            string suffix = "";
            if (!node.IsResolved)
                suffix = "  [not found]";
            else if (node.IsResolved && printed.Contains(node.FilePath))
                suffix = "  [↑ already shown]";

            Console.WriteLine($"{prefix}{connector}{label}{suffix}");

            // Recurse only if resolved and not already expanded
            if (node.IsResolved && !string.IsNullOrEmpty(node.FilePath) && !printed.Contains(node.FilePath))
            {
                printed.Add(node.FilePath);
                PrintChildren(node.Children, prefix + childPrefix, projectDir, printed);
            }
        }
    }
}
