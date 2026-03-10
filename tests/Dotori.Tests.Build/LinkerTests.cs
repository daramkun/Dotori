using Dotori.Core.Linker;
using Dotori.Core.Model;
using Dotori.Core.Parsing;
using Dotori.Core.Toolchain;

namespace Dotori.Tests.Build;

[TestClass]
public sealed class LldLinkerTests
{
    private static FlatProjectModel MakeModel(
        string name = "MyApp",
        ProjectType type = ProjectType.Executable,
        RuntimeLink runtimeLink = RuntimeLink.Static,
        StdlibType? stdlib = null,
        bool lto = false,
        string? targetTriple = null)
    {
        return new FlatProjectModel
        {
            Name       = name,
            ProjectDir = "/tmp/project",
            DotoriPath = "/tmp/project/.dotori",
            Type       = type,
            RuntimeLink = runtimeLink,
            Stdlib     = stdlib,
            Lto        = lto,
        };
    }

    private static ToolchainInfo MakeToolchain(
        string targetTriple = "x86_64-unknown-linux-gnu",
        string? sysRoot = null)
    {
        return new ToolchainInfo
        {
            Kind         = CompilerKind.Clang,
            CompilerPath = "/usr/bin/clang++",
            LinkerPath   = "/usr/bin/clang++",
            TargetTriple = targetTriple,
            SysRoot      = sysRoot,
        };
    }

    [TestMethod]
    public void LinkFlags_BasicExecutable_ContainsTargetAndOutput()
    {
        var model    = MakeModel();
        var toolchain = MakeToolchain("x86_64-unknown-linux-gnu");
        var flags    = LldLinker.LinkFlags(model, toolchain, "/out/app");

        var flagStr = string.Join(" ", flags);
        Assert.IsTrue(flagStr.Contains("--target=x86_64-unknown-linux-gnu"), "Should contain target triple");
        Assert.IsTrue(flagStr.Contains("-o \"/out/app\""), "Should contain output file");
        Assert.IsTrue(flagStr.Contains("-fuse-ld=lld"), "Should use lld");
    }

    [TestMethod]
    public void LinkFlags_SharedLibrary_ContainsSharedFlag()
    {
        var model    = MakeModel(type: ProjectType.SharedLibrary);
        var toolchain = MakeToolchain();
        var flags    = LldLinker.LinkFlags(model, toolchain, "/out/libmyapp.so");

        Assert.IsTrue(flags.Contains("-shared"), "Should contain -shared");
    }

    [TestMethod]
    public void LinkFlags_WithLto_ContainsFlto()
    {
        var model    = MakeModel(lto: true);
        var toolchain = MakeToolchain();
        var flags    = LldLinker.LinkFlags(model, toolchain, "/out/app");

        Assert.IsTrue(flags.Contains("-flto"), "Should contain -flto");
    }

    [TestMethod]
    public void LinkFlags_StaticLibStdCxx_ContainsStaticFlags()
    {
        var model    = MakeModel(runtimeLink: RuntimeLink.Static, stdlib: StdlibType.LibStdCxx);
        var toolchain = MakeToolchain();
        var flags    = LldLinker.LinkFlags(model, toolchain, "/out/app");

        Assert.IsTrue(flags.Contains("-static-libgcc"),   "Should contain -static-libgcc");
        Assert.IsTrue(flags.Contains("-static-libstdc++"), "Should contain -static-libstdc++");
    }

    [TestMethod]
    public void LinkFlags_StaticLibCxx_ContainsLibCxxAbi()
    {
        var model    = MakeModel(runtimeLink: RuntimeLink.Static, stdlib: StdlibType.LibCxx);
        var toolchain = MakeToolchain();
        var flags    = LldLinker.LinkFlags(model, toolchain, "/out/app");

        Assert.IsTrue(flags.Contains("-static-libstdc++"), "Should contain -static-libstdc++");
        Assert.IsTrue(flags.Contains("-lc++abi"), "Should contain -lc++abi");
    }

