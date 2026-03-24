using System.Runtime.InteropServices;

namespace Dotori.Core.Toolchain;

public static partial class ToolchainDetector
{
    // ─── Linux ───────────────────────────────────────────────────────────────

    private static ToolchainInfo? DetectLinux(string archPrefix)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return null;
        var clang = FindInPath("clang++") ?? FindBrewLlvm("clang++");
        if (clang is null) return null;

        var lld = FindInPath("ld.lld");

        return new ToolchainInfo
        {
            Kind         = CompilerKind.Clang,
            CompilerPath = clang,
            LinkerPath   = lld ?? clang,
            TargetTriple = archPrefix == "x86_64"
                ? "x86_64-unknown-linux-gnu"
                : "aarch64-unknown-linux-gnu",
            Assembler    = DetectAssemblers(),
        };
    }
}
