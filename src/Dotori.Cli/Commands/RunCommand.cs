using System.CommandLine;
using System.Diagnostics;
using Dotori.Core.Build;
using Dotori.Core.Executor;
using Dotori.Core.Graph;

namespace Dotori.Cli.Commands;

internal static class RunCommandFactory
{
    public static Command Create()
    {
        var command = new Command("run", "Build and run the project");

        var projectOption = new Option<string?>("--project") { Description = "Path to .dotori file or directory" };
        var releaseOption = new Option<bool>("--release")    { Description = "Run in Release configuration" };
        var targetOption  = new Option<string?>("--target")  { Description = "Build target (e.g. macos-arm64)" };
        var argsOption    = new Option<string[]>("--")       { Description = "Arguments to pass to the executable", AllowMultipleArgumentsPerToken = true };

        command.Add(projectOption);
        command.Add(releaseOption);
        command.Add(targetOption);
        command.Add(argsOption);

        command.SetAction(async (parseResult, ct) =>
        {
            var projectArg = parseResult.GetValue(projectOption);
            var release    = parseResult.GetValue(releaseOption);
            var targetArg  = parseResult.GetValue(targetOption);
            var runArgs    = parseResult.GetValue(argsOption) ?? Array.Empty<string>();

            var config   = release ? "release" : "debug";
            var targetId = BuildContext.ResolveTargetId(targetArg);

            // Resolve project
            var paths = BuildContext.ResolveProjectPaths(projectArg, buildAll: false);
            if (paths.Count == 0) return 1;

            // Only run executable projects
            var dag = BuildContext.BuildDag(paths);
            if (dag is null) return 1;

            var buildOrder = ProjectDagBuilder.BuildLevels(dag);
            var toolchain  = BuildContext.DetectToolchain(targetId, compiler: null);
            if (toolchain is null) return 1;

            var ctx = BuildContext.MakeTargetContext(targetId, config);

            // Find the executable project (last in build order that is executable)
            string? executablePath = null;

            var executor = new LocalExecutor(0);
            var builtLibraries = new System.Collections.Concurrent.ConcurrentDictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var level in buildOrder)
            {
                foreach (var node in level)
                {
                    var model = BuildContext.FlattenProject(node.DotoriPath, ctx, node);
                    if (model is null) return 1;

                    Console.WriteLine($"  Building {model.Name}...");

                    using var checker = new IncrementalChecker(model.ProjectDir);
                    var planner = new BuildPlanner(model, toolchain, config, targetId);

                    var linkOutDir = Path.Combine(model.ProjectDir, ".dotori-cache",
                        "bin", $"{targetId}-{config.ToLower()}");

                    // pre-build scripts
                    if (model.PreBuildCommands.Count > 0)
                    {
                        var preCode = await BuildContext.RunScriptsAsync(
                            model.PreBuildCommands, model.ProjectDir,
                            targetId, config, linkOutDir, ct);
                        if (preCode != 0) return preCode;
                    }

                    // PCH first
                    var pchPlan = planner.PlanPch(checker);
                    if (pchPlan?.BuildJob is not null)
                    {
                        var pchResults = await executor.RunCompileJobsAsync(
                            toolchain.CompilerPath, new[] { pchPlan.BuildJob }, ct);
                        foreach (var r in pchResults)
                        {
                            if (!string.IsNullOrWhiteSpace(r.Stdout)) Console.Write(r.Stdout);
                            if (!string.IsNullOrWhiteSpace(r.Stderr)) Console.Error.Write(r.Stderr);
                            if (!r.Success)
                            {
                                Console.Error.WriteLine($"Build failed (exit {r.ExitCode})");
                                return 1;
                            }
                        }
                        checker.Record(pchPlan.BuildJob.SourceFile);
                    }

                    // Modules next (order-sensitive BMI generation)
                    var moduleJobs = await planner.PlanModuleJobsAsync(ct: ct);
                    IReadOnlyDictionary<string, string>? bmiPaths = null;
                    if (moduleJobs.Count > 0)
                    {
                        foreach (var bmiJob in moduleJobs)
                        {
                            var bmiResults = await executor.RunCompileJobsAsync(
                                toolchain.CompilerPath, new[] { bmiJob }, ct);
                            foreach (var r in bmiResults)
                            {
                                if (!string.IsNullOrWhiteSpace(r.Stdout)) Console.Write(r.Stdout);
                                if (!string.IsNullOrWhiteSpace(r.Stderr)) Console.Error.Write(r.Stderr);
                                if (!r.Success)
                                {
                                    Console.Error.WriteLine($"Build failed (exit {r.ExitCode})");
                                    return 1;
                                }
                            }
                        }
                        bmiPaths = BuildPlanner.ExtractBmiPaths(moduleJobs);
                        planner.WriteModuleMap(moduleJobs);
                    }

                    var compileJobs = planner.PlanCompileJobs(checker, pchPlan: pchPlan, bmiPaths: bmiPaths);
                    if (compileJobs.Count > 0)
                    {
                        var results = await executor.RunCompileJobsAsync(
                            toolchain.CompilerPath, compileJobs, ct);
                        foreach (var r in results)
                        {
                            if (!string.IsNullOrWhiteSpace(r.Stdout)) Console.Write(r.Stdout);
                            if (!string.IsNullOrWhiteSpace(r.Stderr)) Console.Error.Write(r.Stderr);
                            if (!r.Success)
                            {
                                Console.Error.WriteLine($"Build failed (exit {r.ExitCode})");
                                return 1;
                            }
                        }
                        foreach (var j in compileJobs) checker.Record(j.SourceFile);
                    }

                    // Collect transitive dependency library paths
                    var depLibs = CollectDepLibs(node, builtLibraries);

                    var isStaticLib = model.Type == Dotori.Core.Parsing.ProjectType.StaticLibrary;
                    // For Clang modules: .pcm files must also be linked
                    var modulePcmFiles = (bmiPaths is not null && toolchain.Kind != Dotori.Core.Toolchain.CompilerKind.Msvc)
                        ? bmiPaths.Values
                        : Enumerable.Empty<string>();
                    var objFiles = compileJobs.Count > 0
                        ? compileJobs.Select(j => j.OutputFile)
                            .Concat(modulePcmFiles)
                            .Concat(isStaticLib ? Enumerable.Empty<string>() : depLibs)
                            .ToList()
                        : Directory.Exists(Path.Combine(model.ProjectDir, ".dotori-cache", "obj",
                              $"{targetId}-{config.ToLower()}"))
                            ? Directory.GetFiles(Path.Combine(model.ProjectDir, ".dotori-cache", "obj",
                              $"{targetId}-{config.ToLower()}"), "*.o")
                              .Concat(Directory.GetFiles(Path.Combine(model.ProjectDir,
                                  ".dotori-cache", "obj", $"{targetId}-{config.ToLower()}"), "*.obj"))
                              .Concat(modulePcmFiles)
                              .Concat(isStaticLib ? Enumerable.Empty<string>() : depLibs)
                              .ToList()
                            : new List<string>();

                    var linkJob = planner.PlanLinkJob(objFiles);
                    if (linkJob is not null)
                    {
                        var linkResult = await executor.RunLinkJobAsync(planner.GetLinkerPath(), linkJob, ct);
                        if (!linkResult.Success)
                        {
                            Console.Error.WriteLine(linkResult.Stderr);
                            return 1;
                        }
                        Console.WriteLine($"  Linked: {linkJob.OutputFile}");
                        builtLibraries[node.DotoriPath] = linkJob.OutputFile;

                        // Copy artifacts to user-specified output directories
                        planner.CopyArtifacts(linkJob.OutputFile);

                        // post-build scripts
                        if (model.PostBuildCommands.Count > 0)
                        {
                            var postCode = await BuildContext.RunScriptsAsync(
                                model.PostBuildCommands, model.ProjectDir,
                                targetId, config, linkOutDir, ct);
                            if (postCode != 0) return postCode;
                        }

                        // Track the last linked executable
                        if (model.Type == Dotori.Core.Parsing.ProjectType.Executable)
                            executablePath = linkJob.OutputFile;
                    }
                }
            }

            if (executablePath is null)
            {
                Console.Error.WriteLine("Error: No executable project found to run.");
                return 1;
            }

            // Run the executable
            Console.WriteLine();
            var psi = new ProcessStartInfo(executablePath, string.Join(" ", runArgs))
            {
                UseShellExecute = false,
            };

            try
            {
                var proc = Process.Start(psi);
                if (proc is null) { Console.Error.WriteLine($"Failed to start '{executablePath}'"); return 1; }
                await proc.WaitForExitAsync(ct);
                return proc.ExitCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error running '{executablePath}': {ex.Message}");
                return 1;
            }
        });

        return command;
    }

    private static IReadOnlyList<string> CollectDepLibs(
        Dotori.Core.Graph.ProjectNode node,
        System.Collections.Concurrent.ConcurrentDictionary<string, string> builtLibraries)
    {
        var libs = new List<string>();
        CollectDepLibsRecursive(node, builtLibraries, libs,
            new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        return libs;
    }

    private static void CollectDepLibsRecursive(
        Dotori.Core.Graph.ProjectNode node,
        System.Collections.Concurrent.ConcurrentDictionary<string, string> builtLibraries,
        List<string> libs,
        HashSet<string> visited)
    {
        foreach (var dep in node.Dependencies)
        {
            if (!visited.Add(dep.DotoriPath)) continue;
            if (builtLibraries.TryGetValue(dep.DotoriPath, out var libPath))
                libs.Add(libPath);
            CollectDepLibsRecursive(dep, builtLibraries, libs, visited);
        }
    }
}
