using Dotori.Core.Build;
using Dotori.Core.Model;
using Dotori.Core.Parsing;
using Dotori.Core.Toolchain;

namespace Dotori.Tests.Build;

[TestClass]
public sealed class AssemblerJobTests
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

    private string CreateFile(string relPath, string content = "")
    {
        var full = Path.Combine(_tempDir, relPath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllText(full, content);
        return full;
    }

    private BuildPlanner MakePlanner(
        AssemblerConfig? asm,
        string targetId = "linux-x64",
        AssemblerPaths? asmPaths = null,
        CompilerKind kind = CompilerKind.Clang)
    {
        var model = new FlatProjectModel
        {
            Name       = "MyApp",
            ProjectDir = _tempDir,
            DotoriPath = Path.Combine(_tempDir, ".dotori"),
            Assembler  = asm,
        };
        var toolchain = new ToolchainInfo
        {
            Kind         = kind,
            CompilerPath = kind == CompilerKind.Msvc ? "cl.exe" : "clang++",
            LinkerPath   = kind == CompilerKind.Msvc ? "link.exe" : "clang++",
            TargetTriple = kind == CompilerKind.Msvc
                ? "x86_64-pc-windows-msvc"
                : "x86_64-unknown-linux-gnu",
            Assembler    = asmPaths,
        };
        return new BuildPlanner(model, toolchain, "debug", targetId);
    }

    // ─── No assembler block ───────────────────────────────────────────────────

    [TestMethod]
    public void PlanAssemblerJobs_NoBlock_ReturnsEmpty()
    {
        var planner = MakePlanner(asm: null);
        Assert.AreEqual(0, planner.PlanAssemblerJobs().Count);
        Assert.IsNull(planner.GetAssemblerPath());
    }

    // ─── Missing executable ───────────────────────────────────────────────────

    [TestMethod]
    public void PlanAssemblerJobs_ToolNotFound_ReturnsEmpty()
    {
        CreateFile("src/hello.asm");

        var config = new AssemblerConfig { Tool = AssemblerTool.Nasm };
        config.Items.Add(new SourceItem(true, "src/**/*.asm"));

        // asmPaths with no NASM path
        var planner = MakePlanner(config, asmPaths: new AssemblerPaths());
        Assert.AreEqual(0, planner.PlanAssemblerJobs().Count);
    }

    // ─── NASM jobs ────────────────────────────────────────────────────────────

    [TestMethod]
    public void PlanAssemblerJobs_Nasm_CreatesJobs()
    {
        CreateFile("src/hello.asm");
        CreateFile("src/world.asm");

        var config = new AssemblerConfig { Tool = AssemblerTool.Nasm, Format = "elf64" };
        config.Items.Add(new SourceItem(true, "src/**/*.asm"));

        var planner = MakePlanner(config,
            asmPaths: new AssemblerPaths { NasmPath = "/usr/bin/nasm" });

        var jobs = planner.PlanAssemblerJobs();
        Assert.AreEqual(2, jobs.Count);
        Assert.AreEqual("/usr/bin/nasm", planner.GetAssemblerPath());

        // Each job arg must contain the format flag and output file
        foreach (var job in jobs)
        {
            Assert.IsTrue(job.Args.Any(a => a.Contains("-f elf64")),
                "Expected -f elf64 in args");
            Assert.IsTrue(job.Args.Any(a => a.StartsWith("-o ")),
                "Expected -o flag in args");
            Assert.IsTrue(job.OutputFile.EndsWith(".o"));
        }
    }

    [TestMethod]
    public void PlanAssemblerJobs_Nasm_AutoDetectsLinuxFormat()
    {
        CreateFile("src/hello.asm");

        var config = new AssemblerConfig { Tool = AssemblerTool.Nasm };  // no explicit format
        config.Items.Add(new SourceItem(true, "src/**/*.asm"));

        var planner = MakePlanner(config,
            targetId: "linux-x64",
            asmPaths: new AssemblerPaths { NasmPath = "/usr/bin/nasm" });

        var jobs = planner.PlanAssemblerJobs();
        Assert.AreEqual(1, jobs.Count);
        Assert.IsTrue(jobs[0].Args.Any(a => a.Contains("-f elf64")),
            "Expected auto-detected elf64 format");
    }

    [TestMethod]
    public void PlanAssemblerJobs_Nasm_AutoDetectsMacosFormat()
    {
        CreateFile("src/hello.asm");

        var config = new AssemblerConfig { Tool = AssemblerTool.Nasm };
        config.Items.Add(new SourceItem(true, "src/**/*.asm"));

        var planner = MakePlanner(config,
            targetId: "macos-arm64",
            asmPaths: new AssemblerPaths { NasmPath = "/usr/local/bin/nasm" });

        var jobs = planner.PlanAssemblerJobs();
        Assert.AreEqual(1, jobs.Count);
        Assert.IsTrue(jobs[0].Args.Any(a => a.Contains("-f macho64")),
            "Expected auto-detected macho64 format");
    }

    // ─── GAS jobs ─────────────────────────────────────────────────────────────

    [TestMethod]
    public void PlanAssemblerJobs_Gas_NoFormatFlag()
    {
        CreateFile("src/hello.s");

        var config = new AssemblerConfig { Tool = AssemblerTool.Gas };
        config.Items.Add(new SourceItem(true, "src/**/*.s"));

        var planner = MakePlanner(config,
            asmPaths: new AssemblerPaths { GasPath = "/usr/bin/as" });

        var jobs = planner.PlanAssemblerJobs();
        Assert.AreEqual(1, jobs.Count);
        Assert.IsFalse(jobs[0].Args.Any(a => a.StartsWith("-f ")),
            "GAS should not have -f format flag");
        Assert.AreEqual("/usr/bin/as", planner.GetAssemblerPath());
    }

    // ─── MASM jobs ────────────────────────────────────────────────────────────

    [TestMethod]
    public void PlanAssemblerJobs_Masm_WindowsFlags()
    {
        CreateFile("src/hello.asm");

        var config = new AssemblerConfig { Tool = AssemblerTool.Masm };
        config.Items.Add(new SourceItem(true, "src/**/*.asm"));

        var planner = MakePlanner(config,
            targetId: "windows-x64",
            kind: CompilerKind.Msvc,
            asmPaths: new AssemblerPaths { MasmPath = @"C:\VC\bin\ml64.exe" });

        var jobs = planner.PlanAssemblerJobs();
        Assert.AreEqual(1, jobs.Count);
        Assert.IsTrue(jobs[0].Args.Any(a => a == "/nologo"),  "Expected /nologo");
        Assert.IsTrue(jobs[0].Args.Any(a => a == "/c"),       "Expected /c");
        Assert.IsTrue(jobs[0].OutputFile.EndsWith(".obj"),     "MASM output should be .obj");
    }

    // ─── Auto tool selection ──────────────────────────────────────────────────

    [TestMethod]
    public void PlanAssemblerJobs_AutoTool_MsvcChoosesMasm()
    {
        CreateFile("src/hello.asm");

        var config = new AssemblerConfig { Tool = AssemblerTool.Auto };
        config.Items.Add(new SourceItem(true, "src/**/*.asm"));

        var planner = MakePlanner(config,
            targetId: "windows-x64",
            kind: CompilerKind.Msvc,
            asmPaths: new AssemblerPaths { MasmPath = @"C:\VC\bin\ml64.exe" });

        Assert.AreEqual(@"C:\VC\bin\ml64.exe", planner.GetAssemblerPath());
    }

    [TestMethod]
    public void PlanAssemblerJobs_AutoTool_ClangChoosesGas()
    {
        CreateFile("src/hello.s");

        var config = new AssemblerConfig { Tool = AssemblerTool.Auto };
        config.Items.Add(new SourceItem(true, "src/**/*.s"));

        var planner = MakePlanner(config,
            targetId: "linux-x64",
            kind: CompilerKind.Clang,
            asmPaths: new AssemblerPaths { GasPath = "/usr/bin/as" });

        Assert.AreEqual("/usr/bin/as", planner.GetAssemblerPath());
    }

    // ─── Defines and flags pass-through ──────────────────────────────────────

    [TestMethod]
    public void NasmDriver_DefinesAndFlags_InArgs()
    {
        CreateFile("src/hello.asm");

        var config = new AssemblerConfig { Tool = AssemblerTool.Nasm };
        config.Items.Add(new SourceItem(true, "src/**/*.asm"));
        config.Defines.Add("DEBUG");
        config.Flags.Add("-g");

        var planner = MakePlanner(config,
            targetId: "linux-x64",
            asmPaths: new AssemblerPaths { NasmPath = "/usr/bin/nasm" });

        var jobs = planner.PlanAssemblerJobs();
        Assert.AreEqual(1, jobs.Count);
        Assert.IsTrue(jobs[0].Args.Any(a => a == "-DDEBUG"),  "Expected -DDEBUG");
        Assert.IsTrue(jobs[0].Args.Any(a => a == "-g"),       "Expected -g flag");
    }
}
