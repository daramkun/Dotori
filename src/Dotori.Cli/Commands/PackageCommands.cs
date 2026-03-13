using System.CommandLine;
using System.Text;
using Dotori.Core.Parsing;
using Dotori.PackageManager;

namespace Dotori.Cli.Commands;

internal static class PackageCommandFactory
{
    public static Command Create()
    {
        var packageCommand = new Command("package", "Package management commands (aliases: dotori add/remove/update/list)");

        packageCommand.Add(CreateAddCommand());
        packageCommand.Add(CreateRemoveCommand());
        packageCommand.Add(CreateUpdateCommand());
        packageCommand.Add(CreateListCommand());

        return packageCommand;
    }

    // ─── Individual factory methods (also used as top-level commands) ─────────

    public static Command CreateAddCommand()
    {
        var command = new Command("add", "Add a dependency to the project");

        var nameArg    = new Argument<string>("name")    { Description = "Package name (optionally with @version), git URL, or local path" };
        var gitOption  = new Option<bool>("--git")       { Description = "Treat name as a git URL" };
        var pathOption = new Option<bool>("--path")      { Description = "Treat name as a local path" };
        var projectOption = new Option<string?>("--project") { Description = "Path to .dotori file or directory" };

        command.Add(nameArg);
        command.Add(gitOption);
        command.Add(pathOption);
        command.Add(projectOption);

        command.SetAction(async (parseResult, ct) =>
        {
            var nameArg2   = parseResult.GetValue(nameArg);
            var isGit      = parseResult.GetValue(gitOption);
            var isPath     = parseResult.GetValue(pathOption);
            var projectArg = parseResult.GetValue(projectOption);

            var paths = BuildContext.ResolveProjectPaths(projectArg, buildAll: false);
            if (paths.Count == 0) return 1;
            var dotoriPath = paths[0];
            var projectDir = Path.GetDirectoryName(dotoriPath)!;

            // Parse name@version or plain name
            string depName;
            string depLine;

            if (isPath)
            {
                // Local path dependency
                depName = Path.GetFileName(nameArg2!.TrimEnd('/', '\\'));
                depLine = $"    {depName} = {{ path = \"{nameArg2}\" }}";
            }
            else if (isGit)
            {
                // Git URL dependency (no version info at this point)
                depName = ExtractGitName(nameArg2!);
                depLine = $"    {depName} = {{ git = \"{nameArg2}\" }}";
            }
            else
            {
                // name or name@version
                var atIdx = nameArg2!.IndexOf('@');
                if (atIdx >= 0)
                {
                    depName = nameArg2[..atIdx];
                    var version = nameArg2[(atIdx + 1)..];
                    depLine = $"    {depName} = \"{version}\"";
                }
                else
                {
                    depName = nameArg2;
                    depLine = $"    {depName} = \"*\"";
                }
            }

            // Modify the .dotori file text
            var text = File.ReadAllText(dotoriPath, Encoding.UTF8);
            var modified = InsertDependency(text, depLine);
            if (modified is null)
            {
                Console.Error.WriteLine($"Error: could not find project block to insert dependency in '{dotoriPath}'.");
                return 1;
            }

            File.WriteAllText(dotoriPath, modified, Encoding.UTF8);
            Console.WriteLine($"  Added {depName} to {dotoriPath}");

            // Re-resolve lock file
            return await UpdateLockAsync(dotoriPath, projectDir, null, ct);
        });

        return command;
    }

    public static Command CreateRemoveCommand()
    {
        var command = new Command("remove", "Remove a dependency from the project");

        var nameArg       = new Argument<string>("name")     { Description = "Package name to remove" };
        var projectOption = new Option<string?>("--project") { Description = "Path to .dotori file or directory" };

        command.Add(nameArg);
        command.Add(projectOption);

        command.SetAction(async (parseResult, ct) =>
        {
            var depName    = parseResult.GetValue(nameArg);
            var projectArg = parseResult.GetValue(projectOption);

            var paths = BuildContext.ResolveProjectPaths(projectArg, buildAll: false);
            if (paths.Count == 0) return 1;
            var dotoriPath = paths[0];
            var projectDir = Path.GetDirectoryName(dotoriPath)!;

            var text = File.ReadAllText(dotoriPath, Encoding.UTF8);
            var modified = RemoveDependency(text, depName!);
            if (modified is null)
            {
                Console.Error.WriteLine($"Error: dependency '{depName}' not found in '{dotoriPath}'.");
                return 1;
            }

            File.WriteAllText(dotoriPath, modified, Encoding.UTF8);
            Console.WriteLine($"  Removed {depName} from {dotoriPath}");

            // Re-resolve lock file (remove from lock too)
            return await UpdateLockAsync(dotoriPath, projectDir, null, ct);
        });

        return command;
    }

