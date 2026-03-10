namespace Dotori.PackageManager;

// ─── Package source interface ─────────────────────────────────────────────────

/// <summary>
/// Provides available versions and dependency manifests for packages.
/// </summary>
internal interface IPackageSource
{
    /// <summary>Return all known versions of a package, newest first.</summary>
    Task<IReadOnlyList<SemanticVersion>> GetVersionsAsync(string package, CancellationToken ct);

    /// <summary>
    /// Return the dependencies of a package at a specific version,
    /// as (packageName → constraint) pairs.
    /// </summary>
    Task<IReadOnlyDictionary<string, VersionConstraint>> GetDependenciesAsync(
        string package, SemanticVersion version, CancellationToken ct);
}

// ─── PubGrub Solver ───────────────────────────────────────────────────────────

/// <summary>
/// Simplified PubGrub-inspired version solver.
///
/// Algorithm:
/// 1. Maintain a working set of (package → constraint) pairs to resolve.
/// 2. For each package, pick the newest version satisfying the constraint.
/// 3. Fetch its dependencies, intersect constraints.
/// 4. If intersection is empty → conflict.
/// 5. Continue until all packages are resolved.
///
/// Conflicts are reported immediately with a clear error message.
/// </summary>
internal sealed class PubGrubSolver
{
    private readonly IPackageSource _source;

    public PubGrubSolver(IPackageSource source)
    {
        _source = source;
    }

    /// <summary>
    /// Solve version selection for the given root dependencies.
    /// Returns package → selected version.
    /// Throws <see cref="PackageManagerException"/> if no solution exists.
    /// </summary>
    public async Task<IReadOnlyDictionary<string, SemanticVersion>> SolveAsync(
        IReadOnlyDictionary<string, VersionConstraint> rootDeps,
        CancellationToken ct)
    {
        // pending: package → accumulated constraint (may tighten as deps are discovered)
        var pending = new Dictionary<string, VersionConstraint>(StringComparer.OrdinalIgnoreCase);
        // decided: package → chosen version
        var decided = new Dictionary<string, SemanticVersion>(StringComparer.OrdinalIgnoreCase);

        // Seed with root deps
        foreach (var (name, constraint) in rootDeps)
            pending[name] = constraint;

        int maxIterations = 10_000;
        while (pending.Count > 0)
        {
            ct.ThrowIfCancellationRequested();

            if (--maxIterations <= 0)
                throw new PackageManagerException(
                    "Dependency resolution did not converge (possible cycle or unsolvable constraints).");

            // Pick the next unresolved package
            var package = pending.Keys.First();
            var constraint = pending[package];
            pending.Remove(package);

            // If already decided, check compatibility
            if (decided.TryGetValue(package, out var existingVer))
            {
                if (!constraint.Allows(existingVer))
                {
                    throw new PackageManagerException(
                        $"Version conflict for '{package}': " +
                        $"already selected {existingVer} but constraint {constraint} is incompatible.");
                }
                // Already resolved, no need to re-fetch deps
                continue;
            }

            // Find the best (newest) version satisfying the constraint
            var versions = await _source.GetVersionsAsync(package, ct);
            SemanticVersion? chosen = null;
            foreach (var v in versions)
            {
                if (constraint.Allows(v))
                {
                    chosen = v;
                    break;
                }
            }

            if (chosen is null)
            {
                if (versions.Count == 0)
                    throw new PackageManagerException(
                        $"Package '{package}' not found (no versions available).");
                throw new PackageManagerException(
                    $"No version of '{package}' satisfies constraint '{constraint}'. " +
                    $"Available: {string.Join(", ", versions.Take(5))}");
            }

            decided[package] = chosen.Value;

            // Fetch transitive dependencies and add/merge into pending
            var deps = await _source.GetDependenciesAsync(package, chosen.Value, ct);
            foreach (var (depName, depConstraint) in deps)
            {
                if (decided.TryGetValue(depName, out var resolvedVer))
                {
                    // Already resolved: check compatibility
                    if (!depConstraint.Allows(resolvedVer))
                    {
                        throw new PackageManagerException(
                            $"Version conflict for '{depName}': " +
                            $"selected {resolvedVer} (required by another package) but " +
                            $"'{package}@{chosen}' requires {depConstraint}.");
                    }
                }
                else if (pending.TryGetValue(depName, out var existingConstraint))
                {
                    // Merge constraints
                    var merged = VersionConstraint.Intersect(existingConstraint, depConstraint);
                    if (merged is null)
                    {
                        throw new PackageManagerException(
                            $"Version conflict for '{depName}': " +
                            $"constraint {existingConstraint} is incompatible with " +
                            $"{depConstraint} (required by '{package}@{chosen}').");
                    }
                    pending[depName] = merged;
                }
                else
                {
                    pending[depName] = depConstraint;
                }
            }
        }

        return decided;
    }
}
