using System.CommandLine;
using Dotori.Core;
using Dotori.Core.Parsing;

namespace Dotori.Cli.Commands;

internal static class CleanCommandFactory
{
    public static Command Create()
    {
        var command = new Command("clean", "Remove build artifacts");

        var projectOption = new Option<string?>("--project") { Description = "Path to .dotori file or directory" };
        var allOption     = new Option<bool>("--all")        { Description = "Remove all cached artifacts including packages" };

        command.Add(projectOption);
        command.Add(allOption);

        command.SetAction((parseResult) =>
        {
            var projectArg = parseResult.GetValue(projectOption);
            var cleanAll   = parseResult.GetValue(allOption);

            var paths = BuildContext.ResolveProjectPaths(projectArg, buildAll: true);

            foreach (var dotoriPath in paths)
            {
                var projectDir = Path.GetDirectoryName(dotoriPath)!;

                // Remove .dotori-cache/
                var cacheDir = Path.Combine(projectDir, DotoriConstants.CacheDir);
                if (Directory.Exists(cacheDir))
                {
                    Console.WriteLine($"  Removing {cacheDir}");
                    Directory.Delete(cacheDir, recursive: true);
                }

                // Remove output directories declared in output { } blocks
                try
                {
                    var file = DotoriParser.ParseFile(dotoriPath);
                    if (file.Project is not null)
                    {
                        foreach (var dir in CollectOutputPaths(file.Project, projectDir))
                        {
                            if (Directory.Exists(dir))
                            {
                                Console.WriteLine($"  Removing {dir}");
                                Directory.Delete(dir, recursive: true);
                            }
                        }
                    }
                }
                catch { /* ignore parse errors during clean */ }
            }

            // Remove global package cache
            if (cleanAll)
            {
                var globalCache = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".dotori", "packages");
                if (Directory.Exists(globalCache))
                {
                    Console.WriteLine($"  Removing {globalCache}");
                    Directory.Delete(globalCache, recursive: true);
                }
            }

            Console.WriteLine("  Clean complete.");
            return 0;
        });

        return command;
    }

    private static IEnumerable<string> CollectOutputPaths(ProjectDecl project, string projectDir)
        => CollectOutputPathsFromItems(project.Items, projectDir);

    private static IEnumerable<string> CollectOutputPathsFromItems(
        IEnumerable<ProjectItem> items, string projectDir)
    {
        foreach (var item in items)
        {
            if (item is OutputBlock ob)
            {
                if (ob.Binaries  is not null) yield return ResolveDir(ob.Binaries,  projectDir);
                if (ob.Libraries is not null) yield return ResolveDir(ob.Libraries, projectDir);
                if (ob.Symbols   is not null) yield return ResolveDir(ob.Symbols,   projectDir);
            }
            else if (item is ConditionBlock cb)
            {
                foreach (var p in CollectOutputPathsFromItems(cb.Items, projectDir))
                    yield return p;
            }
        }
    }

    private static string ResolveDir(string path, string projectDir)
        => Path.IsPathRooted(path) ? path : Path.GetFullPath(Path.Combine(projectDir, path));
}
