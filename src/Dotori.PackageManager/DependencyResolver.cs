using Dotori.Core.Parsing;

namespace Dotori.PackageManager;

/// <summary>
/// Resolves package dependencies from a project's dependency block using PubGrub.
/// Produces an updated lock file.
/// </summary>
public static class DependencyResolver
{
    /// <summary>
    /// Resolve all non-path dependencies from a project file and update the lock file.
    /// Uses PubGrub for version-constraint solving and conflict detection.
    /// </summary>
    /// <param name="project">Parsed project declaration.</param>
    /// <param name="existingLock">Existing lock file to update.</param>
    /// <returns>Updated lock file.</returns>
    public static async Task<LockFile> ResolveAsync(
        ProjectDecl project,
        LockFile existingLock,
        CancellationToken ct = default)
    {
        // Collect all non-path dependencies from the project
        var rootDeps = CollectRootDependencies(project);

        if (rootDeps.Count == 0)
        {
            // Nothing to resolve — preserve existing lock
            var empty = new LockFile { LockVersion = existingLock.LockVersion };
            foreach (var e in existingLock.Packages)
                empty.Packages.Add(e);
            return empty;
        }

        // Build a package source backed by the existing lock + git fetcher
        var source = new Phase1PackageSource(existingLock, ct);

        // Register git dependencies so the source knows where to fetch them
        RegisterGitDependencies(project, source);

        // Run PubGrub solver
        var solver = new PubGrubSolver(source);
        var solved = await solver.SolveAsync(rootDeps, ct);

        // Build the new lock file from solved versions
        var newLock = new LockFile { LockVersion = 1 };

        foreach (var (name, version) in solved)
        {
            ct.ThrowIfCancellationRequested();

            var existing = existingLock.Packages.FirstOrDefault(
                p => p.Name == name && p.Version == version.ToString());

            if (existing is not null)
            {
                // Re-use existing lock entry
                newLock.Packages.Add(existing);
                continue;
            }

            // Need to fetch / record this package
            if (source.TryGetGitInfo(name, out var gitUrl, out var tagOrCommit))
            {
                var dir  = await GitFetcher.FetchAsync(name, gitUrl!, tagOrCommit!, ct);
                var hash = GitFetcher.ComputeDirectoryHash(dir);
                var sourceStr = $"git+{gitUrl}#{tagOrCommit}";

                var entry = new LockEntry
                {
                    Name    = name,
                    Version = version.ToString(),
                    Source  = sourceStr,
                    Hash    = hash,
                };
                // Add transitive deps as "name@version" references
                var deps = await source.GetDependenciesAsync(name, version, ct);
                foreach (var (depName, _) in deps)
                {
                    if (solved.TryGetValue(depName, out var depVer))
                        entry.Deps.Add($"{depName}@{depVer}");
                }
                newLock.Packages.Add(entry);
            }
            else
            {
                // Version-only dependency (no registry yet — record as placeholder)
                var entry = new LockEntry
                {
                    Name    = name,
                    Version = version.ToString(),
                    Source  = $"version:{name}@{version}",
                };
                newLock.Packages.Add(entry);
            }
        }

        return newLock;
    }

    // ─── Git Registration ────────────────────────────────────────────────────

    private static void RegisterGitDependencies(ProjectDecl project, Phase1PackageSource source)
    {
        var deps = project.Items
            .OfType<DependenciesBlock>()
            .SelectMany(b => b.Items);

        foreach (var dep in deps)
        {
            if (dep.Value is ComplexDependency git && git.Git is not null)
            {
                var tagOrCommit = git.Tag ?? git.Commit ?? "HEAD";
                source.RegisterGit(dep.Name, git.Git, tagOrCommit);
            }
        }
    }

    // ─── Root Dependency Collection ──────────────────────────────────────────

    /// <summary>
    /// Collect root-level (non-path) dependencies from the project, converting them to
    /// (name → VersionConstraint) pairs.  Git deps are registered as exact-version
    /// constraints once their tag/commit is known.
    /// </summary>
    internal static Dictionary<string, VersionConstraint> CollectRootDependencies(ProjectDecl project)
    {
        var result = new Dictionary<string, VersionConstraint>(StringComparer.OrdinalIgnoreCase);

        var deps = project.Items
            .OfType<DependenciesBlock>()
            .SelectMany(b => b.Items);

        foreach (var dep in deps)
        {
            if (dep.Value is ComplexDependency complex && complex.Path is not null)
                continue;  // path deps → ProjectDagBuilder

            VersionConstraint constraint;

            if (dep.Value is VersionDependency ver)
            {
                constraint = VersionConstraint.Parse(ver.Version);
            }
            else if (dep.Value is ComplexDependency git && git.Git is not null)
            {
                // Git dep: version = tag (strip 'v') or commit prefix
                var tagOrCommit = git.Tag ?? git.Commit ?? "HEAD";
                var versionStr = tagOrCommit.StartsWith('v')
                    ? tagOrCommit[1..]
                    : tagOrCommit[..Math.Min(8, tagOrCommit.Length)];

                if (!SemanticVersion.TryParse(versionStr, out _))
                    versionStr = "0.0.0";

                constraint = VersionConstraint.Parse(versionStr);
            }
            else if (dep.Value is ComplexDependency complex2 && complex2.Version is not null)
            {
                constraint = VersionConstraint.Parse(complex2.Version);
            }
            else
            {
                constraint = VersionConstraint.Any;
            }

            // Merge with existing constraint if the same package appears twice
            if (result.TryGetValue(dep.Name, out var existing))
            {
                var intersected = VersionConstraint.Intersect(existing, constraint);
                if (intersected is null)
                    throw new PackageManagerException(
                        $"Incompatible constraints for '{dep.Name}': {existing} and {constraint}");
                result[dep.Name] = intersected;
            }
            else
            {
                result[dep.Name] = constraint;
            }
        }

        return result;
    }
}