    public static Command CreateUpdateCommand()
    {
        var command = new Command("update", "Update dependencies (re-resolve lock file)");

        var nameArg       = new Argument<string?>("name")
        {
            Description = "Package name to update (omit for all)",
            Arity = ArgumentArity.ZeroOrOne,
        };
        var projectOption = new Option<string?>("--project") { Description = "Path to .dotori file or directory" };

        command.Add(nameArg);
        command.Add(projectOption);

        command.SetAction(async (parseResult, ct) =>
        {
            var specificName = parseResult.GetValue(nameArg);
            var projectArg   = parseResult.GetValue(projectOption);

            var paths = BuildContext.ResolveProjectPaths(projectArg, buildAll: false);
            if (paths.Count == 0) return 1;

            int exitCode = 0;
            foreach (var dotoriPath in paths)
            {
                var projectDir = Path.GetDirectoryName(dotoriPath)!;
                exitCode = await UpdateLockAsync(dotoriPath, projectDir, specificName, ct);
                if (exitCode != 0) break;
            }
            return exitCode;
        });

        return command;
    }

    public static Command CreateListCommand()
    {
        var command = new Command("list", "List declared dependencies");

        var projectOption = new Option<string?>("--project") { Description = "Path to .dotori file or directory" };
        command.Add(projectOption);

        command.SetAction((parseResult) =>
        {
            var projectArg = parseResult.GetValue(projectOption);

            var paths = BuildContext.ResolveProjectPaths(projectArg, buildAll: false);
            if (paths.Count == 0) return 1;

            foreach (var dotoriPath in paths)
            {
                try
                {
                    var file = DotoriParser.ParseFile(dotoriPath);
                    if (file.Project is null)
                    {
                        Console.Error.WriteLine($"Error: No project declaration in '{dotoriPath}'.");
                        return 1;
                    }

                    var deps = CollectAllDeps(file.Project);
                    if (deps.Count == 0)
                    {
                        Console.WriteLine($"{file.Project.Name}: (no dependencies)");
                        return 0;
                    }

                    Console.WriteLine($"{file.Project.Name}:");
                    foreach (var dep in deps)
                    {
                        var valueStr = dep.Value switch
                        {
                            VersionDependency v  => v.Version,
                            ComplexDependency c when c.Path is not null  => $"path = {c.Path}",
                            ComplexDependency c when c.Git  is not null  => $"git = {c.Git}" + (c.Tag is not null ? $", tag = {c.Tag}" : c.Commit is not null ? $", commit = {c.Commit}" : ""),
                            ComplexDependency c when c.Version is not null => c.Version,
                            _ => "?",
                        };
                        Console.WriteLine($"  {dep.Name,-30} {valueStr}");
                    }
                }
                catch (ParseException ex)
                {
                    Console.Error.WriteLine($"Parse error: {ex.Message}");
                    return 1;
                }
            }
            return 0;
        });

        return command;
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Collect all dependency items from the project, including those inside condition blocks.
    /// De-duplicates by name (last declaration wins for display purposes).
    /// </summary>
    private static List<DependencyItem> CollectAllDeps(ProjectDecl project)
    {
        var seen  = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var order = new List<string>();
        var map   = new Dictionary<string, DependencyItem>(StringComparer.OrdinalIgnoreCase);

        CollectDepsFromItems(project.Items, seen, order, map);
        return order.Select(n => map[n]).ToList();
    }

    private static void CollectDepsFromItems(
        IEnumerable<ProjectItem> items,
        HashSet<string> seen,
        List<string> order,
        Dictionary<string, DependencyItem> map)
    {
        foreach (var item in items)
        {
            if (item is DependenciesBlock db)
            {
                foreach (var dep in db.Items)
                {
                    if (seen.Add(dep.Name))
                        order.Add(dep.Name);
                    map[dep.Name] = dep;
                }
            }
            else if (item is ConditionBlock cb)
            {
                CollectDepsFromItems(cb.Items, seen, order, map);
            }
        }
    }

    /// <summary>
    /// Insert a dependency line into the first `dependencies { }` block in the file text.
    /// If no dependencies block exists, appends one before the project's closing brace.
    /// Returns the modified text, or null on failure.
    /// </summary>
    private static string? InsertDependency(string text, string depLine)
    {
        var lines = text.Split('\n').ToList();

        // Look for an existing `dependencies {` line
        int depsStart = -1;
        int depsBrace = 0;
        int insertBefore = -1;

        for (int i = 0; i < lines.Count; i++)
        {
            var trimmed = lines[i].Trim();
            if (depsStart < 0)
            {
                if (trimmed.StartsWith("dependencies") && trimmed.Contains('{'))
                {
                    depsStart = i;
                    depsBrace = CountBraces(lines[i]);
                }
            }
            else
            {
                depsBrace += CountBraces(lines[i]);
                if (depsBrace <= 0)
                {
                    // Found the closing brace of dependencies block
                    insertBefore = i;
                    break;
                }
            }
        }

        if (insertBefore >= 0)
        {
            // Insert before the closing brace of the dependencies block
            lines.Insert(insertBefore, depLine);
            return string.Join('\n', lines);
        }

        // No dependencies block — find last `}` of the project block and insert a new block before it
        int lastBrace = -1;
        for (int i = lines.Count - 1; i >= 0; i--)
        {
            if (lines[i].Trim() == "}")
            {
                lastBrace = i;
                break;
            }
        }

        if (lastBrace < 0) return null;

        // Insert a new dependencies block before the last `}`
        lines.Insert(lastBrace, "");
        lines.Insert(lastBrace + 1, "    dependencies {");
        lines.Insert(lastBrace + 2, depLine);
        lines.Insert(lastBrace + 3, "    }");
        return string.Join('\n', lines);
    }

    /// <summary>
    /// Remove a dependency by name from the file text.
    /// Returns modified text, or null if the dependency was not found.
    /// </summary>
    private static string? RemoveDependency(string text, string name)
    {
        var lines    = text.Split('\n').ToList();
        var prefix   = name + " ";
        var prefixEq = name + "=";

        for (int i = 0; i < lines.Count; i++)
        {
            var trimmed = lines[i].Trim();
            // Match lines like `name = ...` inside a dependencies block
            if (trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith(prefixEq, StringComparison.OrdinalIgnoreCase))
            {
                lines.RemoveAt(i);
                return string.Join('\n', lines);
            }
        }

        return null;
    }

    private static int CountBraces(string line)
    {
        int count = 0;
        bool inString = false;
        foreach (var ch in line)
        {
            if (ch == '"') inString = !inString;
            if (!inString)
            {
                if (ch == '{') count++;
                else if (ch == '}') count--;
            }
        }
        return count;
    }

    private static string ExtractGitName(string url)
    {
        // e.g. https://github.com/fmtlib/fmt or https://github.com/fmtlib/fmt.git → fmt
        var u = url.TrimEnd('/');
        if (u.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            u = u[..^4];
        var slash = u.LastIndexOf('/');
        return slash >= 0 ? u[(slash + 1)..] : u;
    }

    /// <summary>Re-resolve the lock file for the given project.</summary>
    private static async Task<int> UpdateLockAsync(
        string dotoriPath, string projectDir, string? specificName, CancellationToken ct)
    {
        try
        {
            var file = DotoriParser.ParseFile(dotoriPath);
            if (file.Project is null)
            {
                Console.Error.WriteLine($"Error: No project declaration in '{dotoriPath}'.");
                return 1;
            }

            var existingLock = LockManager.Load(projectDir);

            // If updating a specific package, remove it from the lock to force re-fetch
            if (specificName is not null)
            {
                var toRemove = existingLock.Packages
                    .Where(p => p.Name.Equals(specificName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                foreach (var e in toRemove)
                    existingLock.Packages.Remove(e);
            }
            else
            {
                // Full update: start fresh
                existingLock = new LockFile();
            }

            var newLock = await DependencyResolver.ResolveAsync(file.Project, existingLock, ct);
            LockManager.Save(newLock, projectDir);

            Console.WriteLine($"  Lock file updated ({newLock.Packages.Count} package(s)).");
            return 0;
        }
        catch (PackageManagerException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error updating lock file: {ex.Message}");
            return 1;
        }
    }
}
