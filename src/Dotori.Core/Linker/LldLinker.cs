using Dotori.Core.Build;
using Dotori.Core.Model;
using Dotori.Core.Parsing;
using Dotori.Core.Toolchain;

namespace Dotori.Core.Linker;

/// <summary>
/// Produces link jobs for the LLD linker (ld.lld / lld-link),
/// used with Clang on Linux, Android, and WASM targets.
/// For Windows Clang + lld-link, use the MsvcLinker with appropriate flags.
/// </summary>
public static class LldLinker
{
    /// <summary>Build linker flags for the given project model.</summary>
    public static IReadOnlyList<string> LinkFlags(
        FlatProjectModel model,
        ToolchainInfo    toolchain,
        string           outputFile)
    {
        var flags = new List<string>();

        flags.Add($"--target={toolchain.TargetTriple}");

        if (toolchain.SysRoot is not null)
            flags.Add($"--sysroot=\"{toolchain.SysRoot}\"");

        flags.Add($"-o \"{outputFile}\"");

        // Use LLD explicitly
        flags.Add("-fuse-ld=lld");

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

            // musl: fully static binary
            if (toolchain.TargetTriple.Contains("musl"))
                flags.Add("-static");
        }

        // stdlib linkage
        if (model.Stdlib.HasValue)
        {
            flags.Add(model.Stdlib.Value switch
            {
                StdlibType.LibCxx    => "-stdlib=libc++",
                StdlibType.LibStdCxx => "-stdlib=libstdc++",
                _                    => string.Empty,
            });
        }

        // Link libraries
        foreach (var lib in model.Links)
            flags.Add($"-l{lib}");

        // WASM bare linker flags
        if (toolchain.TargetTriple == "wasm32-unknown-unknown")
        {
            flags.Add("-Wl,--no-entry");
            flags.Add("-Wl,--export-all");
        }

        return flags.Where(f => !string.IsNullOrEmpty(f)).ToList();
    }

    /// <summary>Generate a link job using lld (invoked via clang++).</summary>
    public static LinkJob MakeLinkJob(
        IEnumerable<string>   objFiles,
        string                outputFile,
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
