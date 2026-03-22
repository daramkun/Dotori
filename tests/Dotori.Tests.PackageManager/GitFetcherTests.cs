using Dotori.PackageManager;

namespace Dotori.Tests.PackageManager;

[TestClass]
public sealed class GitFetcherTests
{
    [TestMethod]
    public void GetLocalDepDir_ReturnsNull_WhenDirDoesNotExist()
    {
        // Use a path that is guaranteed not to exist
        var depsRoot = Path.Combine(Path.GetTempPath(), $"dotori-nonexistent-{Guid.NewGuid():N}", "deps");
        var result   = GitFetcher.GetLocalDepDir(depsRoot, "fmt");
        Assert.IsNull(result, "Should return null when deps/fmt/ does not exist");
    }

    [TestMethod]
    public void GetLocalDepDir_ReturnsPath_WhenDirExistsWithFiles()
    {
        var tmp      = Path.Combine(Path.GetTempPath(), $"dotori-test-{Guid.NewGuid():N}");
        var depsRoot = Path.Combine(tmp, "deps");
        var fmtDir   = Path.Combine(depsRoot, "fmt");
        Directory.CreateDirectory(fmtDir);
        File.WriteAllText(Path.Combine(fmtDir, "dummy.txt"), "x");

        try
        {
            var result = GitFetcher.GetLocalDepDir(depsRoot, "fmt");
            Assert.AreEqual(fmtDir, result, "Should return path when deps/fmt/ exists with files");
        }
        finally
        {
            Directory.Delete(tmp, recursive: true);
        }
    }

    [TestMethod]
    public void GetLocalDepDir_ReturnsNull_WhenDirIsEmpty()
    {
        var tmp      = Path.Combine(Path.GetTempPath(), $"dotori-test-{Guid.NewGuid():N}");
        var depsRoot = Path.Combine(tmp, "deps");
        var fmtDir   = Path.Combine(depsRoot, "fmt");
        Directory.CreateDirectory(fmtDir);  // exists but empty

        try
        {
            var result = GitFetcher.GetLocalDepDir(depsRoot, "fmt");
            Assert.IsNull(result, "Should return null when deps/fmt/ is empty");
        }
        finally
        {
            Directory.Delete(tmp, recursive: true);
        }
    }
}
