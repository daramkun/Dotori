using Dotori.Core.Build;

namespace Dotori.Tests.Build;

[TestClass]
public sealed class UnityBatcherTests
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

    private List<string> MakeSources(params string[] names)
    {
        var result = new List<string>();
        foreach (var name in names)
        {
            var path = Path.Combine(_tempDir, name);
            File.WriteAllText(path, $"// {name}");
            result.Add(path);
        }
        return result;
    }

    [TestMethod]
    public void CreateBatches_AllCpp_AllInOneUnity()
    {
        var sources = MakeSources("a.cpp", "b.cpp", "c.cpp");
        var unityDir = Path.Combine(_tempDir, "unity");

        var (unityFiles, nonUnity) = UnityBatcher.CreateBatches(
            sources, [], [], batchSize: 8, unityDir);

        Assert.AreEqual(1, unityFiles.Count);
        Assert.AreEqual(0, nonUnity.Count);
    }

    [TestMethod]
    public void CreateBatches_BatchSizeRespected()
    {
        var sources = MakeSources("a.cpp", "b.cpp", "c.cpp", "d.cpp", "e.cpp");
        var unityDir = Path.Combine(_tempDir, "unity");

        var (unityFiles, nonUnity) = UnityBatcher.CreateBatches(
            sources, [], [], batchSize: 2, unityDir);

        // 5 files, batch size 2 → 3 batches (2+2+1)
        Assert.AreEqual(3, unityFiles.Count);
    }

    [TestMethod]
    public void CreateBatches_ModuleFilesExcluded()
    {
        var cppm = MakeSources("MyMod.cppm");
        var sources = MakeSources("a.cpp", "b.cpp");
        sources.AddRange(cppm);

        var unityDir = Path.Combine(_tempDir, "unity");

        var (unityFiles, nonUnity) = UnityBatcher.CreateBatches(
            sources, cppm, [], batchSize: 8, unityDir);

        // .cppm should be in nonUnity, .cpp in unity
        Assert.AreEqual(1, unityFiles.Count);
        Assert.IsTrue(nonUnity.Any(f => f.EndsWith(".cppm")));
    }

    [TestMethod]
    public void CreateBatches_UnityFileContainsIncludes()
    {
        var sources = MakeSources("a.cpp", "b.cpp");
        var unityDir = Path.Combine(_tempDir, "unity");

        var (unityFiles, _) = UnityBatcher.CreateBatches(
            sources, [], [], batchSize: 8, unityDir);

        var content = File.ReadAllText(unityFiles[0]);
        Assert.IsTrue(content.Contains("#include"));
        Assert.IsTrue(content.Contains("a.cpp"));
        Assert.IsTrue(content.Contains("b.cpp"));
    }
}
