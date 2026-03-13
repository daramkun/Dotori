namespace Dotori.Core.Toolchain;

/// <summary>The type of compiler driver.</summary>
public enum CompilerKind { Msvc, Clang, Emscripten }

/// <summary>Information about a detected toolchain for a specific target.</summary>
public sealed class ToolchainInfo
{
    public required CompilerKind Kind          { get; init; }
    public required string       CompilerPath  { get; init; }
    public required string       TargetTriple  { get; init; }

    /// <summary>Linker executable path (may equal compiler for Clang/emcc).</summary>
    public required string LinkerPath    { get; init; }

    /// <summary>Optional: path to sysroot / SDK root.</summary>
    public string? SysRoot              { get; init; }

    /// <summary>Optional: MSVC-specific include/lib roots.</summary>
    public MsvcPaths? Msvc              { get; init; }

    /// <summary>Optional: Apple-specific SDK path.</summary>
    public string? AppleSdk             { get; init; }
}

/// <summary>MSVC-specific compiler and linker paths.</summary>
public sealed class MsvcPaths
{
    public required string VcToolsDir   { get; init; }  // e.g. VC\Tools\MSVC\14.x.y
    public required string WinSdkDir    { get; init; }  // e.g. C:\Program Files (x86)\Windows Kits\10
    public required string WinSdkVer    { get; init; }  // e.g. 10.0.22621.0
    public required string Architecture { get; init; }  // x64 / x86 / arm64

    /// <summary>Path to rc.exe (Windows SDK Resource Compiler). Null if SDK not found.</summary>
    public string? RcPath { get; init; }

    /// <summary>Path to mt.exe (Windows SDK Manifest Tool). Null if SDK not found.</summary>
    public string? MtPath { get; init; }
}
