using Dotori.PackageManager;

namespace Dotori.Tests.PackageManager;

[TestClass]
public sealed class LockManagerTests
{
    // ─── Serialization / Parsing ─────────────────────────────────────────────

    [TestMethod]
    public void Serialize_EmptyLock_ContainsVersion()
    {
        var lockFile = new LockFile { LockVersion = 1 };
        var text = LockManager.Serialize(lockFile);
        StringAssert.Contains(text, "lock-version = 1");
    }

    [TestMethod]
    public void Serialize_WithPackage_ContainsPackageSection()
    {
        var lockFile = new LockFile();
        lockFile.Packages.Add(new LockEntry
        {
            Name    = "fmt",
            Version = "10.2.0",
            Source  = "git+https://github.com/fmtlib/fmt#v10.2.0",
            Hash    = "sha256:abcdef",
        });

        var text = LockManager.Serialize(lockFile);
        StringAssert.Contains(text, "[[package]]");
        StringAssert.Contains(text, "name    = \"fmt\"");
        StringAssert.Contains(text, "version = \"10.2.0\"");
        StringAssert.Contains(text, "hash    = \"sha256:abcdef\"");
    }

    [TestMethod]
    public void Serialize_WithDeps_ContainsDepsArray()
    {
        var lockFile = new LockFile();
        var entry = new LockEntry
        {
            Name    = "spdlog",
            Version = "1.13.0",
            Source  = "git+https://github.com/gabime/spdlog#v1.13.0",
        };
        entry.Deps.Add("fmt@10.2.0");
        lockFile.Packages.Add(entry);

        var text = LockManager.Serialize(lockFile);
        StringAssert.Contains(text, "deps    = [\"fmt@10.2.0\"]");
    }

    [TestMethod]
    public void Parse_SerializedLock_RoundTrips()
    {
        var original = new LockFile { LockVersion = 1 };
        var entry = new LockEntry
        {
            Name    = "fmt",
            Version = "10.2.0",
            Source  = "git+https://github.com/fmtlib/fmt#v10.2.0",
            Hash    = "sha256:abcdef",
        };
        entry.Deps.Add("some-dep@1.0");
        original.Packages.Add(entry);

        var text    = LockManager.Serialize(original);
        var parsed  = LockManager.Parse(text);

        Assert.AreEqual(1, parsed.LockVersion);
        Assert.HasCount(1, parsed.Packages);

        var pkg = parsed.Packages[0];
        Assert.AreEqual("fmt",       pkg.Name);
        Assert.AreEqual("10.2.0",    pkg.Version);
        Assert.AreEqual("sha256:abcdef", pkg.Hash);
        Assert.HasCount(1, pkg.Deps);
        Assert.AreEqual("some-dep@1.0", pkg.Deps[0]);
    }

    [TestMethod]
    public void Parse_MultiplePackages_ParsesAll()
    {
        var text = @"
lock-version = 1

[[package]]
name    = ""fmt""
version = ""10.2.0""
source  = ""git+https://github.com/fmtlib/fmt#v10.2.0""

[[package]]
name    = ""spdlog""
version = ""1.13.0""
source  = ""git+https://github.com/gabime/spdlog#v1.13.0""
deps    = [""fmt@10.2.0""]
";
        var parsed = LockManager.Parse(text);
        Assert.HasCount(2, parsed.Packages);
        Assert.AreEqual("fmt",    parsed.Packages[0].Name);
        Assert.AreEqual("spdlog", parsed.Packages[1].Name);
        Assert.HasCount(1, parsed.Packages[1].Deps);
    }

    // ─── File I/O ────────────────────────────────────────────────────────────

    [TestMethod]
    public void SaveAndLoad_RoundTrips()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            var lockFile = new LockFile { LockVersion = 1 };
            lockFile.Packages.Add(new LockEntry
            {
                Name    = "mylib",
                Version = "2.0.0",
                Source  = "git+https://example.com/mylib#v2.0.0",
            });

            LockManager.Save(lockFile, tempDir);
            Assert.IsTrue(File.Exists(Path.Combine(tempDir, ".dotori.lock")));

            var loaded = LockManager.Load(tempDir);
            Assert.HasCount(1, loaded.Packages);
            Assert.AreEqual("mylib", loaded.Packages[0].Name);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public void Load_MissingFile_ReturnsEmpty()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            var loaded = LockManager.Load(tempDir);
            Assert.IsEmpty(loaded.Packages);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }
}
