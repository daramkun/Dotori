namespace Dotori.Core.Debugger;

/// <summary>Debugger type enumeration</summary>
public enum DebuggerKind
{
    /// <summary>LLDB (LLVM Debugger)</summary>
    Lldb,

    /// <summary>GDB (GNU Debugger)</summary>
    Gdb,

    /// <summary>Visual Studio Debugger (devenv.exe /DebugExe)</summary>
    VsDbg,

    /// <summary>Windows Debugger (windbg.exe)</summary>
    WinDbg,

    /// <summary>Console Debugger (cdb.exe, Windows SDK)</summary>
    Cdb,
}

/// <summary>Information about a detected debugger</summary>
public sealed class DebuggerInfo
{
    /// <summary>Debugger kind</summary>
    public required DebuggerKind Kind { get; init; }

    /// <summary>Full path to debugger executable</summary>
    public required string ExecutablePath { get; init; }

    /// <summary>Human-readable debugger name</summary>
    public string? DisplayName { get; init; }

    /// <summary>Debugger version string (if available)</summary>
    public string? Version { get; init; }
}
