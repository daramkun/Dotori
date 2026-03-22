using Dotori.Core;
using Dotori.Core.Parsing;
using Dotori.PackageManager.Config;

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
    /// Packages are fetched into &lt;projectDir&gt;/deps/&lt;name&gt;/.
    /// </summary>
    /// <param name="project">Parsed project declaration.</param>
    /// <param name="existingLock">Existing lock file to update.</param>
    /// <param name="projectDir">Absolute path to the project directory (contains .dotori file).</param>
    /// <param name="registryUrl">레지스트리 URL (null이면 설정에서 읽음).</param>
    /// <returns>Updated lock file.</returns>
    public static async Task<LockFile> ResolveAsync(
        ProjectDecl project,
        LockFile existingLock,
        string projectDir,
        string? registryUrl = null,
        CancellationToken ct = default)
    {
        var depsRoot = Path.Combine(projectDir, DotoriConstants.DepsDir);
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

        // 레지스트리 클라이언트 생성 (연결 불가 시 null)
        RegistryClient? registryClient = null;
        var config = DotoriConfigManager.Load();
        var regUrl = registryUrl ?? config.DefaultRegistry;
        try { registryClient = RegistryClient.FromConfig(regUrl); }
        catch { /* 오프라인 모드 — lock 파일 폴백 */ }

        // Build a package source backed by the existing lock + registry + git fetcher
        var source = new RegistryPackageSource(existingLock, regUrl, registryClient, ct);

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
                var dir  = await GitFetcher.FetchAsync(depsRoot, name, gitUrl!, tagOrCommit!, ct);
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
            else if (source.TryGetRegistryInfo(name, out var resolvedRegUrl) && registryClient is not null)
            {
                // 레지스트리 패키지: 다운로드 후 캐시
                var slash = name.IndexOf('/');
                var owner = name[..slash];
                var pkgName = name[(slash + 1)..];

                await PackageInstaller.InstallAsync(depsRoot, registryClient, resolvedRegUrl, owner, pkgName, version.ToString(), ct);
                var entry = new LockEntry
                {
                    Name    = name,
                    Version = version.ToString(),
                    Source  = $"registry+{resolvedRegUrl}/{owner}/{pkgName}@{version}",
                };
                newLock.Packages.Add(entry);
            }
            else
            {
                // 폴백: 플레이스홀더 기록
                var entry = new LockEntry
                {
                    Name    = name,
                    Version = version.ToString(),
                    Source  = $"version:{name}@{version}",
                };
                newLock.Packages.Add(entry);
            }
        }

        registryClient?.Dispose();
        return newLock;
    }

    // ─── Git Registration ────────────────────────────────────────────────────

    private static void RegisterGitDependencies(ProjectDecl project, RegistryPackageSource source)
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

// ─── Phase 1 Package Source (legacy, kept for backward compat) ───────────────

/// <summary>
/// Package source for Phase 1 (git-only): kept for backward compatibility.
/// New code uses RegistryPackageSource.
/// </summary>
internal sealed class Phase1PackageSource : IPackageSource
{
    private readonly LockFile _existingLock;
    private readonly Dictionary<string, (string url, string tagOrCommit)> _gitInfo = new();
    private readonly Dictionary<string, IReadOnlyDictionary<string, VersionConstraint>> _depCache = new();

    public Phase1PackageSource(LockFile existingLock, CancellationToken ct)
    {
        _existingLock = existingLock;
    }

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

        foreach (var entry in _existingLock.Packages)
        {
            if (entry.Name.Equals(package, StringComparison.OrdinalIgnoreCase) &&
                SemanticVersion.TryParse(entry.Version, out var v))
                versions.Add(v);
        }

        if (_gitInfo.TryGetValue(package, out var gitInfo))
        {
            var tagOrCommit = gitInfo.tagOrCommit;
            var versionStr = tagOrCommit.StartsWith('v')
                ? tagOrCommit[1..]
                : tagOrCommit[..Math.Min(8, tagOrCommit.Length)];

            if (SemanticVersion.TryParse(versionStr, out var gitVer) && !versions.Contains(gitVer))
                versions.Add(gitVer);
        }

        versions.Sort((a, b) => b.CompareTo(a));
        return Task.FromResult<IReadOnlyList<SemanticVersion>>(versions);
    }

    public Task<IReadOnlyDictionary<string, VersionConstraint>> GetDependenciesAsync(
        string package, SemanticVersion version, CancellationToken ct)
    {
        var key = $"{package}@{version}";
        if (_depCache.TryGetValue(key, out var cached))
            return Task.FromResult(cached);

        var lockEntry = _existingLock.Packages.FirstOrDefault(
            p => p.Name.Equals(package, StringComparison.OrdinalIgnoreCase) &&
                 SemanticVersion.TryParse(p.Version, out var v) && v == version);

        if (lockEntry is not null && lockEntry.Deps.Count > 0)
        {
            var deps = new Dictionary<string, VersionConstraint>();
            foreach (var dep in lockEntry.Deps)
            {
                var atIdx = dep.LastIndexOf('@');
                if (atIdx > 0)
                    deps[dep[..atIdx]] = VersionConstraint.Parse(dep[(atIdx + 1)..]);
            }
            _depCache[key] = deps;
            return Task.FromResult<IReadOnlyDictionary<string, VersionConstraint>>(deps);
        }

        IReadOnlyDictionary<string, VersionConstraint> empty = new Dictionary<string, VersionConstraint>();
        _depCache[key] = empty;
        return Task.FromResult(empty);
    }
}
