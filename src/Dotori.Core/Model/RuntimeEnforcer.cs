using Dotori.Core.Parsing;

namespace Dotori.Core.Model;

/// <summary>
/// Applies mandatory runtime-link overrides based on the target platform.
///
///   UWP             → runtime-link = dynamic  (Windows Store policy)
///   iOS / tvOS / watchOS → runtime-link = static   (App Store policy)
///   WASM            → runtime-link = static   (no dynamic linking)
/// </summary>
public static class RuntimeEnforcer
{
    public static void Enforce(FlatProjectModel model, string platform)
    {
        switch (platform.ToLowerInvariant())
        {
            case "uwp":
                if (model.RuntimeLink == RuntimeLink.Static)
                {
                    // Warn: user set static but UWP requires dynamic
                    model.RuntimeLink = RuntimeLink.Dynamic;
                }
                break;

            case "ios":
            case "tvos":
            case "watchos":
                model.RuntimeLink = RuntimeLink.Static;
                break;

            case "wasm":
                model.RuntimeLink = RuntimeLink.Static;
                break;
        }
    }
}
