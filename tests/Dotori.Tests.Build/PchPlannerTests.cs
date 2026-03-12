using Dotori.Core.Build;
using Dotori.Core.Model;
using Dotori.Core.Toolchain;

namespace Dotori.Tests.Build;

[TestClass]
public sealed class PchPlannerTests
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

    private FlatProjectModel MakeModel(string? pchHeader = null, string? pchSource = null)
    {
        var model = new FlatProjectModel
        {
            Name       = "MyApp",
            ProjectDir = _tempDir,
            DotoriPath = Path.Combine(_tempDir, ".dotori"),
        };

        if (pchHeader is not null)
        {
            model.Pch = new PchConfig
            {
                Header = pchHeader,
                Source = pchSource,
            };
        }

        return model;
    }

    private static ToolchainInfo MakeToolchain(CompilerKind kind)
    {
        return new ToolchainInfo
        {
            Kind         = kind,
            CompilerPath = kind == CompilerKind.Msvc ? "cl.exe" : "clang++",
            LinkerPath   = kind == CompilerKind.Msvc ? "link.exe" : "clang++",
            TargetTriple = kind == CompilerKind.Msvc ? "x86_64-pc-windows-msvc" : "x86_64-unknown-linux-gnu",
        };
    }

    // ─── No PCH ───────────────────────────────────────────────────────────────

    [TestMethod]
    public void Plan_NoPchConfig_ReturnsNull()
    {
        var model    = MakeModel();  // no pch
        var toolchain = MakeToolchain(CompilerKind.Clang);

        var plan = PchPlanner.Plan(model, toolchain, Array.Empty<string>());

        Assert.IsNull(plan);
    }

    [TestMethod]
    public void Plan_PchHeaderNotFound_ReturnsNull()
    {
        var model    = MakeModel(pchHeader: "src/pch.h");  // file doesn't exist
        var toolchain = MakeToolchain(CompilerKind.Clang);

        var plan = PchPlanner.Plan(model, toolchain, Array.Empty<string>());

        Assert.IsNull(plan);
    }

    // ─── Clang PCH ───────────────────────────────────────────────────────────

    [TestMethod]
    public void Plan_ClangPch_ProducesPrecompileJob()
    {
        CreateFile("src/pch.h", "#pragma once\n#include <vector>");
        var model    = MakeModel(pchHeader: "src/pch.h");
        var toolchain = MakeToolchain(CompilerKind.Clang);

        var plan = PchPlanner.Plan(model, toolchain, new[] { "-std=c++23", "-c" });

        Assert.IsNotNull(plan);
        Assert.IsNotNull(plan!.BuildJob);
        Assert.EndsWith(".pch", plan.PchFile, "PCH file should end with .pch");

        var args = string.Join(" ", plan.BuildJob!.Args);
        Assert.Contains("-x c++-header", args, "Should contain -x c++-header");
        Assert.Contains("-o", args, "Should contain -o");
        Assert.IsFalse(plan.BuildJob!.Args.Contains("-c"), "Should not contain -c as standalone flag (removed)");
    }

    [TestMethod]
    public void Plan_ClangPch_UseFlagsContainIncludePch()
    {
        CreateFile("src/pch.h");
        var model    = MakeModel(pchHeader: "src/pch.h");
        var toolchain = MakeToolchain(CompilerKind.Clang);

        var plan = PchPlanner.Plan(model, toolchain, new[] { "-std=c++23" });

        Assert.IsNotNull(plan);
        Assert.IsTrue(plan!.UseFlags.Any(f => f.Contains("-include-pch")),
            "Use flags should contain -include-pch");
    }

    [TestMethod]
    public void Plan_ClangPch_AlreadyUpToDate_NoBuildJob()
    {
        var hdrPath = CreateFile("src/pch.h");
        var model   = MakeModel(pchHeader: "src/pch.h");
        var toolchain = MakeToolchain(CompilerKind.Clang);

        // First plan to get the PCH path
        var firstPlan = PchPlanner.Plan(model, toolchain, new[] { "-std=c++23" });
        Assert.IsNotNull(firstPlan);

        // Create the PCH file to simulate it already being built
        File.WriteAllText(firstPlan!.PchFile, "fake pch content");

        // Plan again — checker is null, so just checks file existence
        var secondPlan = PchPlanner.Plan(model, toolchain, new[] { "-std=c++23" });
        Assert.IsNotNull(secondPlan);
        Assert.IsNull(secondPlan!.BuildJob, "No build job when PCH file exists and no checker");
        Assert.IsTrue(secondPlan.IsUpToDate);
    }

    // ─── MSVC PCH ─────────────────────────────────────────────────────────────

    [TestMethod]
    public void Plan_MsvcPch_ProducesYcJob()
    {
        CreateFile("src/pch.h", "#pragma once");
        CreateFile("src/pch.cpp", "#include \"pch.h\"");
        var model    = MakeModel(pchHeader: "src/pch.h", pchSource: "src/pch.cpp");
        var toolchain = MakeToolchain(CompilerKind.Msvc);

        var plan = PchPlanner.Plan(model, toolchain, new[] { "/std:c++latest", "/c" });

        Assert.IsNotNull(plan);
        Assert.IsNotNull(plan!.BuildJob);
        Assert.EndsWith(".pch", plan.PchFile, "PCH file should end with .pch");

        var args = string.Join(" ", plan.BuildJob!.Args);
        Assert.Contains("/Yc", args, "Should contain /Yc");
        Assert.Contains("/Fp", args, "Should contain /Fp");
        Assert.DoesNotContain("/Yu", args, "Should not contain /Yu when creating");
    }

    [TestMethod]
    public void Plan_MsvcPch_UseFlagsContainYu()
    {
        CreateFile("src/pch.h");
        var model    = MakeModel(pchHeader: "src/pch.h");
        var toolchain = MakeToolchain(CompilerKind.Msvc);

        var plan = PchPlanner.Plan(model, toolchain, new[] { "/std:c++latest" });

        Assert.IsNotNull(plan);
        Assert.IsTrue(plan!.UseFlags.Any(f => f.StartsWith("/Yu")),
            "Use flags should contain /Yu");
        Assert.IsTrue(plan.UseFlags.Any(f => f.StartsWith("/Fp")),
            "Use flags should contain /Fp");
    }

    // ─── AddUseFlags ──────────────────────────────────────────────────────────

    [TestMethod]
    public void AddUseFlags_MergesBaseAndPchFlags()
    {
        var plan = new PchPlanner.PchPlan
        {
            PchFile  = "/cache/pch.h.pch",
            UseFlags = new[] { "-include-pch \"/cache/pch.h.pch\"" },
        };

        var merged = PchPlanner.AddUseFlags(new[] { "-std=c++23", "-O2" }, plan);

        Assert.HasCount(3, merged);
        Assert.IsTrue(merged.Contains("-std=c++23"));
        Assert.IsTrue(merged.Contains("-O2"));
        Assert.IsTrue(merged.Any(f => f.Contains("-include-pch")));
    }
}
