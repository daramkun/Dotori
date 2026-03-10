using Dotori.Core.Model;
using Dotori.Core.Parsing;
using Dotori.Core.Toolchain;

namespace Dotori.Core.Build;

/// <summary>
/// Generates compile/link jobs for the Clang toolchain (clang++).
/// Handles Linux, macOS, iOS, Android, and WASM bare targets.
/// </summary>
public static class ClangDriver
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

        // Target triple
        flags.Add($"--target={toolchain.TargetTriple}");

        // Sysroot
        if (toolchain.SysRoot is not null)
            flags.Add($"--sysroot=\"{toolchain.SysRoot}\"");

        // Apple SDK
        if (toolchain.AppleSdk is not null)
            flags.Add($"-isysroot \"{toolchain.AppleSdk}\"");

        // C++ standard
        flags.Add(model.Std switch
        {
            CxxStd.Cxx17 => "-std=c++17",
            CxxStd.Cxx20 => "-std=c++20",
            _             => "-std=c++23",
        });

        // C++ stdlib
        if (model.Stdlib.HasValue)
        {
            flags.Add(model.Stdlib.Value switch
            {
                StdlibType.LibCxx    => "-stdlib=libc++",
                StdlibType.LibStdCxx => "-stdlib=libstdc++",
                _                    => string.Empty,
            });
        }

        // Optimization
        flags.Add(model.Optimize switch
        {
            OptimizeLevel.None  => "-O0",
            OptimizeLevel.Size  => "-Os",
            OptimizeLevel.Speed => "-O2",
            OptimizeLevel.Full  => "-O3",
            _                   => "-O0",
        });

        // LTO
        if (model.Lto) flags.Add("-flto");

        // Debug info
        flags.Add(model.DebugInfo switch
        {
            DebugInfoLevel.Full    => "-g",
            DebugInfoLevel.Minimal => "-gline-tables-only",
            _                      => string.Empty,
        });

        // Warnings
        flags.Add(model.Warnings switch
        {
            WarningLevel.None    => "-w",
            WarningLevel.Default => string.Empty,
            WarningLevel.All     => "-Wall",
            WarningLevel.Extra   => "-Wall -Wextra",
            _                    => string.Empty,
        });
        if (model.WarningsAsErrors) flags.Add("-Werror");

        // Defines
        foreach (var d in model.Defines)
            flags.Add($"-D{d}");

        // Include directories (resolve relative to project root)
        foreach (var h in model.Headers)
        {
            var absPath = Path.IsPathRooted(h.Path)
                ? h.Path
                : Path.GetFullPath(Path.Combine(model.ProjectDir, h.Path));
            flags.Add($"-I\"{absPath}\"");
        }

        // macOS minimum version
        if (model.MacosMin is not null &&
            toolchain.TargetTriple.Contains("macosx"))
            flags.Add($"-mmacosx-version-min={model.MacosMin}");

        // iOS minimum version
        if (model.IosMin is not null &&
            toolchain.TargetTriple.Contains("apple-ios"))
            flags.Add($"-miphoneos-version-min={model.IosMin}");

        // Android API level
        if (toolchain.TargetTriple.Contains("android") && model.AndroidApiLevel.HasValue)
        {
            // The API level is embedded in the triple (e.g. aarch64-linux-android26)
            // Just add as a define for consistency
            flags.Add($"-DANDROID_API_LEVEL={model.AndroidApiLevel}");
        }

        // WASM bare additional flags
        if (toolchain.TargetTriple == "wasm32-unknown-unknown")
        {
            flags.Add("--no-standard-libraries");
        }

        // Compile only (no link)
        flags.Add("-c");

        // Remove any empty strings
        return flags.Where(f => !string.IsNullOrEmpty(f)).ToList();
    }

    /// <summary>Build linker flags for the given project model.</summary>
    public static IReadOnlyList<string> LinkFlags(
        FlatProjectModel model,
        ToolchainInfo toolchain,
        string outputFile)
    {
        var flags = new List<string>();

        flags.Add($"--target={toolchain.TargetTriple}");

        if (toolchain.SysRoot is not null)
            flags.Add($"--sysroot=\"{toolchain.SysRoot}\"");

        if (toolchain.AppleSdk is not null)
            flags.Add($"-isysroot \"{toolchain.AppleSdk}\"");

        flags.Add($"-o \"{outputFile}\"");

        if (model.Lto) flags.Add("-flto");

        // Library type
        if (model.Type == ProjectType.SharedLibrary)
            flags.Add("-shared");

        // Runtime static link
        if (model.RuntimeLink == RuntimeLink.Static)
        {
            if (model.Stdlib == StdlibType.LibCxx)
            {
                flags.Add("-static-libstdc++");
                flags.Add("-lc++abi");
            }
            else
            {
                flags.Add("-static-libgcc");
                flags.Add("-static-libstdc++");
            }

            // musl full static
            if (toolchain.TargetTriple.Contains("musl"))
                flags.Add("-static");
        }

        // Link libraries
        foreach (var lib in model.Links)
            flags.Add($"-l{lib}");

        // Apple frameworks
        foreach (var fw in model.Frameworks)
        {
            flags.Add("-framework");
            flags.Add(fw);
        }

        // macOS minimum version
        if (model.MacosMin is not null && toolchain.TargetTriple.Contains("macosx"))
            flags.Add($"-mmacosx-version-min={model.MacosMin}");

        // WASM bare linker flags
        if (toolchain.TargetTriple == "wasm32-unknown-unknown")
        {
            flags.Add("-Wl,--no-entry");
            flags.Add("-Wl,--export-all");
        }

        return flags;
    }

    /// <summary>Generate a compile job for a single source file.</summary>
    public static CompileJob MakeCompileJob(
        string sourceFile,
        string objDir,
        IReadOnlyList<string> commonFlags,
        bool isModule = false)
    {
        var ext     = isModule ? ".pcm" : ".o";
        var objFile = Path.Combine(objDir,
            Path.GetFileNameWithoutExtension(sourceFile) + ext);

        var args = new List<string>(commonFlags) { $"\"{sourceFile}\"" };
        if (isModule)
        {
            // Module precompilation
            args.Remove("-c");
            args.Add("--precompile");
            args.Add("-x c++-module");
        }

        args.Add($"-o \"{objFile}\"");

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
