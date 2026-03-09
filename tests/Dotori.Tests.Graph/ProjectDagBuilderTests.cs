using Dotori.Core.Graph;

namespace Dotori.Tests.Graph;

[TestClass]
public sealed class ProjectDagBuilderTests
{
    private static string FixturePath(string name) =>
        Path.Combine(AppContext.BaseDirectory, "fixtures", name);

    // ─── Build ───────────────────────────────────────────────────────────────

    [TestMethod]
    public void Build_SingleProject_CreatesOneNode()
    {
        var nodes = ProjectDagBuilder.Build([FixturePath("dag/core/.dotori")]);
        Assert.AreEqual(1, nodes.Count);
        Assert.IsTrue(nodes.Values.Any(n => n.ProjectName == "Core"));
    }

    [TestMethod]
    public void Build_TransitiveDependency_LoadsAllNodes()
    {
        // app → lib → core
        var nodes = ProjectDagBuilder.Build([FixturePath("dag/app/.dotori")]);
        var names = nodes.Values.Select(n => n.ProjectName).ToHashSet();
        Assert.IsTrue(names.Contains("App"));
        Assert.IsTrue(names.Contains("Lib"));
        Assert.IsTrue(names.Contains("Core"));
    }

    [TestMethod]
    public void Build_SharedDependency_DeduplicatesNodes()
    {
        // Both app and tools depend on core — should result in exactly 4 nodes (no dupe for core)
        var nodes = ProjectDagBuilder.Build([
            FixturePath("dag/app/.dotori"),
            FixturePath("dag/tools/.dotori"),
        ]);
        // app, lib, core, tools — core deduplicated
        Assert.AreEqual(4, nodes.Count);
    }

    [TestMethod]
    public void Build_DependencyEdges_AreCorrect()
    {
        var nodes = ProjectDagBuilder.Build([FixturePath("dag/lib/.dotori")]);
        var lib   = nodes.Values.Single(n => n.ProjectName == "Lib");
        Assert.AreEqual(1, lib.Dependencies.Count);
        Assert.AreEqual("Core", lib.Dependencies[0].ProjectName);
    }

    [TestMethod]
    public void Build_Cycle_ThrowsCircularDependencyException()
    {
        Assert.ThrowsExactly<CircularDependencyException>(() =>
            ProjectDagBuilder.Build([FixturePath("dag-cycle/a/.dotori")]));
    }

    [TestMethod]
    public void Build_CycleMessage_ContainsProjectNames()
    {
        try
        {
            ProjectDagBuilder.Build([FixturePath("dag-cycle/a/.dotori")]);
            Assert.Fail("Expected CircularDependencyException");
        }
        catch (CircularDependencyException ex)
        {
            StringAssert.Contains(ex.Message, "Cycle");
        }
    }

    // ─── TopologicalSort ─────────────────────────────────────────────────────

    [TestMethod]
    public void TopologicalSort_CoreBeforeLib()
    {
        var nodes = ProjectDagBuilder.Build([FixturePath("dag/lib/.dotori")]);
        var order = ProjectDagBuilder.TopologicalSort(nodes);
        var names = order.Select(n => n.ProjectName).ToList();

        var coreIdx = names.IndexOf("Core");
        var libIdx  = names.IndexOf("Lib");
        Assert.IsTrue(coreIdx < libIdx, $"Core ({coreIdx}) should precede Lib ({libIdx})");
    }

    [TestMethod]
    public void TopologicalSort_CoreBeforeLibBeforeApp()
    {
        var nodes = ProjectDagBuilder.Build([FixturePath("dag/app/.dotori")]);
        var order = ProjectDagBuilder.TopologicalSort(nodes);
        var names = order.Select(n => n.ProjectName).ToList();

        var coreIdx = names.IndexOf("Core");
        var libIdx  = names.IndexOf("Lib");
        var appIdx  = names.IndexOf("App");
        Assert.IsTrue(coreIdx < libIdx, "Core before Lib");
        Assert.IsTrue(libIdx  < appIdx, "Lib before App");
        Assert.IsTrue(coreIdx < appIdx, "Core before App");
    }

    // ─── BuildLevels ─────────────────────────────────────────────────────────

    [TestMethod]
    public void BuildLevels_CoreIsFirstLevel()
    {
        var nodes  = ProjectDagBuilder.Build([FixturePath("dag/app/.dotori")]);
        var levels = ProjectDagBuilder.BuildLevels(nodes);
        var firstLevel = levels[0].Select(n => n.ProjectName).ToList();
        CollectionAssert.Contains(firstLevel, "Core");
    }

    [TestMethod]
    public void BuildLevels_AppIsLastLevel()
    {
        var nodes  = ProjectDagBuilder.Build([FixturePath("dag/app/.dotori")]);
        var levels = ProjectDagBuilder.BuildLevels(nodes);
        var lastLevel = levels[^1].Select(n => n.ProjectName).ToList();
        CollectionAssert.Contains(lastLevel, "App");
    }

    [TestMethod]
    public void BuildLevels_LibAndToolsParallelAfterCore()
    {
        // tools and lib both depend only on core → should be in same level
        var nodes = ProjectDagBuilder.Build([
            FixturePath("dag/app/.dotori"),
            FixturePath("dag/tools/.dotori"),
        ]);
        var levels = ProjectDagBuilder.BuildLevels(nodes);

        // Find level containing Lib and Tools
        var libLevel   = levels.FirstOrDefault(l => l.Any(n => n.ProjectName == "Lib"));
        var toolsLevel = levels.FirstOrDefault(l => l.Any(n => n.ProjectName == "Tools"));
        Assert.IsNotNull(libLevel);
        Assert.IsNotNull(toolsLevel);

        // They should be in the same level (parallel)
        var levelList = levels.ToList();
        int libLevelIdx   = levelList.IndexOf(libLevel);
        int toolsLevelIdx = levelList.IndexOf(toolsLevel);
        Assert.AreEqual(libLevelIdx, toolsLevelIdx,
            "Lib and Tools should be at the same parallel build level");
    }

    [TestMethod]
    public void BuildLevels_ContainsAllNodes()
    {
        var nodes  = ProjectDagBuilder.Build([FixturePath("dag/app/.dotori")]);
        var levels = ProjectDagBuilder.BuildLevels(nodes);
        var allInLevels = levels.SelectMany(l => l).Select(n => n.ProjectName).ToHashSet();
        foreach (var node in nodes.Values)
            Assert.IsTrue(allInLevels.Contains(node.ProjectName),
                $"Node '{node.ProjectName}' missing from levels");
    }
}
