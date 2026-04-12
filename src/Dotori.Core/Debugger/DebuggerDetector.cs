using System.Runtime.InteropServices;

namespace Dotori.Core.Debugger;

/// <summary>
/// Detects available debuggers on the host platform.
/// </summary>
public static class DebuggerDetector
{
    /// <summary>
    /// Find all available debuggers on the current platform.
    /// Returns empty list if none found.
    /// </summary>
    public static IReadOnlyList<DebuggerInfo> DetectAll()
    {
        var debuggers = new List<DebuggerInfo>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (DetectVsDbg() is { } vsdbg) debuggers.Add(vsdbg);
            if (DetectWinDbg() is { } windbg) debuggers.Add(windbg);
            if (DetectCdb() is { } cdb) debuggers.Add(cdb);
            if (DetectLldb() is { } lldb) debuggers.Add(lldb);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (DetectGdb() is { } gdb) debuggers.Add(gdb);
            if (DetectLldb() is { } lldb) debuggers.Add(lldb);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            if (DetectLldb() is { } lldb) debuggers.Add(lldb);
            if (DetectGdb() is { } gdb) debuggers.Add(gdb);
        }

        return debuggers;
    }

    /// <summary>
    /// Find the default/preferred debugger for the current platform.
    /// Windows: VsDbg > WinDbg > CDB > LLDB
    /// Linux: GDB > LLDB
    /// macOS: LLDB > GDB
    /// Returns null if no debugger found.
    /// </summary>
    public static DebuggerInfo? DetectDefault()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return DetectVsDbg() ?? DetectWinDbg() ?? DetectCdb() ?? DetectLldb();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return DetectGdb() ?? DetectLldb();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return DetectLldb() ?? DetectGdb();
        }

        return null;
    }

    /// <summary>
    /// Find a specific debugger by kind.
    /// Returns null if not found.
    /// </summary>
    public static DebuggerInfo? Detect(DebuggerKind kind)
    {
        return kind switch
        {
            DebuggerKind.Lldb   => DetectLldb(),
            DebuggerKind.Gdb    => DetectGdb(),
            DebuggerKind.VsDbg  => DetectVsDbg(),
            DebuggerKind.WinDbg => DetectWinDbg(),
            DebuggerKind.Cdb    => DetectCdb(),
            _                   => null,
        };
    }

    /// <summary>
    /// Validate that target platform matches host platform for debugging.
    /// Returns error message if incompatible, null if compatible.
    /// </summary>
    public static string? ValidateDebugCompatibility(string targetId)
    {
        var hostPlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "windows"
            : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux"
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "macos"
            : "unknown";

        var targetPlatform = targetId.Split('-')[0]; // e.g. "linux-x64" -> "linux"

        // Special case: UWP is Windows
        if (targetPlatform == "uwp")
            targetPlatform = "windows";

        if (targetPlatform != hostPlatform)
        {
            return $"Cannot debug {targetId} target on {hostPlatform} host. " +
                   "Debugging is only supported when host and target platforms match.";
        }

        return null;
    }

    // ─── Platform-Specific Detection ────────────────────────────────────────

    private static DebuggerInfo? DetectLldb()
    {
        var path = FindInPath("lldb");
        if (path is null) return null;

        return new DebuggerInfo
        {
            Kind           = DebuggerKind.Lldb,
            ExecutablePath = path,
            DisplayName    = "LLDB",
        };
    }

    private static DebuggerInfo? DetectGdb()
    {
        var path = FindInPath("gdb");
        if (path is null) return null;

        return new DebuggerInfo
        {
            Kind           = DebuggerKind.Gdb,
            ExecutablePath = path,
            DisplayName    = "GDB",
        };
    }

    private static DebuggerInfo? DetectVsDbg()
    {
        // Try vsdbg.exe (standalone debugger)
        var vsdbg = FindInPath("vsdbg");
        if (vsdbg is not null)
        {
            return new DebuggerInfo
            {
                Kind           = DebuggerKind.VsDbg,
                ExecutablePath = vsdbg,
                DisplayName    = "Visual Studio Debugger",
            };
        }

        // Try devenv.exe (Visual Studio IDE)
        var devenv = FindInPath("devenv");
        if (devenv is not null)
        {
            return new DebuggerInfo
            {
                Kind           = DebuggerKind.VsDbg,
                ExecutablePath = devenv,
                DisplayName    = "Visual Studio",
            };
        }

        // Search Visual Studio installation paths
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var vsBasePath = Path.Combine(programFiles, "Microsoft Visual Studio");

        if (Directory.Exists(vsBasePath))
        {
            // Search for devenv.exe in VS installations (2019, 2022, etc.)
            var pattern = Path.Combine(vsBasePath, "*", "*", "Common7", "IDE", "devenv.exe");
            var matches = Directory.GetFiles(vsBasePath, "devenv.exe", SearchOption.AllDirectories);

            if (matches.Length > 0)
            {
                // Pick the newest version (sorted by path)
                var newest = matches.OrderByDescending(p => p).First();
                return new DebuggerInfo
                {
                    Kind           = DebuggerKind.VsDbg,
                    ExecutablePath = newest,
                    DisplayName    = "Visual Studio",
                };
            }
        }

        return null;
    }

    private static DebuggerInfo? DetectWinDbg()
    {
        var path = FindInPath("windbg");
        if (path is not null)
        {
            return new DebuggerInfo
            {
                Kind           = DebuggerKind.WinDbg,
                ExecutablePath = path,
                DisplayName    = "WinDbg",
            };
        }

        // Search Windows SDK paths
        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        var sdkBasePath = Path.Combine(programFilesX86, "Windows Kits", "10", "Debuggers");

        if (Directory.Exists(sdkBasePath))
        {
            var arch = RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64   => "x64",
                Architecture.X86   => "x86",
                Architecture.Arm64 => "arm64",
                _                  => "x64",
            };

            var windbgPath = Path.Combine(sdkBasePath, arch, "windbg.exe");
            if (File.Exists(windbgPath))
            {
                return new DebuggerInfo
                {
                    Kind           = DebuggerKind.WinDbg,
                    ExecutablePath = windbgPath,
                    DisplayName    = "WinDbg",
                };
            }
        }

        return null;
    }

    private static DebuggerInfo? DetectCdb()
    {
        var path = FindInPath("cdb");
        if (path is not null)
        {
            return new DebuggerInfo
            {
                Kind           = DebuggerKind.Cdb,
                ExecutablePath = path,
                DisplayName    = "CDB (Console Debugger)",
            };
        }

        // Search Windows SDK paths (same as WinDbg)
        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        var sdkBasePath = Path.Combine(programFilesX86, "Windows Kits", "10", "Debuggers");

        if (Directory.Exists(sdkBasePath))
        {
            var arch = RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64   => "x64",
                Architecture.X86   => "x86",
                Architecture.Arm64 => "arm64",
                _                  => "x64",
            };

            var cdbPath = Path.Combine(sdkBasePath, arch, "cdb.exe");
            if (File.Exists(cdbPath))
            {
                return new DebuggerInfo
                {
                    Kind           = DebuggerKind.Cdb,
                    ExecutablePath = cdbPath,
                    DisplayName    = "CDB (Console Debugger)",
                };
            }
        }

        return null;
    }

    // ─── Utilities ──────────────────────────────────────────────────────────

    /// <summary>
    /// Search for an executable in PATH.
    /// On Windows, also tries .exe, .cmd, .bat extensions.
    /// </summary>
    private static string? FindInPath(string name)
    {
        var extensions = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new[] { ".exe", ".cmd", ".bat" }
            : new[] { string.Empty };

        var pathVar = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        var searchPaths = pathVar.Split(Path.PathSeparator);

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
