using System.CommandLine;
using Dotori.Core.Graph;
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
}