    [TestMethod]
    public void LinkFlags_MuslTarget_ContainsStaticFlag()
    {
        var model    = MakeModel(runtimeLink: RuntimeLink.Static);
        var toolchain = MakeToolchain("x86_64-unknown-linux-musl");
        var flags    = LldLinker.LinkFlags(model, toolchain, "/out/app");

        Assert.IsTrue(flags.Contains("-static"), "musl + static should add -static");
    }

    [TestMethod]
    public void LinkFlags_WasmBare_ContainsExportFlags()
    {
        var model    = MakeModel();
        var toolchain = MakeToolchain("wasm32-unknown-unknown");
        var flags    = LldLinker.LinkFlags(model, toolchain, "/out/app.wasm");

        Assert.IsTrue(flags.Contains("-Wl,--no-entry"),   "Should contain --no-entry");
        Assert.IsTrue(flags.Contains("-Wl,--export-all"), "Should contain --export-all");
    }

    [TestMethod]
    public void LinkFlags_WithSysroot_ContainsSysrootFlag()
    {
        var model    = MakeModel();
        var toolchain = MakeToolchain(sysRoot: "/sysroot");
        var flags    = LldLinker.LinkFlags(model, toolchain, "/out/app");

        Assert.IsTrue(flags.Any(f => f.Contains("--sysroot=")), "Should contain sysroot");
    }

    [TestMethod]
    public void MakeLinkJob_ProducesCorrectJob()
    {
        var model    = MakeModel();
        var toolchain = MakeToolchain();
        var flags    = LldLinker.LinkFlags(model, toolchain, "/out/app");
        var job      = LldLinker.MakeLinkJob(new[] { "a.o", "b.o" }, "/out/app", flags);

        Assert.AreEqual("/out/app", job.OutputFile);
        Assert.AreEqual(2, job.InputFiles.Length);
        Assert.IsTrue(job.Args.Any(a => a.Contains("a.o")));
    }
}

[TestClass]
public sealed class AppleLinkerTests
{
    private static FlatProjectModel MakeModel(
        ProjectType type = ProjectType.Executable,
        string? macosMin = null,
        string? iosMin = null,
        string? tvosMin = null,
        string? watchosMin = null,
        bool lto = false)
    {
        var model = new FlatProjectModel
        {
            Name       = "MyApp",
            ProjectDir = "/tmp/project",
            DotoriPath = "/tmp/project/.dotori",
            Type       = type,
            Lto        = lto,
            MacosMin   = macosMin,
            IosMin     = iosMin,
            TvosMin    = tvosMin,
            WatchosMin = watchosMin,
        };
        return model;
    }

    private static ToolchainInfo MakeToolchain(
        string targetTriple = "arm64-apple-macosx14.0",
        string? appleSdk = null)
    {
        return new ToolchainInfo
        {
            Kind         = CompilerKind.Clang,
            CompilerPath = "/usr/bin/clang++",
            LinkerPath   = "/usr/bin/clang++",
            TargetTriple = targetTriple,
            AppleSdk     = appleSdk,
        };
    }

    [TestMethod]
    public void IsAppleTarget_MacosTriple_ReturnsTrue()
    {
        var tc = MakeToolchain("arm64-apple-macosx14.0");
        Assert.IsTrue(AppleLinker.IsAppleTarget(tc));
    }

    [TestMethod]
    public void IsAppleTarget_IosTriple_ReturnsTrue()
    {
        var tc = MakeToolchain("arm64-apple-ios16.0");
        Assert.IsTrue(AppleLinker.IsAppleTarget(tc));
    }

    [TestMethod]
    public void IsAppleTarget_LinuxTriple_ReturnsFalse()
    {
        var tc = MakeToolchain("x86_64-unknown-linux-gnu");
        Assert.IsFalse(AppleLinker.IsAppleTarget(tc));
    }

