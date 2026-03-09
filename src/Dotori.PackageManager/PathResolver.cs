using Dotori.Core.Location;

namespace Dotori.PackageManager;

/// <summary>
/// Resolves local path dependencies.
/// Path dependencies are not cached — they are referenced directly.
/// </summary>
public static class PathResolver
{
    /// <summary>
    /// Resolve a path dependency relative to the owner project directory.
    /// </summary>
    /// <param name="ownerDir">Directory of the .dotori file that declares the dependency.</param>
    /// <param name="relPath">Relative path value from the DSL (e.g. "../lib").</param>
    /// <returns>Absolute path to the dependency's .dotori file.</returns>
    public static string Resolve(string ownerDir, string relPath)
    {
        var absDir = Path.GetFullPath(Path.Combine(ownerDir, relPath));
        return ProjectLocator.ResolveExplicitPath(absDir);
    }
}
