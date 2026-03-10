using Dotori.Core.Parsing;
using Dotori.PackageManager;

namespace Dotori.Tests.PackageManager;

// ─── Test Package Source ─────────────────────────────────────────────────────

/// <summary>
/// In-memory package source for PubGrub tests.
/// </summary>
internal sealed class TestPackageSource : IPackageSource
{
    private readonly Dictionary<string, List<SemanticVersion>> _versions = new();
    private readonly Dictionary<string, Dictionary<string, VersionConstraint>> _deps = new();

    public void AddVersion(string package, string version,
        Dictionary<string, string>? deps = null)
    {
        var ver = SemanticVersion.Parse(version);
        if (!_versions.TryGetValue(package, out var list))
            _versions[package] = list = new List<SemanticVersion>();
        list.Add(ver);

        var key = $"{package}@{version}";
        var depMap = new Dictionary<string, VersionConstraint>();
        if (deps is not null)
            foreach (var (n, c) in deps)
                depMap[n] = VersionConstraint.Parse(c);
        _deps[key] = depMap;
    }

    public Task<IReadOnlyList<SemanticVersion>> GetVersionsAsync(
        string package, CancellationToken ct)
    {
        if (_versions.TryGetValue(package, out var list))
        {
            var sorted = list.OrderByDescending(v => v).ToList();
            return Task.FromResult<IReadOnlyList<SemanticVersion>>(sorted);
        }
        return Task.FromResult<IReadOnlyList<SemanticVersion>>(Array.Empty<SemanticVersion>());
    }

    public Task<IReadOnlyDictionary<string, VersionConstraint>> GetDependenciesAsync(
        string package, SemanticVersion version, CancellationToken ct)
    {
        var key = $"{package}@{version}";
        if (_deps.TryGetValue(key, out var deps))
            return Task.FromResult<IReadOnlyDictionary<string, VersionConstraint>>(deps);
        return Task.FromResult<IReadOnlyDictionary<string, VersionConstraint>>(
            new Dictionary<string, VersionConstraint>());
    }
}

// ─── PubGrub Solver Tests ────────────────────────────────────────────────────

[TestClass]
public sealed class PubGrubSolverTests
{
    [TestMethod]
    public async Task Solve_SinglePackage_ExactVersion_ResolvesCorrectly()
    {
        var source = new TestPackageSource();
        source.AddVersion("fmt", "10.2.0");

        var solver = new PubGrubSolver(source);
        var result = await solver.SolveAsync(
            new Dictionary<string, VersionConstraint>
            {
                ["fmt"] = VersionConstraint.Parse("10.2.0"),
            },
            CancellationToken.None);

        Assert.IsTrue(result.ContainsKey("fmt"));
        Assert.AreEqual("10.2.0", result["fmt"].ToString());
    }

    [TestMethod]
    public async Task Solve_SinglePackage_PicksNewestSatisfying()
    {
        var source = new TestPackageSource();
        source.AddVersion("fmt", "9.0.0");
        source.AddVersion("fmt", "10.0.0");
        source.AddVersion("fmt", "10.2.0");

        var solver = new PubGrubSolver(source);
        var result = await solver.SolveAsync(
            new Dictionary<string, VersionConstraint>
            {
                ["fmt"] = VersionConstraint.Parse("^10.0.0"),
            },
            CancellationToken.None);

        Assert.IsTrue(result.ContainsKey("fmt"));
        Assert.AreEqual("10.2.0", result["fmt"].ToString());
    }

