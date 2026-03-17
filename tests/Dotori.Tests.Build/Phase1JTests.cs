using System.Text.Json;
using Dotori.Core.Build;
using Dotori.Core.Model;
using Dotori.Core.Parsing;
using Dotori.Core.Toolchain;

namespace Dotori.Tests.Build;

/// <summary>Tests for Phase 1-J: output copy, module-map.json, and build script env vars.</summary>
[TestClass]
public sealed class CopyArtifactsTests
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

    private BuildPlanner MakePlanner(
        ProjectType type,
        OutputConfig? output,
        string targetId = "linux-x64",
        string config   = "debug")
    {
        var model = new FlatProjectModel
        {
            Name       = "TestProject",
            ProjectDir = _tempDir,
            DotoriPath = Path.Combine(_tempDir, ".dotori"),
            Type       = type,
            Output     = output,
        };

        // Minimal fake toolchain (we won't actually compile)
        var toolchain = new ToolchainInfo
        {
            Kind         = CompilerKind.Clang,
            CompilerPath = "clang++",
            LinkerPath   = "clang++",
            TargetTriple = "x86_64-unknown-linux-gnu",
        };

        return new BuildPlanner(model, toolchain, config, targetId);
    }

    // ── CopyArtifacts: no Output config ────────────────────────────────────

    [TestMethod]
    public void CopyArtifacts_NullOutput_DoesNothing()
    {
        var planner = MakePlanner(ProjectType.Executable, output: null);

        var exePath = Path.Combine(_tempDir, "fake_exe");
        File.WriteAllText(exePath, "fake");

        // Must not throw and must not create any new files
        planner.CopyArtifacts(exePath);
        Assert.HasCount(1, Directory.GetFiles(_tempDir));  // only the original
    }

    [TestMethod]
    public void CopyArtifacts_MissingSourceFile_DoesNothing()
    {
        var planner = MakePlanner(ProjectType.Executable, new OutputConfig { Binaries = "bin/" });
        planner.CopyArtifacts(Path.Combine(_tempDir, "does_not_exist"));

        Assert.IsFalse(Directory.Exists(Path.Combine(_tempDir, "bin")));
    }

    // ── CopyArtifacts: executable ───────────────────────────────────────────

    [TestMethod]
    public void CopyArtifacts_Executable_CopiesToBinaries()
    {
        var binDir  = Path.Combine(_tempDir, "bin");
        var planner = MakePlanner(ProjectType.Executable, new OutputConfig { Binaries = "bin/" });

        var exePath = Path.Combine(_tempDir, "myapp");
        File.WriteAllText(exePath, "fake exe");

        planner.CopyArtifacts(exePath);

        Assert.IsTrue(Directory.Exists(binDir));
        Assert.IsTrue(File.Exists(Path.Combine(binDir, "myapp")));
    }

    [TestMethod]
    public void CopyArtifacts_Executable_CreatesDestDirIfMissing()
    {
        var destDir = Path.Combine(_tempDir, "nested", "out");
        var planner = MakePlanner(ProjectType.Executable,
            new OutputConfig { Binaries = Path.Combine("nested", "out") });

        var exePath = Path.Combine(_tempDir, "myapp");
        File.WriteAllText(exePath, "fake exe");

        planner.CopyArtifacts(exePath);

        Assert.IsTrue(File.Exists(Path.Combine(destDir, "myapp")));
    }

    [TestMethod]
    public void CopyArtifacts_Executable_AbsolutePath()
    {
        var absDir  = Path.Combine(_tempDir, "abs_out");
        var planner = MakePlanner(ProjectType.Executable, new OutputConfig { Binaries = absDir });

        var exePath = Path.Combine(_tempDir, "myapp");
        File.WriteAllText(exePath, "fake exe");

        planner.CopyArtifacts(exePath);

        Assert.IsTrue(File.Exists(Path.Combine(absDir, "myapp")));
    }

    // ── CopyArtifacts: static library ──────────────────────────────────────

    [TestMethod]
    public void CopyArtifacts_StaticLibrary_CopiesToLibraries()
    {
        var libDir  = Path.Combine(_tempDir, "lib");
        var planner = MakePlanner(ProjectType.StaticLibrary, new OutputConfig { Libraries = "lib/" });

        var libPath = Path.Combine(_tempDir, "libfoo.a");
        File.WriteAllText(libPath, "fake lib");

        planner.CopyArtifacts(libPath);

        Assert.IsTrue(File.Exists(Path.Combine(libDir, "libfoo.a")));
    }

    [TestMethod]
    public void CopyArtifacts_StaticLibrary_BinariesNotUsed()
    {
        var planner = MakePlanner(ProjectType.StaticLibrary,
            new OutputConfig { Binaries = "bin/", Libraries = "lib/" });

        var libPath = Path.Combine(_tempDir, "libfoo.a");
        File.WriteAllText(libPath, "fake lib");

        planner.CopyArtifacts(libPath);

        // Static lib should go to libraries, NOT binaries
        Assert.IsFalse(Directory.Exists(Path.Combine(_tempDir, "bin")));
        Assert.IsTrue(File.Exists(Path.Combine(_tempDir, "lib", "libfoo.a")));
    }

    // ── CopyArtifacts: Windows PDB ─────────────────────────────────────────

    [TestMethod]
    public void CopyArtifacts_WindowsExe_PdbCopiedToSymbols()
    {
        var planner = MakePlanner(ProjectType.Executable,
            new OutputConfig { Binaries = "bin/", Symbols = "pdb/" },
            targetId: "windows-x64");

        var exePath = Path.Combine(_tempDir, "myapp.exe");
        var pdbPath = Path.Combine(_tempDir, "myapp.pdb");
        File.WriteAllText(exePath, "fake exe");
        File.WriteAllText(pdbPath, "fake pdb");

        planner.CopyArtifacts(exePath);

        Assert.IsTrue(File.Exists(Path.Combine(_tempDir, "bin", "myapp.exe")));
        Assert.IsTrue(File.Exists(Path.Combine(_tempDir, "pdb", "myapp.pdb")));
    }

    [TestMethod]
    public void CopyArtifacts_WindowsExe_NoPdb_SymbolsNotCreated()
    {
        var planner = MakePlanner(ProjectType.Executable,
            new OutputConfig { Binaries = "bin/", Symbols = "pdb/" },
            targetId: "windows-x64");

        var exePath = Path.Combine(_tempDir, "myapp.exe");
        File.WriteAllText(exePath, "fake exe");
        // No .pdb file created

        planner.CopyArtifacts(exePath);

        Assert.IsTrue(File.Exists(Path.Combine(_tempDir, "bin", "myapp.exe")));
        Assert.IsFalse(Directory.Exists(Path.Combine(_tempDir, "pdb")));
    }

    // ── CopyArtifacts: Windows DLL import lib ──────────────────────────────

    [TestMethod]
    public void CopyArtifacts_WindowsDll_ImportLibCopiedToLibraries()
    {
        var planner = MakePlanner(ProjectType.SharedLibrary,
            new OutputConfig { Binaries = "bin/", Libraries = "lib/" },
            targetId: "windows-x64");

        var dllPath    = Path.Combine(_tempDir, "mylib.dll");
        var importPath = Path.Combine(_tempDir, "mylib.lib");
        File.WriteAllText(dllPath,    "fake dll");
        File.WriteAllText(importPath, "fake import lib");

        planner.CopyArtifacts(dllPath);

        Assert.IsTrue(File.Exists(Path.Combine(_tempDir, "bin", "mylib.dll")));
        Assert.IsTrue(File.Exists(Path.Combine(_tempDir, "lib", "mylib.lib")));
    }
}

