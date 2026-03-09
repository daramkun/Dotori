using System.CommandLine;
using Dotori.Core.Build;
using Dotori.Core.Executor;
using Dotori.Core.Graph;

namespace Dotori.Cli.Commands;

internal static class BuildCommandFactory
{
    public static Command Create()
    {
        var command = new Command("build", "Build the project");

        var projectOption     = new Option<string?>("--project")      { Description = "Path to .dotori file or directory" };
        var allOption         = new Option<bool>("--all")             { Description = "Build all projects without prompt" };
        var releaseOption     = new Option<bool>("--release")         { Description = "Build in Release configuration" };
        var targetOption      = new Option<string?>("--target")       { Description = "Build target (e.g. windows-x64, linux-x64)" };
        var compilerOption    = new Option<string?>("--compiler")     { Description = "Compiler to use (msvc, clang)" };
        var runtimeLinkOption = new Option<string?>("--runtime-link") { Description = "Runtime link mode (static, dynamic)" };
        var libcOption        = new Option<string?>("--libc")         { Description = "C runtime library (glibc, musl)" };
        var stdlibOption      = new Option<string?>("--stdlib")       { Description = "C++ standard library (libc++, libstdc++)" };
        var jobsOption        = new Option<int?>("--jobs")            { Description = "Number of parallel jobs" };
        var fileOption        = new Option<string?>("--file")         { Description = "Build a single source file" };
        var noLinkOption      = new Option<bool>("--no-link")         { Description = "Compile only, do not link" };

        command.Add(projectOption);
        command.Add(allOption);
        command.Add(releaseOption);
        command.Add(targetOption);
        command.Add(compilerOption);
        command.Add(runtimeLinkOption);
        command.Add(libcOption);
        command.Add(stdlibOption);
        command.Add(jobsOption);
        command.Add(fileOption);
        command.Add(noLinkOption);

        command.SetAction(async (parseResult, ct) =>
        {
            var projectArg  = parseResult.GetValue(projectOption);
            var buildAll    = parseResult.GetValue(allOption);
            var release     = parseResult.GetValue(releaseOption);
            var targetArg   = parseResult.GetValue(targetOption);
            var compiler    = parseResult.GetValue(compilerOption);
            var runtimeLink = parseResult.GetValue(runtimeLinkOption);
            var libc        = parseResult.GetValue(libcOption);
            var stdlib      = parseResult.GetValue(stdlibOption);
            var jobs        = parseResult.GetValue(jobsOption);
            var fileArg     = parseResult.GetValue(fileOption);
            var noLink      = parseResult.GetValue(noLinkOption);

            var config   = release ? "release" : "debug";
            var targetId = BuildContext.ResolveTargetId(targetArg);

            // Resolve project paths
            var paths = BuildContext.ResolveProjectPaths(projectArg, buildAll);
            if (paths.Count == 0) return 1;

            // Build DAG
            var dag = BuildContext.BuildDag(paths);
            if (dag is null) return 1;

            // Get build order
            var buildOrder = ProjectDagBuilder.BuildLevels(dag);

            // Detect toolchain
            var toolchain = BuildContext.DetectToolchain(targetId, compiler);
            if (toolchain is null) return 1;

            var ctx      = BuildContext.MakeTargetContext(targetId, config, runtimeLink, libc, stdlib);
            var executor = new LocalExecutor(jobs ?? 0);
            int exitCode = 0;

            foreach (var level in buildOrder)
            {
                // Compile all projects in this level in parallel
                var levelTasks = level.Select(async node =>
                {
                    var model = BuildContext.FlattenProject(node.DotoriPath, ctx);
                    if (model is null) return 1;

                    Console.WriteLine($"  Building {model.Name} ({node.DotoriPath})");

                    using var checker = new IncrementalChecker(model.ProjectDir);
                    var planner = new BuildPlanner(model, toolchain, config, targetId);

                    // Single-file mode
                    if (fileArg is not null)
                    {
                        var absFile = Path.GetFullPath(fileArg);
                        var singleJob = planner.PlanCompileJobs(null)
                            .Where(j => string.Equals(j.SourceFile, absFile,
                                StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        if (singleJob.Count == 0)
                        {
                            Console.Error.WriteLine($"Error: '{fileArg}' is not in project sources.");
                            return 1;
                        }

                        var results = await executor.RunCompileJobsAsync(
                            toolchain.CompilerPath, singleJob, ct);
                        return PrintResults(results);
                    }

                    // Normal build
                    var compileJobs = planner.PlanCompileJobs(checker);
                    if (compileJobs.Count == 0)
                    {
                        Console.WriteLine($"  {model.Name}: up to date");
                        return 0;
                    }

                    var compileResults = await executor.RunCompileJobsAsync(
                        toolchain.CompilerPath, compileJobs, ct);

                    int code = PrintResults(compileResults);
                    if (code != 0) return code;

                    foreach (var job in compileJobs)
                        checker.Record(job.SourceFile);

                    if (noLink) return 0;

                    var objFiles = compileJobs.Select(j => j.OutputFile).ToList();
                    var linkJob  = planner.PlanLinkJob(objFiles);
                    if (linkJob is null) return 0;

                    var linkResult = await executor.RunLinkJobAsync(toolchain.LinkerPath, linkJob, ct);
                    if (!linkResult.Success)
                    {
                        Console.Error.WriteLine(linkResult.Stderr);
                        return 1;
                    }

                    Console.WriteLine($"  Linked: {linkJob.OutputFile}");
                    return 0;
                });

                var levelResults = await Task.WhenAll(levelTasks);
                if (levelResults.Any(r => r != 0)) { exitCode = 1; break; }
            }

            return exitCode;
        });

        return command;
    }

    private static int PrintResults(IReadOnlyList<JobResult> results)
    {
        foreach (var r in results)
        {
            if (!string.IsNullOrWhiteSpace(r.Stdout)) Console.Write(r.Stdout);
            if (!string.IsNullOrWhiteSpace(r.Stderr)) Console.Error.Write(r.Stderr);
            if (!r.Success)
            {
                Console.Error.WriteLine($"Compilation failed (exit {r.ExitCode})");
                return 1;
            }
        }
        return 0;
    }
}
