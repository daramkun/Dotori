using System.Runtime.InteropServices;

namespace Dotori.Core.Toolchain;

/// <summary>
/// Detects available toolchains for supported target platforms.
/// </summary>
public static class ToolchainDetector
{
    // ─── Public API ──────────────────────────────────────────────────────────

    /// <summary>
    /// Detect the best toolchain for the given target ID.
    /// </summary>
    /// <param name="targetId">
    /// One of: windows-x64, windows-x86, windows-arm64,
    ///         uwp-x64, uwp-arm64,
    ///         linux-x64, linux-arm64,
    ///         android-arm64, android-x64, android-arm,
    ///         macos-arm64, macos-x64,
    ///         ios-arm64, ios-sim-arm64, tvos-arm64, watchos-arm64_32,
    ///         wasm32-emscripten, wasm32-bare
    /// </param>
    /// <param name="preferredCompiler">
    /// Optional: "msvc" | "clang" to prefer a compiler kind, or a path/name to
    /// an explicit compiler binary (e.g. "/usr/local/bin/clang++-18", "clang++").
    /// Takes precedence over the CXX / CC environment variables.
    /// </param>
    /// <returns>Detected toolchain info, or null if not found.</returns>
    public static ToolchainInfo? Detect(string targetId, string? preferredCompiler = null)
    {
        // Resolve an explicit compiler path from --compiler <path/name> or CXX/CC env vars.
        // CLI flag takes precedence; env vars are the fallback.
        string? compilerPathOverride = null;
        string? kindPreference       = null;

        if (preferredCompiler is not null)
        {
            if (IsKnownKindName(preferredCompiler))
                kindPreference = preferredCompiler;       // "msvc" | "clang"
            else
                compilerPathOverride = ResolveAsCompilerPath(preferredCompiler); // path or bare name
        }

        // Fall back to CXX / CC environment variables when no CLI override was given.
        compilerPathOverride ??= GetEnvCompilerPath();

        var toolchain = targetId.ToLowerInvariant() switch
        {
            "windows-x64"       => DetectWindows("x64",   kindPreference),
            "windows-x86"       => DetectWindows("x86",   kindPreference),
            "windows-arm64"     => DetectWindows("arm64", kindPreference),
            "uwp-x64"           => DetectUwp("x64"),
            "uwp-arm64"         => DetectUwp("arm64"),
            "linux-x64"         => DetectLinux("x86_64"),
            "linux-arm64"       => DetectLinux("aarch64"),
            "android-arm64"     => DetectAndroid("aarch64-linux-android"),
            "android-x64"       => DetectAndroid("x86_64-linux-android"),
            "android-arm"       => DetectAndroid("armv7a-linux-androideabi"),
            "macos-arm64"       => DetectMacos("arm64-apple-macosx"),
            "macos-x64"         => DetectMacos("x86_64-apple-macosx"),
            "ios-arm64"         => DetectApple("iphoneos",       "arm64-apple-ios"),
            "ios-sim-arm64"     => DetectApple("iphonesimulator","arm64-apple-ios-simulator"),
            "tvos-arm64"        => DetectApple("appletvos",      "arm64-apple-tvos"),
            "watchos-arm64_32"  => DetectApple("watchos",        "arm64_32-apple-watchos"),
            "wasm32-emscripten" => DetectEmscripten(),
            "wasm32-bare"       => DetectWasmBare(),
            _                   => null,
        };

        // Apply explicit compiler path override, keeping all platform-specific
        // metadata (sysroot, SDK paths, Apple SDK, …) from the detected toolchain.
        if (toolchain is not null && compilerPathOverride is not null)
            toolchain = ApplyCompilerOverride(toolchain, compilerPathOverride);

        return toolchain;
    }

