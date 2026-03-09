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
}
