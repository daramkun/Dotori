using System.Runtime.InteropServices;
using Dotori.Core;

namespace Dotori.Core.Toolchain;

public static partial class ToolchainDetector
{
    // ─── Android ─────────────────────────────────────────────────────────────

    private static ToolchainInfo? DetectAndroid(string triple)
    {
        var ndkRoot = FindAndroidNdk();
        if (ndkRoot is null) return null;

        var host      = GetNdkHostPlatform();
        var toolchain = Path.Combine(ndkRoot, "toolchains", "llvm", "prebuilt", host);
        if (!Directory.Exists(toolchain)) return null;

        var clang = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? Path.Combine(toolchain, "bin", "clang++.exe")
            : Path.Combine(toolchain, "bin", "clang++");

        if (!File.Exists(clang)) return null;

        return new ToolchainInfo
        {
            Kind         = CompilerKind.Clang,
            CompilerPath = clang,
            LinkerPath   = clang,
            TargetTriple = triple,
            SysRoot      = Path.Combine(toolchain, "sysroot"),
        };
    }

    private static string? FindAndroidNdk()
    {
        var ndkHome = Environment.GetEnvironmentVariable(DotoriConstants.EnvAndroidNdkHome);
        if (ndkHome is not null && Directory.Exists(ndkHome)) return ndkHome;

        var androidHome = Environment.GetEnvironmentVariable(DotoriConstants.EnvAndroidHome);
        if (androidHome is not null)
        {
            var ndkDir = Path.Combine(androidHome, "ndk");
            if (Directory.Exists(ndkDir))
            {
                // Pick the highest version
                var versions = Directory.GetDirectories(ndkDir)
                    .OrderByDescending(d => d)
                    .FirstOrDefault();
                if (versions is not null) return versions;
            }
        }

        return null;
    }

    private static string GetNdkHostPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "windows-x86_64";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "darwin-x86_64";
        return "linux-x86_64";
    }
}
