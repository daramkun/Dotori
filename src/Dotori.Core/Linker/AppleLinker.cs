using Dotori.Core.Build;
using Dotori.Core.Model;
using Dotori.Core.Parsing;
using Dotori.Core.Toolchain;

namespace Dotori.Core.Linker;

/// <summary>
/// Produces link jobs for Apple's ld linker (used on macOS / iOS / tvOS / watchOS).
/// Invoked via clang++ with Apple SDK settings.
/// </summary>
public static class AppleLinker
{
    /// <summary>Build linker flags for the given Apple platform project model.</summary>
    public static IReadOnlyList<string> LinkFlags(
        FlatProjectModel model,
        ToolchainInfo    toolchain,
        string           outputFile)
    {
        var flags = new List<string>();

        flags.Add($"--target={toolchain.TargetTriple}");

        // Apple SDK
        if (toolchain.AppleSdk is not null)
            flags.Add($"-isysroot \"{toolchain.AppleSdk}\"");

        flags.Add($"-o \"{outputFile}\"");

        if (model.Lto) flags.Add("-flto");

        // Library type
        if (model.Type == ProjectType.SharedLibrary)
            flags.Add("-dynamiclib");

        // stdlib is always libc++ on Apple platforms
        flags.Add("-stdlib=libc++");

        // Custom framework search paths (-F) — must come before -framework flags
        foreach (var fp in model.FrameworkSearchPaths)
            flags.Add($"-F\"{fp}\"");

        // Apple frameworks (system + resolved from framework-paths / xcframeworks)
        foreach (var fw in model.Frameworks)
        {
            flags.Add("-framework");
            flags.Add(fw);
        }

        // Platform minimum version flags
        if (model.MacosMin is not null && toolchain.TargetTriple.Contains("macos"))
            flags.Add($"-mmacosx-version-min={model.MacosMin}");

        if (model.IosMin is not null && toolchain.TargetTriple.Contains("apple-ios"))
        {
            // Simulator vs device
            if (toolchain.TargetTriple.Contains("simulator"))
                flags.Add($"-mios-simulator-version-min={model.IosMin}");
            else
                flags.Add($"-miphoneos-version-min={model.IosMin}");
        }

        if (model.TvosMin is not null && toolchain.TargetTriple.Contains("tvos"))
            flags.Add($"-mtvos-version-min={model.TvosMin}");

        if (model.WatchosMin is not null && toolchain.TargetTriple.Contains("watchos"))
            flags.Add($"-mwatchos-version-min={model.WatchosMin}");

        // Objective-C runtime — linked automatically when ObjC sources are present
        if (model.HasObjcSources)
            flags.Add("-lobjc");

        // Link libraries
        foreach (var lib in model.Links)
            flags.Add($"-l{lib}");

        // User-defined link flags (appended after dotori-generated flags)
        flags.AddRange(model.LinkFlags);

        return flags;
    }

    /// <summary>Generate a link job using Apple ld (invoked via clang++).</summary>
    public static LinkJob MakeLinkJob(
        IEnumerable<string>   objFiles,
        string                outputFile,
        IReadOnlyList<string> linkFlags) =>
        LinkJobFactory.Create(objFiles, outputFile, linkFlags);

    /// <summary>
    /// Determine if a toolchain targets an Apple platform (macOS / iOS / tvOS / watchOS).
    /// </summary>
    public static bool IsAppleTarget(ToolchainInfo toolchain) =>
        toolchain.TargetTriple.Contains("apple") ||
        toolchain.TargetTriple.Contains("macos") ||
        toolchain.TargetTriple.Contains("ios");
}
