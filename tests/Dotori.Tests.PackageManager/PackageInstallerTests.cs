using Dotori.PackageManager;

namespace Dotori.Tests.PackageManager;

[TestClass]
public sealed class PackageInstallerTests
{
    [TestMethod]
    public void GetLocalDepDir_ReturnsCorrectPath()
    {
        var depsRoot = "/proj/deps";
        var result   = PackageInstaller.GetLocalDepDir(depsRoot, "spdlog");
        Assert.AreEqual(Path.Combine(depsRoot, "spdlog"), result);
    }

    [TestMethod]
    public void IsInstalled_ReturnsFalse_WhenDirMissing()
    {
        var depsRoot = Path.Combine(Path.GetTempPath(), $"dotori-test-{Guid.NewGuid():N}", "deps");
        Assert.IsFalse(PackageInstaller.IsInstalled(depsRoot, "spdlog"),
            "Should return false when deps/spdlog/ does not exist");
    }

    [TestMethod]
    public void IsInstalled_ReturnsFalse_WhenDirExistsButNoDotoriFile()
    {
        var tmp      = Path.Combine(Path.GetTempPath(), $"dotori-test-{Guid.NewGuid():N}");
        var depsRoot = Path.Combine(tmp, "deps");
        Directory.CreateDirectory(Path.Combine(depsRoot, "spdlog"));

        try
        {
            Assert.IsFalse(PackageInstaller.IsInstalled(depsRoot, "spdlog"),
                "Should return false when deps/spdlog/.dotori is absent");
        }
        finally
        {
            Directory.Delete(tmp, recursive: true);
        }
    }

    [TestMethod]
    public void IsInstalled_ReturnsTrue_WhenDotoriFilePresent()
    {
        var tmp      = Path.Combine(Path.GetTempPath(), $"dotori-test-{Guid.NewGuid():N}");
        var depsRoot = Path.Combine(tmp, "deps");
        var pkgDir   = Path.Combine(depsRoot, "spdlog");
        Directory.CreateDirectory(pkgDir);
        File.WriteAllText(Path.Combine(pkgDir, ".dotori"), "project Spdlog {}");

        try
        {
            Assert.IsTrue(PackageInstaller.IsInstalled(depsRoot, "spdlog"),
                "Should return true when deps/spdlog/.dotori exists");
        }
        finally
        {
            Directory.Delete(tmp, recursive: true);
        }
    }
}
