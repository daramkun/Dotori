using Dotori.Core.Model;
using Dotori.Core.Parsing;
using Dotori.Core.Toolchain;

namespace Dotori.Core.Build;

/// <summary>
/// Generates compile/link jobs for the MSVC toolchain (cl.exe + link.exe).
/// </summary>
public static class MsvcDriver
{
    /// <summary>
    /// Build the common compiler flags for a flattened project model.
    /// </summary>
    public static IReadOnlyList<string> CompileFlags(
        FlatProjectModel model,
        ToolchainInfo toolchain,
        string config,
        string objDir)
    {
        var flags = new List<string>();

        // C++ standard
        flags.Add(model.Std switch
        {
            CxxStd.Cxx17 => "/std:c++17",
            CxxStd.Cxx20 => "/std:c++20",
            _             => "/std:c++latest",  // c++23 → latest
        });

        // Optimization
        flags.Add(model.Optimize switch
        {
            OptimizeLevel.None  => "/Od",
            OptimizeLevel.Size  => "/O1",
            OptimizeLevel.Speed => "/O2",
            OptimizeLevel.Full  => "/Ox",
            _                   => "/Od",
        });

        // LTO (whole-program optimization)
        if (model.Lto) flags.Add("/GL");

        // Debug info
        if (model.DebugInfo != DebugInfoLevel.None)
            flags.Add(model.DebugInfo == DebugInfoLevel.Full ? "/Zi" : "/Z7");

        // Runtime link
        bool isDebug = config.Equals("debug", StringComparison.OrdinalIgnoreCase);
        flags.Add(model.RuntimeLink switch
        {
            RuntimeLink.Static  => isDebug ? "/MTd" : "/MT",
            RuntimeLink.Dynamic => isDebug ? "/MDd" : "/MD",
            _                   => isDebug ? "/MTd" : "/MT",
        });

        // Warnings
        flags.Add(model.Warnings switch
        {
            WarningLevel.None    => "/W0",
            WarningLevel.Default => "/W3",
            WarningLevel.All     => "/W4",
            WarningLevel.Extra   => "/W4",
            _                    => "/W3",
        });

        if (model.WarningsAsErrors) flags.Add("/WX");

        // Defines
        foreach (var d in model.Defines)
            flags.Add($"/D{d}");

        // Include directories (headers)
        foreach (var h in model.Headers)
            flags.Add($"/I\"{h.Path}\"");

        // Output dir for .obj
        flags.Add($"/Fo\"{objDir}\\\"");

        // No logo
        flags.Add("/nologo");

        // Compile only (no link)
        flags.Add("/c");

        return flags;
    }

    /// <summary>
    /// Build linker flags for the given project model.
    /// </summary>
    public static IReadOnlyList<string> LinkFlags(
        FlatProjectModel model,
        ToolchainInfo toolchain,
        string config,
        string outputFile)
    {
        var flags = new List<string>();

        flags.Add("/nologo");
        flags.Add($"/OUT:\"{outputFile}\"");

        // LTO needs /LTCG
        if (model.Lto) flags.Add("/LTCG");

        // Debug info: generate .pdb
        if (model.DebugInfo != DebugInfoLevel.None)
        {
            var pdb = Path.ChangeExtension(outputFile, ".pdb");
            flags.Add($"/PDB:\"{pdb}\"");
            flags.Add("/DEBUG");
        }

        // Library type
        if (model.Type == ProjectType.StaticLibrary)
            flags.Add("/LIB");

        // Shared library
        if (model.Type == ProjectType.SharedLibrary)
            flags.Add("/DLL");

        // Link libraries
        foreach (var lib in model.Links)
            flags.Add($"\"{lib}.lib\"");

        // MSVC SDK libs
        if (toolchain.Msvc is { } msvc && !string.IsNullOrEmpty(msvc.WinSdkDir))
        {
            var arch = msvc.Architecture;
            var sdkLib = Path.Combine(msvc.WinSdkDir, "Lib", msvc.WinSdkVer);
            flags.Add($"/LIBPATH:\"{Path.Combine(sdkLib, "um", arch)}\"");
            flags.Add($"/LIBPATH:\"{Path.Combine(sdkLib, "ucrt", arch)}\"");
        }

        return flags;
    }

    /// <summary>
    /// Generate a compile job for a single source file.
    /// </summary>
    public static CompileJob MakeCompileJob(
        string sourceFile,
        string objDir,
        IReadOnlyList<string> commonFlags)
    {
        var objFile = Path.Combine(objDir,
            Path.GetFileNameWithoutExtension(sourceFile) + ".obj");

        var args = new List<string>(commonFlags) { $"\"{sourceFile}\"" };

        return new CompileJob
        {
            SourceFile = sourceFile,
            OutputFile = objFile,
            Args       = args.ToArray(),
        };
    }

    /// <summary>Generate a link job.</summary>
    public static LinkJob MakeLinkJob(
        IEnumerable<string> objFiles,
        string outputFile,
        IReadOnlyList<string> linkFlags)
    {
        var args = new List<string>(linkFlags);
        foreach (var obj in objFiles) args.Add($"\"{obj}\"");

        return new LinkJob
        {
            InputFiles = objFiles.ToArray(),
            OutputFile = outputFile,
            Args       = args.ToArray(),
        };
    }
}
