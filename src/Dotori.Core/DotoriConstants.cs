namespace Dotori.Core;

/// <summary>
/// Project-wide constants for directory names, file names, and default values.
/// Centralises magic strings and numbers that were previously scattered across the codebase.
/// </summary>
public static class DotoriConstants
{
    // ─── File / directory names ────────────────────────────────────────────────

    /// <summary>Extension used by all .dotori project files.</summary>
    public const string DotoriExtension = ".dotori";

    /// <summary>Lock file name written next to the .dotori file.</summary>
    public const string LockFileName = ".dotori.lock";

    /// <summary>Root cache directory created inside every project directory.</summary>
    public const string CacheDir = ".dotori-cache";

    /// <summary>Sub-directory under <see cref="CacheDir"/> for compiled object files.</summary>
    public const string ObjSubDir = "obj";

    /// <summary>Sub-directory under <see cref="CacheDir"/> for linked binaries (before artifact copy).</summary>
    public const string BinSubDir = "bin";

    /// <summary>Sub-directory under <see cref="CacheDir"/> for BMI (.pcm / .ifc) files.</summary>
    public const string BmiSubDir = "bmi";

    /// <summary>Sub-directory under <see cref="CacheDir"/> for unity-build amalgamation files.</summary>
    public const string UnitySubDir = "unity";

    /// <summary>Sub-directory under <see cref="CacheDir"/> for pre-compiled headers.</summary>
    public const string PchSubDir = "pch";

    /// <summary>Incremental build hash database file name inside <see cref="CacheDir"/>.</summary>
    public const string HashDbFileName = "hashes.db";

    /// <summary>Module export-map file name written inside the BMI directory.</summary>
    public const string ModuleMapFileName = "module-map.json";

    // ─── Default numeric values ────────────────────────────────────────────────

    /// <summary>Default number of source files per unity-build batch.</summary>
    public const int DefaultUnityBatchSize = 8;

    /// <summary>mt.exe resource ID for executable binaries.</summary>
    public const int ManifestResourceIdExe = 1;

    /// <summary>mt.exe resource ID for DLL binaries.</summary>
    public const int ManifestResourceIdDll = 2;

    /// <summary>Format version written into module-map.json.</summary>
    public const int ModuleMapFormatVersion = 1;

    /// <summary>Format version written into .dotori.lock.</summary>
    public const int LockFileVersion = 1;
}
