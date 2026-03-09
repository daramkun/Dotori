using Dotori.Core.Parsing;

namespace Dotori.PackageManager;

/// <summary>
/// Resolves package dependencies from a project's dependency block.
/// Produces an updated lock file.
///
/// Phase 1: Simple resolver (no PubGrub — just collects git + version deps).
/// PubGrub conflict resolution is scheduled for a later phase.
/// </summary>
public static class DependencyResolver
{
    /// <summary>
    /// Resolve all non-path dependencies from a project file and update the lock file.
    /// </summary>
    /// <param name="project">Parsed project declaration.</param>
    /// <param name="existingLock">Existing lock file to update.</param>
    /// <returns>Updated lock file (may be same instance if no changes needed).</returns>
    public static async Task<LockFile> ResolveAsync(
        ProjectDecl project,
        LockFile existingLock,
        CancellationToken ct = default)
    {
        var updatedLock = new LockFile { LockVersion = existingLock.LockVersion };

        // Preserve existing entries (other tools/commands may have added them)
        foreach (var existing in existingLock.Packages)
            updatedLock.Packages.Add(existing);

        var deps = project.Items
            .OfType<DependenciesBlock>()
            .SelectMany(b => b.Items)
            .ToList();

        foreach (var dep in deps)
        {
            ct.ThrowIfCancellationRequested();

            if (dep.Value is ComplexDependency complex && complex.Path is not null)
                continue;  // path deps are handled by ProjectDagBuilder

            if (dep.Value is VersionDependency ver)
            {
                EnsureVersionPackage(dep.Name, ver.Version, updatedLock);
            }
            else if (dep.Value is ComplexDependency git && git.Git is not null)
            {
                await EnsureGitPackage(dep.Name, git.Git, git.Tag ?? git.Commit ?? "HEAD",
                    updatedLock, ct);
            }
        }

        return updatedLock;
    }

    private static void EnsureVersionPackage(
        string name, string version, LockFile lockFile)
    {
        // Check if already locked at this version
        if (lockFile.Packages.Any(p => p.Name == name && p.Version == version))
            return;

        // Placeholder: no registry yet — record the version constraint
        var source = $"version:{name}@{version}";
        lockFile.Packages.Add(new LockEntry { Name = name, Version = version, Source = source });
    }

    private static async Task EnsureGitPackage(
        string name, string gitUrl, string tagOrCommit,
        LockFile lockFile, CancellationToken ct)
    {
        var source = $"git+{gitUrl}#{tagOrCommit}";

        // Check if already locked
        if (lockFile.Packages.Any(p => p.Name == name && p.Source == source))
            return;

        // Fetch from git
        var dir  = await GitFetcher.FetchAsync(name, gitUrl, tagOrCommit, ct);
        var hash = GitFetcher.ComputeDirectoryHash(dir);

        // Determine version (use tag if available, else commit prefix)
        var version = tagOrCommit.StartsWith('v')
            ? tagOrCommit[1..]  // strip leading 'v'
            : tagOrCommit[..Math.Min(8, tagOrCommit.Length)];

        var entry = new LockEntry
        {
            Name    = name,
            Version = version,
            Source  = source,
            Hash    = hash,
        };
        lockFile.Packages.Add(entry);
    }
}