[TestClass]
public sealed class ModuleMapWriterTests
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

    private static CompileJob MakeJob(string sourcePath, string bmiPath)
        => new() { SourceFile = sourcePath, OutputFile = bmiPath, Args = [] };

    // ── WriteModuleMap: basic behavior ─────────────────────────────────────

    [TestMethod]
    public void WriteModuleMap_EmptyJobs_DoesNotCreateFile()
    {
        var bmiDir = Path.Combine(_tempDir, "bmi");
        ModuleMapWriter.Write([], "linux-x64", "debug", bmiDir);
        Assert.IsFalse(File.Exists(Path.Combine(bmiDir, "module-map.json")));
    }

    [TestMethod]
    public void WriteModuleMap_CreatesFile()
    {
        var bmiDir  = Path.Combine(_tempDir, "bmi");
        var srcFile = CreateModuleSource("export module MyLib;");
        var bmiFile = Path.Combine(bmiDir, "MyLib.pcm");

        ModuleMapWriter.Write([MakeJob(srcFile, bmiFile)], "macos-arm64", "debug", bmiDir);

        Assert.IsTrue(File.Exists(Path.Combine(bmiDir, "module-map.json")));
    }

    [TestMethod]
    public void WriteModuleMap_Json_ContainsCorrectFields()
    {
        var bmiDir  = Path.Combine(_tempDir, "bmi");
        var srcFile = CreateModuleSource("export module MyLib;");
        var bmiFile = Path.Combine(bmiDir, "MyLib.pcm");

        ModuleMapWriter.Write([MakeJob(srcFile, bmiFile)], "macos-arm64", "release", bmiDir);

        var json = File.ReadAllText(Path.Combine(bmiDir, "module-map.json"));
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.AreEqual(1,           root.GetProperty("version").GetInt32());
        Assert.AreEqual("macos-arm64", root.GetProperty("target").GetString());
        Assert.AreEqual("release",   root.GetProperty("config").GetString());

        var modules = root.GetProperty("modules");
        Assert.AreEqual(1, modules.GetArrayLength());

        var entry = modules[0];
        Assert.AreEqual("MyLib",  entry.GetProperty("logical-name").GetString());
        Assert.AreEqual(srcFile,  entry.GetProperty("source-file").GetString());
        Assert.AreEqual(bmiFile,  entry.GetProperty("bmi-path").GetString());
    }

    [TestMethod]
    public void WriteModuleMap_MultipleModules_AllPresent()
    {
        var bmiDir = Path.Combine(_tempDir, "bmi");
        var src1   = CreateModuleSource("export module ModA;");
        var src2   = CreateModuleSource("export module ModB;");
        var bmi1   = Path.Combine(bmiDir, "ModA.pcm");
        var bmi2   = Path.Combine(bmiDir, "ModB.pcm");

        ModuleMapWriter.Write(
            [MakeJob(src1, bmi1), MakeJob(src2, bmi2)],
            "linux-x64", "debug", bmiDir);

        var json = File.ReadAllText(Path.Combine(bmiDir, "module-map.json"));
        using var doc = JsonDocument.Parse(json);
        var modules = doc.RootElement.GetProperty("modules");

        Assert.AreEqual(2, modules.GetArrayLength());
        var names = Enumerable.Range(0, 2)
            .Select(i => modules[i].GetProperty("logical-name").GetString())
            .ToHashSet();
        Assert.Contains("ModA", names);
        Assert.Contains("ModB", names);
    }

    [TestMethod]
    public void WriteModuleMap_OverwritesExistingFile()
    {
        var bmiDir  = Path.Combine(_tempDir, "bmi");
        Directory.CreateDirectory(bmiDir);
        var mapPath = Path.Combine(bmiDir, "module-map.json");
        File.WriteAllText(mapPath, "old content");

        var srcFile = CreateModuleSource("export module NewMod;");
        var bmiFile = Path.Combine(bmiDir, "NewMod.pcm");

        ModuleMapWriter.Write([MakeJob(srcFile, bmiFile)], "linux-x64", "debug", bmiDir);

        var content = File.ReadAllText(mapPath);
        Assert.DoesNotStartWith("old", content);
        Assert.Contains("NewMod", content);
    }

    private string CreateModuleSource(string content)
    {
        var path = Path.Combine(_tempDir, Path.GetRandomFileName() + ".cppm");
        File.WriteAllText(path, content);
        return path;
    }
}

