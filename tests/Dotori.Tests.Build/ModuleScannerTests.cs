using Dotori.Core.Build;

namespace Dotori.Tests.Build;

[TestClass]
public sealed class ModuleScannerTests
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

    private string CreateFile(string name, string content)
    {
        var path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, content);
        return path;
    }

    // ─── ScanByText ───────────────────────────────────────────────────────────

    [TestMethod]
    public void ScanByText_ExportModule_DetectsProvides()
    {
        var file = CreateFile("mylib.cppm", """
            export module MyLib;
            import std.core;
            """);

        var dep = ModuleScanner.ScanByText(file);

        Assert.AreEqual("MyLib", dep.Provides);
        Assert.HasCount(1, dep.Requires);
        Assert.AreEqual("std.core", dep.Requires[0]);
    }

    [TestMethod]
    public void ScanByText_ImportOnly_NoProvides()
    {
        var file = CreateFile("main.cpp", """
            import MyLib;
            import fmt;
            """);

        var dep = ModuleScanner.ScanByText(file);

        Assert.IsNull(dep.Provides);
        Assert.HasCount(2, dep.Requires);
        Assert.IsTrue(dep.Requires.Contains("MyLib"));
        Assert.IsTrue(dep.Requires.Contains("fmt"));
    }

    [TestMethod]
    public void ScanByText_HeaderImport_Skipped()
    {
        var file = CreateFile("main.cpp", """
            import <vector>;
            import MyLib;
            """);

        var dep = ModuleScanner.ScanByText(file);

        Assert.HasCount(1, dep.Requires);
        Assert.AreEqual("MyLib", dep.Requires[0]);
    }

    [TestMethod]
    public void ScanByText_PartitionImport_Skipped()
    {
        var file = CreateFile("part.cppm", """
            export module MyLib:Part;
            import :OtherPart;
            """);

        var dep = ModuleScanner.ScanByText(file);

        // ":OtherPart" should be skipped (starts with ':')
        Assert.IsEmpty(dep.Requires);
    }

    [TestMethod]
    public void ScanByText_EmptyFile_NoDeps()
    {
        var file = CreateFile("empty.cpp", string.Empty);

        var dep = ModuleScanner.ScanByText(file);

        Assert.IsNull(dep.Provides);
        Assert.IsEmpty(dep.Requires);
    }

    [TestMethod]
    public void ScanByText_NonexistentFile_ReturnsEmptyDep()
    {
        var dep = ModuleScanner.ScanByText(Path.Combine(_tempDir, "nonexistent.cppm"));

        Assert.IsNull(dep.Provides);
        Assert.IsEmpty(dep.Requires);
    }

    [TestMethod]
    public void ScanByText_ModuleGlobalFragment_NotProvides()
    {
        // "module;" starts the global module fragment — not a named module
        var file = CreateFile("compat.cpp", """
            module;
            #include <cstdio>
            export module Compat;
            """);

        var dep = ModuleScanner.ScanByText(file);

        Assert.AreEqual("Compat", dep.Provides);
    }

    // ─── ParseP1689 ───────────────────────────────────────────────────────────

    [TestMethod]
    public void ParseP1689_ValidJson_ParsesCorrectly()
    {
        const string json = """
            {
              "version": 1,
              "rules": [
                {
                  "primary-output": "MyLib.pcm",
                  "provides": [{ "logical-name": "MyLib", "is-interface": true }],
                  "requires": [
                    { "logical-name": "std.core" },
                    { "logical-name": "utils" }
                  ]
                }
              ]
            }
            """;

        var dep = ModuleScanner.ParseP1689("mylib.cppm", json);

        Assert.IsNotNull(dep);
        Assert.AreEqual("MyLib", dep!.Provides);
        Assert.HasCount(2, dep.Requires);
        Assert.IsTrue(dep.Requires.Contains("std.core"));
        Assert.IsTrue(dep.Requires.Contains("utils"));
    }

    [TestMethod]
    public void ParseP1689_NoProvides_ProvidesIsNull()
    {
        const string json = """
            {
              "version": 1,
              "rules": [
                {
                  "primary-output": "main.o",
                  "requires": [{ "logical-name": "MyLib" }]
                }
              ]
            }
            """;

        var dep = ModuleScanner.ParseP1689("main.cpp", json);

        Assert.IsNotNull(dep);
        Assert.IsNull(dep!.Provides);
        Assert.HasCount(1, dep.Requires);
    }

    [TestMethod]
    public void ParseP1689_NoRules_ReturnsNull()
    {
        const string json = """{ "version": 1 }""";

        var dep = ModuleScanner.ParseP1689("file.cppm", json);

        Assert.IsNull(dep);
    }

    [TestMethod]
    public void ParseP1689_MalformedJson_FallsBackToTextScan()
    {
        // File must exist for ScanByText fallback to return non-null
        var file = CreateFile("mylib.cppm", "export module MyLib;");

        var dep = ModuleScanner.ParseP1689(file, "NOT JSON {{{");

        // Falls back to ScanByText — should detect the export module
        Assert.IsNotNull(dep);
        Assert.AreEqual("MyLib", dep!.Provides);
    }

    [TestMethod]
    public void ParseP1689_EmptyRulesArray_ReturnsNull()
    {
        const string json = """{ "version": 1, "rules": [] }""";

        var dep = ModuleScanner.ParseP1689("file.cppm", json);

        Assert.IsNull(dep);
    }
}
