using Dotori.Core.Build;

namespace Dotori.Tests.Build;

[TestClass]
public sealed class GlobExpanderTests
{
    private string _tempDir = string.Empty;

    [TestInitialize]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
    }

    [TestCleanup]
    public void TearDown()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private string Create(string relPath)
    {
        var full = Path.Combine(_tempDir, relPath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllText(full, string.Empty);
        return full;
    }

    // ─── MatchComponent ──────────────────────────────────────────────────────

    [TestMethod]
    public void MatchComponent_Exact_Matches() =>
        Assert.IsTrue(GlobExpander.MatchComponent("foo.cpp", "foo.cpp"));

    [TestMethod]
    public void MatchComponent_StarExtension_Matches() =>
        Assert.IsTrue(GlobExpander.MatchComponent("foo.cpp", "*.cpp"));

    [TestMethod]
    public void MatchComponent_StarExtension_NoMatch() =>
        Assert.IsFalse(GlobExpander.MatchComponent("foo.h", "*.cpp"));

    [TestMethod]
    public void MatchComponent_Question_MatchesSingleChar() =>
        Assert.IsTrue(GlobExpander.MatchComponent("foo.cpp", "???.cpp"));

    [TestMethod]
    public void MatchComponent_StarMatchesAnyChars() =>
        Assert.IsTrue(GlobExpander.MatchComponent("abc.cpp", "a*.cpp"));

    // ─── Expand ──────────────────────────────────────────────────────────────

    [TestMethod]
    public void Expand_SingleFile_ReturnsFile()
    {
        var file = Create("src/main.cpp");
        var result = GlobExpander.Expand(_tempDir, ["src/main.cpp"], []);
        CollectionAssert.Contains(result.ToList(), file);
    }

    [TestMethod]
    public void Expand_StarPattern_MatchesAllCpp()
    {
        Create("src/a.cpp");
        Create("src/b.cpp");
        Create("src/c.h");

        var result = GlobExpander.Expand(_tempDir, ["src/*.cpp"], []);
        Assert.HasCount(2, result);
    }

    [TestMethod]
    public void Expand_DoubleStarPattern_Recursive()
    {
        Create("src/a.cpp");
        Create("src/sub/b.cpp");
        Create("src/sub/deep/c.cpp");

        var result = GlobExpander.Expand(_tempDir, ["src/**/*.cpp"], []);
        Assert.HasCount(3, result);
    }

    [TestMethod]
    public void Expand_Exclude_RemovesFile()
    {
        Create("src/a.cpp");
        Create("src/b.cpp");

        var result = GlobExpander.Expand(_tempDir, ["src/*.cpp"], ["src/b.cpp"]);
        Assert.HasCount(1, result);
        Assert.EndsWith("a.cpp", result[0]);
    }

    [TestMethod]
    public void Expand_NonexistentPattern_ReturnsEmpty()
    {
        var result = GlobExpander.Expand(_tempDir, ["nonexistent/**/*.cpp"], []);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void Expand_MultipleIncludes_Deduplicated()
    {
        Create("src/a.cpp");
        // Include same pattern twice
        var result = GlobExpander.Expand(_tempDir, ["src/*.cpp", "src/*.cpp"], []);
        Assert.HasCount(1, result);
    }
}
