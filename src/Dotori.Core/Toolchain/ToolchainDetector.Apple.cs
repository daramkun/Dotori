using System.Runtime.InteropServices;

namespace Dotori.Core.Toolchain;

public static partial class ToolchainDetector
{
    // ─── macOS ───────────────────────────────────────────────────────────────

    private static ToolchainInfo? DetectMacos(string triple)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return null;

        var sdkPath = RunXcrun("--sdk macosx --show-sdk-path");

        // Prefer PATH clang++ (e.g. LLVM from brew/mise) over Apple Clang for better C++23 Modules support
        var pathClang = FindInPath("clang++") ?? FindBrewLlvm("clang++");
        if (pathClang is not null)
        {
            return new ToolchainInfo
            {
                Kind         = CompilerKind.Clang,
                CompilerPath = pathClang,
                LinkerPath   = pathClang,
                TargetTriple = triple,
                AppleSdk     = sdkPath,
                Assembler    = DetectAssemblers(),
            };
        }

        return DetectApple("macosx", triple);
    }

    // ─── Apple (iOS / tvOS / watchOS / macOS) ────────────────────────────────

    private static ToolchainInfo? DetectApple(string sdk, string triple)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return null;

        var clang  = RunXcrun($"--sdk {sdk} --find clang++");
        if (clang is null) return null;

        var sdkPath = RunXcrun($"--sdk {sdk} --show-sdk-path");

        return new ToolchainInfo
        {
            Kind         = CompilerKind.Clang,
            CompilerPath = clang,
            LinkerPath   = clang,
            TargetTriple = triple,
            AppleSdk     = sdkPath,
            Assembler    = DetectAssemblers(),
        };
    }

    private static string? RunXcrun(string args)
    {
        try
        {
            return RunProcess("xcrun", args)?.Trim();
        }
        catch
        {
            return null;
        }
    }
}
