using System.Xml.Linq;

namespace Dotori.Core.Build;

/// <summary>
/// Resolves an .xcframework bundle to the correct inner .framework slice
/// for a given build target.
///
/// An .xcframework contains multiple slices, each targeting a different
/// platform/architecture combination. This class reads Info.plist and
/// selects the best matching slice.
/// </summary>
public static class XcframeworkResolver
{
    /// <summary>Result of a successful xcframework resolution.</summary>
    public sealed class ResolvedSlice
    {
        /// <summary>
        /// The directory that contains the inner .framework bundle.
        /// Pass this as <c>-F &lt;SliceDir&gt;</c> to the compiler and linker.
        /// </summary>
        public required string SliceDir      { get; init; }

        /// <summary>
        /// The framework name without extension (e.g. "MySDK" from MySDK.framework).
        /// Pass this as <c>-framework &lt;FrameworkName&gt;</c> to the linker.
        /// </summary>
        public required string FrameworkName { get; init; }
    }

    /// <summary>
    /// Resolve an .xcframework to the slice matching <paramref name="targetId"/>.
    /// Returns null when no slice matches or Info.plist cannot be read.
    /// </summary>
    /// <param name="xcframeworkPath">Absolute path to the .xcframework directory.</param>
    /// <param name="targetId">Dotori target ID, e.g. "macos-arm64", "ios-sim-arm64".</param>
    public static ResolvedSlice? Resolve(string xcframeworkPath, string targetId)
    {
        var infoPlist = Path.Combine(xcframeworkPath, "Info.plist");
        if (!File.Exists(infoPlist)) return null;

        var frameworkName = Path.GetFileNameWithoutExtension(xcframeworkPath);
        var slices = ParseInfoPlist(infoPlist);

        var (platform, xcfArch, isSimulator) = ParseTargetId(targetId);

        foreach (var slice in slices)
        {
            // Match platform (macos, ios, tvos, watchos)
            if (!string.Equals(slice.Platform, platform, StringComparison.OrdinalIgnoreCase))
                continue;

            // Match simulator variant
            var sliceIsSimulator = string.Equals(
                slice.PlatformVariant, "simulator", StringComparison.OrdinalIgnoreCase);
            if (isSimulator != sliceIsSimulator)
                continue;

            // Match architecture
            if (!slice.Architectures.Any(a =>
                    string.Equals(a, xcfArch, StringComparison.OrdinalIgnoreCase)))
                continue;

            // Found a match — the slice dir is xcframeworkPath/LibraryIdentifier
            var sliceDir = Path.Combine(xcframeworkPath, slice.LibraryIdentifier);
            return new ResolvedSlice
            {
                SliceDir      = sliceDir,
                FrameworkName = frameworkName,
            };
        }

        return null;
    }

    // ─── Target ID parsing ───────────────────────────────────────────────────

    /// <summary>
    /// Parse a dotori targetId into (platform, xcfArch, isSimulator).
    /// xcfArch uses xcframework naming: x86_64 instead of x64.
    /// </summary>
    private static (string platform, string xcfArch, bool isSimulator) ParseTargetId(string targetId)
    {
        var lower = targetId.ToLowerInvariant();
        var parts = lower.Split('-');

        // Platform: first segment
        var platform = parts[0] switch
        {
            "macos"    => "macos",
            "ios"      => "ios",
            "tvos"     => "tvos",
            "watchos"  => "watchos",
            _          => parts[0],
        };

        // Simulator: target ID contains "sim"
        var isSimulator = lower.Contains("sim");

        // Architecture: scan parts for known identifiers
        var arch = "arm64"; // default for Apple targets
        foreach (var p in parts)
        {
            if (p is "arm64" or "arm64_32")        { arch = p;      break; }
            if (p is "x64"   or "x86_64")          { arch = "x86_64"; break; }
            if (p is "x86")                         { arch = "i386";   break; }
        }

        return (platform, arch, isSimulator);
    }

    // ─── Info.plist parsing ──────────────────────────────────────────────────

    private sealed record PlistSlice(
        string         LibraryIdentifier,
        string         LibraryPath,
        string         Platform,
        string?        PlatformVariant,
        List<string>   Architectures);

    private static List<PlistSlice> ParseInfoPlist(string plistPath)
    {
        var slices = new List<PlistSlice>();
        try
        {
            var doc      = XDocument.Load(plistPath);
            var rootDict = doc.Root?.Element("dict");
            if (rootDict is null) return slices;

            var libsArray = GetDictArray(rootDict, "AvailableLibraries");
            foreach (var libDict in libsArray)
            {
                var ident   = GetDictString(libDict, "LibraryIdentifier");
                var libPath = GetDictString(libDict, "LibraryPath");
                var plat    = GetDictString(libDict, "SupportedPlatform");
                var variant = GetDictString(libDict, "SupportedPlatformVariant"); // null if device slice
                var archs   = GetDictStringArray(libDict, "SupportedArchitectures");

                if (ident is not null && libPath is not null && plat is not null)
                    slices.Add(new PlistSlice(ident, libPath, plat, variant, archs));
            }
        }
        catch
        {
            // Malformed plist — return empty, caller will warn
        }
        return slices;
    }

    /// <summary>
    /// In a plist &lt;dict&gt;, key/value pairs are alternating siblings.
    /// Returns the string value for the given key, or null if not found.
    /// </summary>
    private static string? GetDictString(XElement dict, string key)
    {
        var takeNext = false;
        foreach (var el in dict.Elements())
        {
            if (takeNext)
                return el.Value;
            if (el.Name == "key" && el.Value == key)
                takeNext = true;
        }
        return null;
    }

    /// <summary>
    /// Returns the child &lt;dict&gt; elements of an &lt;array&gt; value for the given key.
    /// </summary>
    private static IEnumerable<XElement> GetDictArray(XElement dict, string key)
    {
        var takeNext = false;
        foreach (var el in dict.Elements())
        {
            if (takeNext)
                return el.Name == "array" ? el.Elements("dict") : [];
            if (el.Name == "key" && el.Value == key)
                takeNext = true;
        }
        return [];
    }

    /// <summary>
    /// Returns the string values inside an &lt;array&gt; value for the given key.
    /// </summary>
    private static List<string> GetDictStringArray(XElement dict, string key)
    {
        var takeNext = false;
        foreach (var el in dict.Elements())
        {
            if (takeNext)
            {
                return el.Name == "array"
                    ? el.Elements("string").Select(s => s.Value).ToList()
                    : new List<string>();
            }
            if (el.Name == "key" && el.Value == key)
                takeNext = true;
        }
        return new List<string>();
    }
}
