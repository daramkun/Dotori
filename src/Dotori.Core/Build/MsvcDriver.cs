using Dotori.Core.Model;
using Dotori.Core.Parsing;
using Dotori.Core.Toolchain;

namespace Dotori.Core.Build;

/// <summary>
/// Generates compile jobs for the MSVC toolchain (cl.exe).
/// Link jobs are handled by <see cref="Dotori.Core.Linker.MsvcLinker"/>.
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

        // Include directories (resolve relative to project root)
        foreach (var h in model.Headers)
        {
            var absPath = Path.IsPathRooted(h.Path)
                ? h.Path
                : Path.GetFullPath(Path.Combine(model.ProjectDir, h.Path));
            flags.Add($"/I\"{absPath}\"");
        }

        // Output dir for .obj
        flags.Add($"/Fo\"{objDir}\\\"");

        // No logo
        flags.Add("/nologo");

        // Compile only (no link)
        flags.Add("/c");

        // User-defined compile flags (appended after dotori-generated flags)
        flags.AddRange(model.CompileFlags);

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
}