    [TestMethod]
    public async Task Solve_NoDependencies_ReturnsEmpty()
    {
        var source = new TestPackageSource();
        var solver = new PubGrubSolver(source);
        var result = await solver.SolveAsync(
            new Dictionary<string, VersionConstraint>(),
            CancellationToken.None);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task Solve_TransitiveDependency_IncludesTransitive()
    {
        var source = new TestPackageSource();
        source.AddVersion("spdlog", "1.13.0", deps: new() { ["fmt"] = "^10.0.0" });
        source.AddVersion("fmt", "10.2.0");

        var solver = new PubGrubSolver(source);
        var result = await solver.SolveAsync(
            new Dictionary<string, VersionConstraint>
            {
                ["spdlog"] = VersionConstraint.Parse("1.13.0"),
            },
            CancellationToken.None);

        Assert.IsTrue(result.ContainsKey("spdlog"));
        Assert.IsTrue(result.ContainsKey("fmt"));
        Assert.AreEqual("10.2.0", result["fmt"].ToString());
    }

    [TestMethod]
    public async Task Solve_TwoPackagesDependOnSameLib_SharedDep_Resolved()
    {
        // lib-a 1.0.0 depends on: common ^3.0.0
        // lib-b 2.0.0 depends on: common ^3.1.0
        // common 3.2.0 satisfies both
        var source = new TestPackageSource();
        source.AddVersion("lib-a", "1.0.0", deps: new() { ["common"] = "^3.0.0" });
        source.AddVersion("lib-b", "2.0.0", deps: new() { ["common"] = "^3.1.0" });
        source.AddVersion("common", "3.0.0");
        source.AddVersion("common", "3.1.0");
        source.AddVersion("common", "3.2.0");

        var solver = new PubGrubSolver(source);
        var result = await solver.SolveAsync(
            new Dictionary<string, VersionConstraint>
            {
                ["lib-a"] = VersionConstraint.Parse("^1.0.0"),
                ["lib-b"] = VersionConstraint.Parse("^2.0.0"),
            },
            CancellationToken.None);

        Assert.IsTrue(result.ContainsKey("common"));
        Assert.IsTrue(result["common"] >= SemanticVersion.Parse("3.1.0"));
    }

    [TestMethod]
    public async Task Solve_ConflictingExactVersions_ThrowsPackageManagerException()
    {
        var source = new TestPackageSource();
        source.AddVersion("lib-a", "1.0.0", deps: new() { ["common"] = "1.0.0" });
        source.AddVersion("lib-b", "1.0.0", deps: new() { ["common"] = "2.0.0" });
        source.AddVersion("common", "1.0.0");
        source.AddVersion("common", "2.0.0");

        var solver = new PubGrubSolver(source);

        await Assert.ThrowsExactlyAsync<PackageManagerException>(async () =>
        {
            await solver.SolveAsync(
                new Dictionary<string, VersionConstraint>
                {
                    ["lib-a"] = VersionConstraint.Parse("1.0.0"),
                    ["lib-b"] = VersionConstraint.Parse("1.0.0"),
                },
                CancellationToken.None);
        });
    }

    [TestMethod]
    public async Task Solve_NoVersionSatisfiesConstraint_ThrowsPackageManagerException()
    {
        var source = new TestPackageSource();
        source.AddVersion("fmt", "9.0.0");

        var solver = new PubGrubSolver(source);

        await Assert.ThrowsExactlyAsync<PackageManagerException>(async () =>
        {
            await solver.SolveAsync(
                new Dictionary<string, VersionConstraint>
                {
                    ["fmt"] = VersionConstraint.Parse("^10.0.0"),
                },
                CancellationToken.None);
        });
    }
}

// ─── DependencyResolver Integration Tests ────────────────────────────────────

[TestClass]
public sealed class DependencyResolverTests
{
    private static readonly SourceLocation Loc = new("test.dotori", 1, 1);

    [TestMethod]
    public void CollectRootDependencies_NoDeps_ReturnsEmpty()
    {
        var project = MakeProject();
        var deps = DependencyResolver.CollectRootDependencies(project);
        Assert.AreEqual(0, deps.Count);
    }

    [TestMethod]
    public void CollectRootDependencies_VersionDep_ParsesConstraint()
    {
        var project = MakeProject(("fmt", new VersionDependency("^10.0.0")));
        var deps = DependencyResolver.CollectRootDependencies(project);

        Assert.IsTrue(deps.ContainsKey("fmt"));
        Assert.IsTrue(deps["fmt"].Allows(SemanticVersion.Parse("10.2.0")));
        Assert.IsFalse(deps["fmt"].Allows(SemanticVersion.Parse("11.0.0")));
    }

    [TestMethod]
    public void CollectRootDependencies_PathDep_Excluded()
    {
        var project = MakeProject(("my-lib", new ComplexDependency { Path = "../lib" }));
        var deps = DependencyResolver.CollectRootDependencies(project);
        Assert.AreEqual(0, deps.Count);
    }

    [TestMethod]
    public void CollectRootDependencies_GitDep_ParsesVersionFromTag()
    {
        var project = MakeProject(("spdlog", new ComplexDependency
        {
            Git = "https://github.com/gabime/spdlog",
            Tag = "v1.13.0",
        }));
        var deps = DependencyResolver.CollectRootDependencies(project);

        Assert.IsTrue(deps.ContainsKey("spdlog"));
        Assert.IsTrue(deps["spdlog"].Allows(SemanticVersion.Parse("1.13.0")));
    }

    [TestMethod]
    public void CollectRootDependencies_SamePackageTwice_Merges()
    {
        var project = MakeProject(
            ("fmt", new VersionDependency("^10.0.0")),
            ("fmt", new VersionDependency(">=10.1.0")));

        var deps = DependencyResolver.CollectRootDependencies(project);

        // Intersection: >=10.1.0 <11.0.0
        Assert.IsTrue(deps.ContainsKey("fmt"));
        Assert.IsFalse(deps["fmt"].Allows(SemanticVersion.Parse("10.0.9")));
        Assert.IsTrue(deps["fmt"].Allows(SemanticVersion.Parse("10.1.0")));
    }

    [TestMethod]
    public void CollectRootDependencies_IncompatibleConstraints_Throws()
    {
        var project = MakeProject(
            ("fmt", new VersionDependency("^10.0.0")),
            ("fmt", new VersionDependency("^11.0.0")));

        Assert.ThrowsExactly<PackageManagerException>(() =>
            DependencyResolver.CollectRootDependencies(project));
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static ProjectDecl MakeProject(
        params (string name, DependencyValue value)[] deps)
    {
        var project = new ProjectDecl { Location = Loc, Name = "TestProject" };

        if (deps.Length > 0)
        {
            var block = new DependenciesBlock { Location = Loc };
            foreach (var (name, value) in deps)
                block.Items.Add(new DependencyItem(name, value));
            project.Items.Add(block);
        }

        return project;
    }
}