    /// <summary>
    /// Return all target IDs for which a toolchain can be detected on this host.
    /// </summary>
    public static IReadOnlyList<string> DetectAvailable()
    {
        var all = new[]
        {
            "windows-x64", "windows-x86", "windows-arm64",
            "uwp-x64", "uwp-arm64",
            "linux-x64", "linux-arm64",
            "android-arm64", "android-x64", "android-arm",
            "macos-arm64", "macos-x64",
            "ios-arm64", "ios-sim-arm64", "tvos-arm64", "watchos-arm64_32",
            "wasm32-emscripten", "wasm32-bare",
        };
        return all.Where(t => Detect(t) is not null).ToList();
    }

    // ─── Windows / UWP ───────────────────────────────────────────────────────

    private static ToolchainInfo? DetectWindows(string arch, string? preferred)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return DetectWindowsCross(arch);

        // Try MSVC first (unless clang preferred)
        if (preferred is not "clang")
        {
            var msvc = TryFindMsvc(arch);
            if (msvc is not null) return msvc;
        }

        // Try Clang
        return TryFindClangWindows(arch);
    }

    /// <summary>
    /// Detects a Windows cross-compilation toolchain on a non-Windows host.
    /// Supports llvm-mingw (prefixed clang++) and clang++ + MinGW sysroot.
    /// </summary>
    private static ToolchainInfo? DetectWindowsCross(string arch)
    {
        var triple = arch switch
        {
            "x64"   => "x86_64-w64-mingw32",
            "arm64" => "aarch64-w64-mingw32",
            _       => null,
        };
        if (triple is null) return null;

        // 1. Try prefixed clang++ (llvm-mingw style: x86_64-w64-mingw32-clang++)
        var prefixedClang = FindInPath($"{triple}-clang++");
        if (prefixedClang is not null)
        {
            // llvm-mingw sysroot is two directories above the bin/ directory
            var sysroot = Path.GetDirectoryName(Path.GetDirectoryName(prefixedClang));
            return new ToolchainInfo
            {
                Kind         = CompilerKind.Clang,
                CompilerPath = prefixedClang,
                LinkerPath   = prefixedClang,
                TargetTriple = triple,
                SysRoot      = sysroot,
            };
        }

        // 2. Try generic clang++ + MinGW sysroot
        var clang = FindInPath("clang++") ?? FindBrewLlvm("clang++");
        if (clang is null) return null;

        var mingwSysroot = FindMinGWSysroot(arch);
        if (mingwSysroot is null) return null;

        return new ToolchainInfo
        {
            Kind         = CompilerKind.Clang,
            CompilerPath = clang,
            LinkerPath   = clang,
            TargetTriple = triple,
            SysRoot      = mingwSysroot,
        };
    }

    /// <summary>
    /// Searches for a MinGW sysroot directory for the given architecture.
    /// Search order: MINGW_SYSROOT env → Homebrew → Linux system path.
    /// </summary>
    private static string? FindMinGWSysroot(string arch)
    {
        var brewArch = arch == "x64" ? "x86_64" : "aarch64";

        // Environment variable override
        var env = Environment.GetEnvironmentVariable("MINGW_SYSROOT");
        if (env is not null && Directory.Exists(env)) return env;

        // Homebrew mingw-w64 (macOS)
        var brewPaths = new[]
        {
            $"/opt/homebrew/opt/mingw-w64/toolchain-{brewArch}",  // Apple Silicon
            $"/usr/local/opt/mingw-w64/toolchain-{brewArch}",     // Intel Mac
        };
        foreach (var p in brewPaths)
            if (Directory.Exists(p)) return p;

        // Linux system MinGW-w64
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var linuxPath = $"/usr/{brewArch}-w64-mingw32";
            if (Directory.Exists(linuxPath)) return linuxPath;
        }

        return null;
    }

    private static ToolchainInfo? DetectUwp(string arch)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return null;
        // UWP requires MSVC
        return TryFindMsvc(arch, uwp: true);
    }

    private static ToolchainInfo? TryFindMsvc(string arch, bool uwp = false)
    {
        // 1. Try vswhere.exe
        var vswhereResult = RunVswhere();
        if (vswhereResult is null) return null;

        var vcToolsDir = FindVcToolsDir(vswhereResult, arch);
        if (vcToolsDir is null) return null;

        var clExe = Path.Combine(vcToolsDir, "bin", $"Host{HostArch}", arch, "cl.exe");
        if (!File.Exists(clExe)) return null;

        var linkExe = Path.Combine(vcToolsDir, "bin", $"Host{HostArch}", arch, "link.exe");

        return new ToolchainInfo
        {
            Kind         = CompilerKind.Msvc,
            CompilerPath = clExe,
            LinkerPath   = linkExe,
            TargetTriple = arch switch
            {
                "x64"   => "x86_64-pc-windows-msvc",
                "x86"   => "i686-pc-windows-msvc",
                "arm64" => "aarch64-pc-windows-msvc",
                _       => arch,
            },
            Msvc = TryGetMsvcPaths(vcToolsDir, arch),
        };
    }

    /// <summary>
    /// Collects MSVC + Windows SDK paths for a given VcToolsDir and architecture.
    /// Used by both TryFindMsvc (cl.exe) and TryFindClangWindows (clang-cl).
    /// </summary>
    private static MsvcPaths TryGetMsvcPaths(string vcToolsDir, string arch)
    {
        var (winSdkDir, winSdkVer) = FindWindowsKits();
        return winSdkDir is not null
            ? new MsvcPaths
            {
                VcToolsDir   = vcToolsDir,
                WinSdkDir    = winSdkDir,
                WinSdkVer    = winSdkVer!,
                Architecture = arch,
                RcPath       = FindSdkTool(winSdkDir, winSdkVer!, "rc.exe"),
                MtPath       = FindSdkTool(winSdkDir, winSdkVer!, "mt.exe"),
            }
            : new MsvcPaths
            {
                VcToolsDir   = vcToolsDir,
                WinSdkDir    = string.Empty,
                WinSdkVer    = string.Empty,
                Architecture = arch,
            };
    }

    private static ToolchainInfo? TryFindClangWindows(string arch)
    {
        var clangCl = FindInPath("clang-cl");
        var clangPp = FindInPath("clang++");
        var clang   = clangCl ?? clangPp;
        if (clang is null) return null;

        var lld = FindInPath("lld-link") ?? FindInPath("ld.lld");

        var triple = arch switch
        {
            "x64"   => "x86_64-pc-windows-msvc",
            "x86"   => "i686-pc-windows-msvc",
            "arm64" => "aarch64-pc-windows-msvc",
            _       => arch,
        };

        // clang-cl + MSVC SDK → use Msvc driver (Kind=Msvc) with clang-cl as the compiler.
        // MsvcDriver flags (/std:c++latest, /O2, /MT …) are accepted by clang-cl.
        // lld-link accepts MSVC linker flags, so MsvcLinker works unchanged.
        if (clangCl is not null)
        {
            var vswhereResult = RunVswhere();
            if (vswhereResult is not null)
            {
                var vcToolsDir = FindVcToolsDir(vswhereResult, arch);
                if (vcToolsDir is not null)
                {
                    return new ToolchainInfo
                    {
                        Kind         = CompilerKind.Msvc,
                        CompilerPath = clangCl,
                        LinkerPath   = lld ?? clangCl,
                        TargetTriple = triple,
                        Msvc         = TryGetMsvcPaths(vcToolsDir, arch),
                    };
                }
            }
        }

        // No MSVC SDK available — fall back to plain Clang (no SDK headers/libs).
        return new ToolchainInfo
        {
            Kind         = CompilerKind.Clang,
            CompilerPath = clang,
            LinkerPath   = lld ?? clang,
            TargetTriple = triple,
        };
    }

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
        };
    }

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
        var ndkHome = Environment.GetEnvironmentVariable("ANDROID_NDK_HOME");
        if (ndkHome is not null && Directory.Exists(ndkHome)) return ndkHome;

        var androidHome = Environment.GetEnvironmentVariable("ANDROID_HOME");
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
        };
    }

    // ─── WASM ────────────────────────────────────────────────────────────────

    private static ToolchainInfo? DetectEmscripten()
    {
        // Look for emcc in PATH or EMSDK_PATH
        var emcc = FindInPath("emcc");
        if (emcc is null)
        {
            var emsdkPath = Environment.GetEnvironmentVariable("EMSDK_PATH");
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

    // ─── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>Returns true for the two reserved kind-preference names.</summary>
    internal static bool IsKnownKindName(string value) =>
        value.Equals("msvc", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("clang", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Reads the CXX environment variable (then CC as fallback) and returns the
    /// compiler path if the file exists, or null if neither variable points to a
    /// valid executable.
    /// </summary>
    internal static string? GetEnvCompilerPath()
    {
        var cxx = Environment.GetEnvironmentVariable("CXX");
        if (cxx is not null)
        {
            var resolved = ResolveAsCompilerPath(cxx);
            if (resolved is not null) return resolved;
        }

        var cc = Environment.GetEnvironmentVariable("CC");
        if (cc is not null)
        {
            var resolved = ResolveAsCompilerPath(cc);
            if (resolved is not null) return resolved;
        }

        return null;
    }

    /// <summary>
    /// Resolves a compiler value to an absolute path:
    /// <list type="bullet">
    ///   <item>Absolute path → checked directly.</item>
    ///   <item>Relative path (contains separator or starts with ./) → resolved via CWD.</item>
    ///   <item>Bare name (e.g. "clang++") → searched in PATH.</item>
    /// </list>
    /// Returns null when the executable cannot be found.
    /// </summary>
    internal static string? ResolveAsCompilerPath(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        if (Path.IsPathRooted(value))
            return File.Exists(value) ? value : null;

        if (value.Contains(Path.DirectorySeparatorChar) ||
            value.Contains(Path.AltDirectorySeparatorChar))
        {
            var full = Path.GetFullPath(value);
            return File.Exists(full) ? full : null;
        }

        // Bare name — search PATH (and add .exe on Windows).
        return FindInPath(value);
    }

    /// <summary>
    /// Infers <see cref="CompilerKind"/> from the compiler binary file name.
    /// Handles both Unix ('/') and Windows ('\') path separators regardless of host OS.
    /// </summary>
    internal static CompilerKind GuessKindFromPath(string compilerPath)
    {
        // Split on both separators so Windows paths work correctly on Unix hosts.
        var parts    = compilerPath.Split('/', '\\');
        var fileName = parts.LastOrDefault(p => p.Length > 0) ?? compilerPath;

        var dotIdx = fileName.LastIndexOf('.');
        var name   = (dotIdx >= 0 ? fileName[..dotIdx] : fileName)
                         .ToLowerInvariant();

        if (name is "cl" || name.StartsWith("clang-cl"))
            return CompilerKind.Msvc;

        if (name.StartsWith("emcc") || name.StartsWith("em++"))
            return CompilerKind.Emscripten;

        return CompilerKind.Clang;
    }

    /// <summary>
    /// Returns a new <see cref="ToolchainInfo"/> identical to <paramref name="original"/>
    /// except that <see cref="ToolchainInfo.CompilerPath"/> and
    /// <see cref="ToolchainInfo.Kind"/> are replaced by values derived from
    /// <paramref name="compilerPath"/>.
    /// All platform-specific metadata (sysroot, SDK paths, Apple SDK, MSVC paths)
    /// are preserved so that the rest of the build pipeline still works.
    /// </summary>
    internal static ToolchainInfo ApplyCompilerOverride(
        ToolchainInfo original, string compilerPath)
    {
        return new ToolchainInfo
        {
            Kind         = GuessKindFromPath(compilerPath),
            CompilerPath = compilerPath,
            LinkerPath   = original.LinkerPath,
            TargetTriple = original.TargetTriple,
            SysRoot      = original.SysRoot,
            Msvc         = original.Msvc,
            AppleSdk     = original.AppleSdk,
        };
    }

    private static string HostArch =>
        RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64  => "x64",
            Architecture.X86  => "x86",
            Architecture.Arm64 => "arm64",
            _                 => "x64",
        };

    /// <summary>
    /// Look up a Windows SDK tool (e.g. rc.exe, mt.exe) in the host-arch bin directory.
    /// Returns null if the file does not exist.
    /// </summary>
    private static string? FindSdkTool(string winSdkDir, string winSdkVer, string toolName)
    {
        var path = Path.Combine(winSdkDir, "bin", winSdkVer, HostArch, toolName);
        return File.Exists(path) ? path : null;
    }

    private static string? RunVswhere()
    {
        // Common vswhere.exe locations
        var candidates = new[]
        {
            Environment.GetEnvironmentVariable("PROGRAMFILES(X86)") is { } pf86
                ? Path.Combine(pf86, "Microsoft Visual Studio", "Installer", "vswhere.exe")
                : null,
            Environment.GetEnvironmentVariable("PROGRAMFILES") is { } pf
                ? Path.Combine(pf, "Microsoft Visual Studio", "Installer", "vswhere.exe")
                : null,
        };

        var vswhere = candidates.FirstOrDefault(c => c is not null && File.Exists(c))
                   ?? FindInPath("vswhere");

        if (vswhere is null) return null;

        return RunProcess(vswhere, "-latest -products * -requires Microsoft.VisualCpp.Tools.HostX64.TargetX64 -property installationPath");
    }

    private static string? FindVcToolsDir(string vsInstallPath, string arch)
    {
        var vcDir = Path.Combine(vsInstallPath.Trim(), "VC", "Tools", "MSVC");
        if (!Directory.Exists(vcDir)) return null;

        var latest = Directory.GetDirectories(vcDir)
            .OrderByDescending(d => d)
            .FirstOrDefault();

        return latest;
    }

    private static (string? dir, string? ver) FindWindowsKits()
    {
        // Check registry or common paths
        var kitsRoots = new[]
        {
            @"C:\Program Files (x86)\Windows Kits\10",
            @"C:\Program Files\Windows Kits\10",
        };

        foreach (var root in kitsRoots)
        {
            if (!Directory.Exists(root)) continue;
            var includeDir = Path.Combine(root, "Include");
            if (!Directory.Exists(includeDir)) continue;

            var ver = Directory.GetDirectories(includeDir)
                .Select(Path.GetFileName)
                .Where(v => v is not null && v.StartsWith("10."))
                .OrderByDescending(v => v)
                .FirstOrDefault();

            if (ver is not null) return (root, ver);
        }

        return (null, null);
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

    private static string? RunProcess(string exe, string args)
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo(exe, args)
            {
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                CreateNoWindow         = true,
            };
            using var proc = System.Diagnostics.Process.Start(psi);
            if (proc is null) return null;
            var output = proc.StandardOutput.ReadToEnd().Trim();
            proc.WaitForExit();
            return proc.ExitCode == 0 ? output : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Look for a tool in common Homebrew LLVM installation paths (macOS only).
    /// Checks Apple Silicon path first, then Intel Mac path.
    /// </summary>
    private static string? FindBrewLlvm(string toolName)
    {
        var brewPrefixes = new[]
        {
            "/opt/homebrew/opt/llvm/bin", // Apple Silicon
            "/usr/local/opt/llvm/bin",    // Intel Mac
        };

        foreach (var dir in brewPrefixes)
        {
            var candidate = Path.Combine(dir, toolName);
            if (File.Exists(candidate)) return candidate;
        }

        return null;
    }

    private static string? FindInPath(string name, string? extraDir = null)
    {
        var extensions = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new[] { ".exe", ".cmd", ".bat" }
            : new[] { string.Empty };

        var searchPaths = new List<string>();
        if (extraDir is not null) searchPaths.Add(extraDir);

        var pathVar = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        searchPaths.AddRange(pathVar.Split(Path.PathSeparator));

        foreach (var dir in searchPaths)
        {
            foreach (var ext in extensions)
            {
                var candidate = Path.Combine(dir, name + ext);
                if (File.Exists(candidate)) return candidate;
            }
        }

        return null;
    }
}