    [TestMethod]
    public void LinkFlags_BasicMacos_ContainsEssentialFlags()
    {
        var model    = MakeModel();
        var toolchain = MakeToolchain("arm64-apple-macosx14.0");
        var flags    = AppleLinker.LinkFlags(model, toolchain, "/out/MyApp");

        var flagStr = string.Join(" ", flags);
        Assert.IsTrue(flagStr.Contains("--target=arm64-apple-macosx14.0"), "Should contain target triple");
        Assert.IsTrue(flagStr.Contains("-o \"/out/MyApp\""), "Should contain output");
        Assert.IsTrue(flagStr.Contains("-stdlib=libc++"), "Apple always uses libc++");
    }

    [TestMethod]
    public void LinkFlags_SharedLibrary_ContainsDynamiclib()
    {
        var model    = MakeModel(type: ProjectType.SharedLibrary);
        var toolchain = MakeToolchain("arm64-apple-macosx14.0");
        var flags    = AppleLinker.LinkFlags(model, toolchain, "/out/libMyApp.dylib");

        Assert.IsTrue(flags.Contains("-dynamiclib"), "Should contain -dynamiclib for dylib");
    }

    [TestMethod]
    public void LinkFlags_WithAppleSdk_ContainsIsysroot()
    {
        var model    = MakeModel();
        var toolchain = MakeToolchain(appleSdk: "/Applications/Xcode.app/Contents/Developer/Platforms/MacOSX.platform/Developer/SDKs/MacOSX.sdk");
        var flags    = AppleLinker.LinkFlags(model, toolchain, "/out/app");

        Assert.IsTrue(flags.Any(f => f.Contains("-isysroot")), "Should contain -isysroot");
    }

    [TestMethod]
    public void LinkFlags_MacosMin_ContainsMinVersionFlag()
    {
        var model    = MakeModel(macosMin: "12.0");
        var toolchain = MakeToolchain("arm64-apple-macosx14.0");
        var flags    = AppleLinker.LinkFlags(model, toolchain, "/out/app");

        Assert.IsTrue(flags.Any(f => f.Contains("-mmacosx-version-min=12.0")),
            "Should contain macOS min version flag");
    }

    [TestMethod]
    public void LinkFlags_IosMin_ContainsIphoneosMinVersion()
    {
        var model    = MakeModel(iosMin: "15.0");
        var toolchain = MakeToolchain("arm64-apple-ios15.0");
        var flags    = AppleLinker.LinkFlags(model, toolchain, "/out/app");

        Assert.IsTrue(flags.Any(f => f.Contains("-miphoneos-version-min=15.0")),
            "Should contain iOS min version flag");
    }

    [TestMethod]
    public void LinkFlags_IosSimulator_ContainsSimulatorMinVersion()
    {
        var model    = MakeModel(iosMin: "15.0");
        var toolchain = MakeToolchain("arm64-apple-ios15.0-simulator");
        var flags    = AppleLinker.LinkFlags(model, toolchain, "/out/app");

        Assert.IsTrue(flags.Any(f => f.Contains("-mios-simulator-version-min=15.0")),
            "Should contain iOS simulator min version flag");
    }

    [TestMethod]
    public void LinkFlags_WithLto_ContainsFlto()
    {
        var model    = MakeModel(lto: true);
        var toolchain = MakeToolchain();
        var flags    = AppleLinker.LinkFlags(model, toolchain, "/out/app");

        Assert.IsTrue(flags.Contains("-flto"), "Should contain -flto");
    }

    [TestMethod]
    public void MakeLinkJob_ProducesCorrectJob()
    {
        var model    = MakeModel();
        var toolchain = MakeToolchain();
        var flags    = AppleLinker.LinkFlags(model, toolchain, "/out/app");
        var job      = AppleLinker.MakeLinkJob(new[] { "a.o", "b.o" }, "/out/app", flags);

        Assert.AreEqual("/out/app", job.OutputFile);
        Assert.AreEqual(2, job.InputFiles.Length);
    }
}
