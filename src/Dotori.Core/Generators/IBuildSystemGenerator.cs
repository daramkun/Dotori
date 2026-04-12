using Dotori.Core.Model;

namespace Dotori.Core.Generators;

/// <summary>
/// Generates build system files from a flattened project model.
/// A generator may produce multiple files (e.g. .vcxproj + .vcxproj.filters).
/// Each returned tuple contains a relative path and the file content.
/// </summary>
public interface IBuildSystemGenerator
{
    /// <summary>Format identifier (e.g. "cmake", "meson", "vcxproj").</summary>
    string FormatId { get; }

    /// <summary>
    /// Generate build system file(s) for the given project model.
    /// </summary>
    /// <param name="model">Flattened project configuration.</param>
    /// <param name="config">"debug", "release", or "both".</param>
    /// <param name="targetId">Target platform triple (e.g. "macos-arm64").</param>
    /// <returns>
    /// One or more (relativePath, content) pairs.
    /// Paths are relative to the project directory.
    /// </returns>
    IReadOnlyList<(string RelativePath, string Content)> Generate(
        FlatProjectModel model,
        string config,
        string targetId);
}
