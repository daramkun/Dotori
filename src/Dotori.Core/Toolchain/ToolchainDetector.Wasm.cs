using Dotori.Core;

namespace Dotori.Core.Toolchain;

public static partial class ToolchainDetector
{
    // ─── WASM ────────────────────────────────────────────────────────────────

    private static ToolchainInfo? DetectEmscripten()
    {
        // Look for emcc in PATH or EMSDK_PATH
        var emcc = FindInPath("emcc");
        if (emcc is null)
        {
            var emsdkPath = Environment.GetEnvironmentVariable(DotoriConstants.EnvEmsdkPath);
            if (emsdkPath is not null)
                emcc = FindInPath("emcc", Path.Combine(emsdkPath, "upstream", "emscripten"));
        }
        if (emcc is null) return null;

        return new ToolchainInfo
        {
            Kind         = CompilerKind.Emscripten,
            CompilerPath = emcc,
            LinkerPath   = emcc,
            TargetTriple = "wasm32-unknown-emscripten",
        };
    }

    private static ToolchainInfo? DetectWasmBare()
    {
        var clang = FindInPath("clang++") ?? FindBrewLlvm("clang++");
        if (clang is null) return null;

        return new ToolchainInfo
        {
            Kind         = CompilerKind.Clang,
            CompilerPath = clang,
            LinkerPath   = clang,
            TargetTriple = "wasm32-unknown-unknown",
        };
    }
}