[TestClass]
public sealed class BuildPlannerWriteModuleMapTests
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

    private BuildPlanner MakePlanner(bool exportMap = true)
    {
        var model = new FlatProjectModel
        {
            Name            = "TestProject",
            ProjectDir      = _tempDir,
            DotoriPath      = Path.Combine(_tempDir, ".dotori"),
            Type            = ProjectType.StaticLibrary,
            ModuleExportMap = exportMap,
        };
        var toolchain = new ToolchainInfo
        {
            Kind         = CompilerKind.Clang,
            CompilerPath = "clang++",
            LinkerPath   = "clang++",
            TargetTriple = "x86_64-unknown-linux-gnu",
        };
        return new BuildPlanner(model, toolchain, "debug", "linux-x64");
    }

    [TestMethod]
    public void WriteModuleMap_WhenExportMapTrue_CreatesJson()
    {
        var planner = MakePlanner(exportMap: true);

        var srcFile = Path.Combine(_tempDir, "mod.cppm");
        File.WriteAllText(srcFile, "export module Foo;");

        var bmiDir = Path.Combine(_tempDir, ".dotori-cache", "obj", "linux-x64-debug", "bmi");
        Directory.CreateDirectory(bmiDir);
        var bmiFile = Path.Combine(bmiDir, "Foo.pcm");

        var jobs = new[] { new CompileJob { SourceFile = srcFile, OutputFile = bmiFile, Args = [] } };
        planner.WriteModuleMap(jobs);

        Assert.IsTrue(File.Exists(Path.Combine(bmiDir, "module-map.json")));
    }

    [TestMethod]
    public void WriteModuleMap_WhenExportMapFalse_DoesNotCreate()
    {
        var planner = MakePlanner(exportMap: false);

        var srcFile = Path.Combine(_tempDir, "mod.cppm");
        File.WriteAllText(srcFile, "export module Foo;");

        var bmiDir = Path.Combine(_tempDir, ".dotori-cache", "obj", "linux-x64-debug", "bmi");
        Directory.CreateDirectory(bmiDir);
        var bmiFile = Path.Combine(bmiDir, "Foo.pcm");

        var jobs = new[] { new CompileJob { SourceFile = srcFile, OutputFile = bmiFile, Args = [] } };
        planner.WriteModuleMap(jobs);

        Assert.IsFalse(File.Exists(Path.Combine(bmiDir, "module-map.json")));
    }
}
