using System.CommandLine;
using Dotori.Core.Generators;
using Dotori.Core.Graph;

namespace Dotori.Cli.Commands;

internal static class ExportBuildSystemCommandFactory
{
    private static readonly Dictionary<string, IBuildSystemGenerator> Generators = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        ["cmake"]    = new CMakeGenerator(),
        ["meson"]    = new MesonGenerator(),
        ["vcxproj"]  = new VcxprojGenerator(),
        ["pbxproj"]  = new PbxprojGenerator(),
        ["ninja"]    = new NinjaGenerator(),
        ["makefile"] = new MakefileGenerator(),
    };

    public static Command Create()
    {
        var command = new Command(
            "build-system",
            "Export project as a build system file (cmake, meson, vcxproj, pbxproj, ninja, makefile)");

        var formatOption = new Option<string>("--format")
        {
            Description        = "Output format: cmake (default), meson, vcxproj, pbxproj, ninja, makefile",
            DefaultValueFactory = _ => "cmake",
        };
        var projectOption     = new Option<string?>("--project")      { Description = "Path to .dotori file or directory" };
        var outputOption      = new Option<string?>("--output")       { Description = "Output directory (default: project directory)" };
        var configOption      = new Option<string>("--config")
        {
            Description         = "Build configuration: debug, release, or both (default: both)",
            DefaultValueFactory = _ => "both",
        };
        var targetOption      = new Option<string?>("--target")       { Description = "Build target triple (e.g. macos-arm64, windows-x64)" };
        var compilerOption    = new Option<string?>("--compiler")     { Description = "Compiler to use (msvc, clang)" };
        var runtimeLinkOption = new Option<string?>("--runtime-link") { Description = "Runtime link mode (static, dynamic)" };
        var libcOption        = new Option<string?>("--libc")         { Description = "C runtime library (glibc, musl)" };
        var stdlibOption      = new Option<string?>("--stdlib")       { Description = "C++ standard library (libc++, libstdc++)" };

        command.Add(formatOption);
        command.Add(projectOption);
        command.Add(outputOption);
        command.Add(configOption);
        command.Add(targetOption);
        command.Add(compilerOption);
        command.Add(runtimeLinkOption);
        command.Add(libcOption);
        command.Add(stdlibOption);

        command.SetAction((parseResult, ct) =>
        {
            var format      = parseResult.GetValue(formatOption)!;
            var projectArg  = parseResult.GetValue(projectOption);
            var outputArg   = parseResult.GetValue(outputOption);
            var config      = parseResult.GetValue(configOption)!;
            var targetArg   = parseResult.GetValue(targetOption);
            var compiler    = parseResult.GetValue(compilerOption);
            var runtimeLink = parseResult.GetValue(runtimeLinkOption);
            var libc        = parseResult.GetValue(libcOption);
            var stdlib      = parseResult.GetValue(stdlibOption);

            // Validate format
            if (!Generators.TryGetValue(format, out var generator))
            {
                Console.Error.WriteLine(
                    $"error: unknown format '{format}'. " +
                    $"Available: {string.Join(", ", Generators.Keys)}");
                return Task.FromResult(1);
            }

            // Validate config
            if (config is not ("debug" or "release" or "both"))
            {
                Console.Error.WriteLine(
                    $"error: unknown config '{config}'. Expected: debug, release, or both.");
                return Task.FromResult(1);
            }

            var targetId = BuildContext.ResolveTargetId(targetArg);

            // Resolve project paths
            var paths = BuildContext.ResolveProjectPaths(projectArg, buildAll: false);
            if (paths.Count == 0) return Task.FromResult(1);

            var ctx = BuildContext.MakeTargetContext(targetId, config == "both" ? "debug" : config,
                runtimeLink, libc, stdlib);

            // Build DAG
            var dag = BuildContext.BuildDag(paths);
            if (dag is null) return Task.FromResult(1);

            var buildOrder = ProjectDagBuilder.BuildLevels(dag);
            var written = 0;

            foreach (var level in buildOrder)
            {
                foreach (var node in level)
                {
                    var model = BuildContext.FlattenProject(node.DotoriPath, ctx, node);
                    if (model is null)
                    {
                        Console.Error.WriteLine(
                            $"Warning: Could not flatten '{node.DotoriPath}', skipping.");
                        continue;
                    }

                    var files = generator.Generate(model, config, targetId);

                    // Determine output directory
                    var outDir = outputArg is not null
                        ? (Path.IsPathRooted(outputArg) ? outputArg : Path.GetFullPath(outputArg))
                        : model.ProjectDir;

                    foreach (var (relativePath, content) in files)
                    {
                        var fullPath = Path.Combine(outDir, relativePath);
                        var fileDir = Path.GetDirectoryName(fullPath);
                        if (!string.IsNullOrEmpty(fileDir))
                            Directory.CreateDirectory(fileDir);

                        File.WriteAllText(fullPath, content);
                        Console.WriteLine($"Generated '{fullPath}'");
                        written++;
                    }
                }
            }

            if (written == 0)
                Console.Error.WriteLine("warning: No files were generated.");

            return Task.FromResult(0);
        });

        return command;
    }
}