// ─── Phase 1 Package Source ──────────────────────────────────────────────────

/// <summary>
/// Package source for Phase 1: no central registry.
/// Versions come from the existing lock file; git packages are fetched to discover deps.
/// </summary>
internal sealed class Phase1PackageSource : IPackageSource
{
    private readonly LockFile _existingLock;
    private readonly CancellationToken _ct;

    // git: name → (url, tagOrCommit)
    private readonly Dictionary<string, (string url, string tagOrCommit)> _gitInfo = new();

    // Cache of fetched manifests: name@version → deps
    private readonly Dictionary<string, IReadOnlyDictionary<string, VersionConstraint>> _depCache = new();

    public Phase1PackageSource(LockFile existingLock, CancellationToken ct)
    {
        _existingLock = existingLock;
        _ct = ct;
    }

    /// <summary>Register a git dependency so we can fetch it later.</summary>
    public void RegisterGit(string name, string url, string tagOrCommit) =>
        _gitInfo[name] = (url, tagOrCommit);

    public bool TryGetGitInfo(string name, out string? url, out string? tagOrCommit)
    {
        if (_gitInfo.TryGetValue(name, out var info))
        {
            url = info.url;
            tagOrCommit = info.tagOrCommit;
            return true;
        }
        url = tagOrCommit = null;
        return false;
    }

    public Task<IReadOnlyList<SemanticVersion>> GetVersionsAsync(string package, CancellationToken ct)
    {
        var versions = new List<SemanticVersion>();

        // Check existing lock first
        foreach (var entry in _existingLock.Packages)
        {
            if (entry.Name.Equals(package, StringComparison.OrdinalIgnoreCase) &&
                SemanticVersion.TryParse(entry.Version, out var v))
            {
                versions.Add(v);
            }
        }

        // If we have git info, the only "available" version is the pinned one
        if (_gitInfo.TryGetValue(package, out var gitInfo))
        {
            var tagOrCommit = gitInfo.tagOrCommit;
            var versionStr = tagOrCommit.StartsWith('v')
                ? tagOrCommit[1..]
                : tagOrCommit[..Math.Min(8, tagOrCommit.Length)];

            if (SemanticVersion.TryParse(versionStr, out var gitVer) && !versions.Contains(gitVer))
                versions.Add(gitVer);
        }

        // Sort newest first
        versions.Sort((a, b) => b.CompareTo(a));

        return Task.FromResult<IReadOnlyList<SemanticVersion>>(versions);
    }

    public async Task<IReadOnlyDictionary<string, VersionConstraint>> GetDependenciesAsync(
        string package, SemanticVersion version, CancellationToken ct)
    {
        var key = $"{package}@{version}";
        if (_depCache.TryGetValue(key, out var cached))
            return cached;

        // Try to get from existing lock (no transitive manifest available in Phase 1 without fetching)
        var lockEntry = _existingLock.Packages.FirstOrDefault(
            p => p.Name.Equals(package, StringComparison.OrdinalIgnoreCase) &&
                 SemanticVersion.TryParse(p.Version, out var v) && v == version);

        if (lockEntry is not null && lockEntry.Deps.Count > 0)
        {
            var deps = new Dictionary<string, VersionConstraint>();
            foreach (var dep in lockEntry.Deps)
            {
                // Format: "name@version"
                var atIdx = dep.LastIndexOf('@');
                if (atIdx > 0)
                {
                    var depName = dep[..atIdx];
                    var depVer  = dep[(atIdx + 1)..];
                    deps[depName] = VersionConstraint.Parse(depVer);
                }
            }
            _depCache[key] = deps;
            return deps;
        }

        // For git packages: we could fetch and parse .dotori, but that is expensive.
        // In Phase 1, return empty deps (transitive deps of git packages are resolved
        // if they declare their own deps in a future registry phase).
        var empty = new Dictionary<string, VersionConstraint>();
        _depCache[key] = empty;
        return empty;
    }
}
