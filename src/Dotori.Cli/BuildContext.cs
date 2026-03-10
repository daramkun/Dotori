using Dotori.Core.Graph;
using Dotori.Core.Location;
using Dotori.Core.Model;
using Dotori.Core.Parsing;
using Dotori.Core.Toolchain;

namespace Dotori.Cli;

/// <summary>
/// Shared helper to resolve projects, select targets, detect toolchains,
/// and flatten project models for CLI commands.
/// </summary>
internal static class BuildContext
{
    /// <summary>
    /// Resolve which .dotori files to operate on, using the standard search order.
    /// </summary>
    internal static IReadOnlyList<string> ResolveProjectPaths(
        string? projectArg,
        bool    buildAll)
    {
        if (projectArg is not null)
            return [ProjectLocator.ResolveExplicitPath(projectArg)];

        var found = ProjectLocator.FindDotoriFiles(Directory.GetCurrentDirectory());
        if (found.Count == 0)
        {
            Console.Error.WriteLine("Error: No .dotori file found.");
            return [];
        }

        if (found.Count == 1 || buildAll)
            return found;

        // Multiple projects: load metadata and prompt
        var located = found.Select(ProjectLocator.LoadProject).ToList();
        try
        {
            return ProjectLocator.PromptSelection(located, buildAll);
        }
        catch (ProjectLocatorException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return [];
        }
    }

    /// <summary>
    /// Build the DAG for the given root project paths, checking for cycles.
    /// Returns null on error.
    /// </summary>
    internal static IReadOnlyDictionary<string, ProjectNode>? BuildDag(
        IReadOnlyList<string> rootPaths)
    {
        try
        {
            return ProjectDagBuilder.Build(rootPaths);
        }
        catch (CircularDependencyException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Determine the build target ID from CLI options and host environment.
    /// </summary>
    internal static string ResolveTargetId(string? targetArg)
    {
        if (targetArg is not null) return targetArg;

        // Auto-detect from host OS + arch
        var os   = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                       System.Runtime.InteropServices.OSPlatform.Windows) ? "windows"
                 : System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                       System.Runtime.InteropServices.OSPlatform.OSX)     ? "macos"
                 : "linux";

        var arch = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture switch
        {
            System.Runtime.InteropServices.Architecture.X64   => "x64",
            System.Runtime.InteropServices.Architecture.Arm64 => "arm64",
            System.Runtime.InteropServices.Architecture.X86   => "x86",
            _                                                  => "x64",
        };

        return $"{os}-{arch}";
    }

    /// <summary>
    /// Detect toolchain for the given target. Prints error and returns null on failure.
    /// </summary>
    internal static ToolchainInfo? DetectToolchain(string targetId, string? compiler)
    {
        var tc = ToolchainDetector.Detect(targetId, compiler);
        if (tc is null)
            Console.Error.WriteLine(
                $"Error: No toolchain found for target '{targetId}'. " +
                $"Ensure the compiler is installed and in PATH.");
        return tc;
    }

    /// <summary>
    /// Build <see cref="TargetContext"/> from CLI arguments.
    /// </summary>
    internal static TargetContext MakeTargetContext(
        string targetId, string config,
        string? runtimeLink = null, string? libc = null, string? stdlib = null)
    {
        var parts    = targetId.Split('-');
        var platform = parts.Length > 0 ? parts[0] : "linux";
        var compiler = parts.Any(p => p is "msvc") ? "msvc" : "clang";

        return new TargetContext
        {
            Platform = platform,
            Config   = config,
            Compiler = compiler,
            Runtime  = runtimeLink ?? "static",
            Libc     = libc,
            Stdlib   = stdlib,
            WasmBackend = targetId.Contains("emscripten") ? "emscripten"
                        : targetId.Contains("bare")       ? "bare"
                        : null,
        };
    }

    /// <summary>
    /// Flatten a project for the given target context.
    /// Optionally injects the public headers from all transitive dependency nodes.
    /// </summary>
    internal static FlatProjectModel? FlattenProject(
        string dotoriPath, TargetContext ctx,
        ProjectNode? node = null)
    {
        try
        {
            var file = DotoriParser.ParseFile(dotoriPath);
            if (file.Project is null)
            {
                Console.Error.WriteLine($"Error: No project declaration in '{dotoriPath}'.");
                return null;
            }
            var model = ProjectFlattener.Flatten(file.Project, dotoriPath, ctx);

            // Inject public headers from all transitive dependencies
            if (node is not null)
                InjectDepHeaders(model, node, ctx, new HashSet<string>(StringComparer.OrdinalIgnoreCase));

            return model;
        }
        catch (ParseException ex)
        {
            Console.Error.WriteLine($"Parse error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Recursively collect public headers from dependency nodes and add them to model.
    /// </summary>
    private static void InjectDepHeaders(
        FlatProjectModel model,
        ProjectNode node,
        TargetContext ctx,
        HashSet<string> visited)
    {
        foreach (var dep in node.Dependencies)
        {
            if (!visited.Add(dep.DotoriPath)) continue;

            try
            {
                var file = DotoriParser.ParseFile(dep.DotoriPath);
                if (file.Project is null) continue;
                var depModel = ProjectFlattener.Flatten(file.Project, dep.DotoriPath, ctx);

                foreach (var h in depModel.Headers.Where(h => h.IsPublic))
                {
                    var absPath = Path.IsPathRooted(h.Path)
                        ? h.Path
                        : Path.GetFullPath(Path.Combine(depModel.ProjectDir, h.Path));
                    // Add only if not already present
                    if (!model.Headers.Any(existing =>
                            string.Equals(existing.Path, absPath, StringComparison.OrdinalIgnoreCase)))
                        model.Headers.Add(new Dotori.Core.Parsing.HeaderItem(isPublic: false, path: absPath));
                }
            }
            catch { /* skip unreadable deps */ }

            InjectDepHeaders(model, dep, ctx, visited);
        }
    }
}
