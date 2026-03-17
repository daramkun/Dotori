using Dotori.Core.Toolchain;

namespace Dotori.Tests.Graph;

[TestClass]
public sealed class ToolchainDetectorTests
{
    [TestMethod]
    public void DetectAvailable_DoesNotThrow()
    {
        // Smoke test: should not throw regardless of host environment
        var available = ToolchainDetector.DetectAvailable();
        Assert.IsNotNull(available);
    }

    [TestMethod]
    public void Detect_UnknownTarget_ReturnsNull()
    {
        var result = ToolchainDetector.Detect("nonexistent-target-xyz");
        Assert.IsNull(result);
    }

    [TestMethod]
    public void Detect_ValidTargetOnHost_ReturnsInfoOrNull()
    {
        // On macOS we expect to find clang for macos-arm64 or macos-x64
        // On Linux  we expect to find clang for linux-x64 (if installed)
        // On Windows we expect MSVC or clang
        // This just verifies it doesn't throw
        var targets = new[] { "macos-arm64", "macos-x64", "linux-x64", "windows-x64" };
        foreach (var target in targets)
        {
            var result = ToolchainDetector.Detect(target);
            // result may be null if not on the matching OS — that's fine
            if (result is not null)
            {
                Assert.IsFalse(string.IsNullOrEmpty(result.CompilerPath),
                    $"CompilerPath should not be empty for {target}");
                Assert.IsFalse(string.IsNullOrEmpty(result.TargetTriple),
                    $"TargetTriple should not be empty for {target}");
            }
        }
    }

    // ── Phase 1-M: IsClangCl / IsMinGW property tests ───────────────────────

    [TestMethod]
    public void IsClangCl_True_WhenKindMsvcAndCompilerNameClangCl()
    {
        // Use path.Combine so the test works cross-platform (macOS/Linux CI as well)
        var compilerPath = Path.Combine("llvm", "bin", "clang-cl.exe");
        var toolchain = new ToolchainInfo
        {
            Kind         = CompilerKind.Msvc,
            CompilerPath = compilerPath,
            LinkerPath   = Path.Combine("llvm", "bin", "lld-link.exe"),
            TargetTriple = "x86_64-pc-windows-msvc",
        };

        Assert.IsTrue(toolchain.IsClangCl, "Should be clang-cl when Kind==Msvc and compiler name is clang-cl");
    }

    [TestMethod]
    public void IsClangCl_False_WhenKindMsvcAndCompilerNameCl()
    {
        var compilerPath = Path.Combine("msvc", "bin", "cl.exe");
        var toolchain = new ToolchainInfo
        {
            Kind         = CompilerKind.Msvc,
            CompilerPath = compilerPath,
            LinkerPath   = Path.Combine("msvc", "bin", "link.exe"),
            TargetTriple = "x86_64-pc-windows-msvc",
        };

        Assert.IsFalse(toolchain.IsClangCl, "Should not be clang-cl when compiler is cl.exe");
    }

    [TestMethod]
    public void IsClangCl_False_WhenKindClang()
    {
        var toolchain = new ToolchainInfo
        {
            Kind         = CompilerKind.Clang,
            CompilerPath = "/usr/bin/clang++",
            LinkerPath   = "/usr/bin/clang++",
            TargetTriple = "x86_64-unknown-linux-gnu",
        };

        Assert.IsFalse(toolchain.IsClangCl, "Clang (not clang-cl) should not be IsClangCl");
    }

    [TestMethod]
    public void IsMinGW_True_WhenKindClangAndTripleContainsMingw()
    {
        var toolchain = new ToolchainInfo
        {
            Kind         = CompilerKind.Clang,
            CompilerPath = "/usr/bin/clang++",
            LinkerPath   = "/usr/bin/clang++",
            TargetTriple = "x86_64-w64-mingw32",
        };

        Assert.IsTrue(toolchain.IsMinGW, "Should be MinGW when triple contains 'mingw'");
    }

    [TestMethod]
    public void IsMinGW_False_WhenKindMsvc()
    {
        var toolchain = new ToolchainInfo
        {
            Kind         = CompilerKind.Msvc,
            CompilerPath = @"C:\LLVM\clang-cl.exe",
            LinkerPath   = @"C:\LLVM\lld-link.exe",
            TargetTriple = "x86_64-w64-mingw32",  // triple doesn't matter if Kind != Clang
        };

        Assert.IsFalse(toolchain.IsMinGW, "MSVC kind should never be IsMinGW");
    }

    [TestMethod]
    public void IsMinGW_False_WhenKindClangAndTripleNotMingw()
    {
        var toolchain = new ToolchainInfo
        {
            Kind         = CompilerKind.Clang,
            CompilerPath = "/usr/bin/clang++",
            LinkerPath   = "/usr/bin/clang++",
            TargetTriple = "x86_64-unknown-linux-gnu",
        };

        Assert.IsFalse(toolchain.IsMinGW, "Linux Clang should not be IsMinGW");
    }
}
