using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dotori.Core.Model;
using Dotori.Core.Parsing;

namespace Dotori.Core.Build;

// NativeAOT-safe serialization context for the copy manifest
[JsonSerializable(typeof(Dictionary<string, string>))]
internal sealed partial class CopyManifestJsonContext : JsonSerializerContext { }

public sealed partial class BuildPlanner
{
    // ─── copy { } block file copying ──────────────────────────────────────────

    /// <summary>
    /// Copies files declared in <c>copy { }</c> blocks to their target directories.
    /// Only copies files whose content hash has changed since the last build (incremental).
    /// Records each copied destination in <c>.dotori-cache/copy-manifest.json</c>
    /// so that <c>dotori clean</c> can remove individual files.
    /// </summary>
    public void CopyCopyItems(IncrementalChecker checker)
    {
        if (_model.CopyItems.Count == 0) return;

        var manifestPath = Path.Combine(
            _model.ProjectDir, DotoriConstants.CacheDir, DotoriConstants.CopyManifestFileName);

        // Load existing manifest (srcPath → destPath)
        var manifest = LoadManifest(manifestPath);

        foreach (var ci in _model.CopyItems)
        {
            var absFrom = PathUtils.MakeAbsolute(_model.ProjectDir, ci.From);
            var absTo   = PathUtils.MakeAbsolute(_model.ProjectDir, ci.To);

            // Determine source root (for preserving relative structure)
            // and enumerate matched source files.
            string srcRoot;
            IEnumerable<string> sourceFiles;

            if (Directory.Exists(absFrom))
            {
                // Directory: enumerate all files, root = directory itself
                srcRoot     = absFrom;
                sourceFiles = EnumerateFilesRecursive(absFrom);
            }
            else if (GlobExpander.IsGlobPattern(ci.From))
            {
                // Glob: use project dir as base, determine root from non-wildcard prefix
                var baseDir = Path.IsPathRooted(ci.From)
                    ? Path.GetDirectoryName(ci.From)!
                    : _model.ProjectDir;
                srcRoot     = GetGlobRoot(ci.From, baseDir);
                sourceFiles = GlobExpander.MatchGlob(baseDir, ci.From);
            }
            else if (File.Exists(absFrom))
            {
                // Single file
                srcRoot     = Path.GetDirectoryName(absFrom)!;
                sourceFiles = [absFrom];
            }
            else
            {
                // Pattern or path doesn't match anything — skip silently
                continue;
            }

            foreach (var srcFile in sourceFiles)
            {
                if (!File.Exists(srcFile)) continue;

                if (!checker.IsChanged(srcFile)) continue;

                // Compute destination path: preserve relative structure from srcRoot
                var relPath  = Path.GetRelativePath(srcRoot, srcFile);
                var destFile = Path.GetFullPath(Path.Combine(absTo, relPath));

                Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
                File.Copy(srcFile, destFile, overwrite: true);

                checker.Record(srcFile);
                manifest[srcFile] = destFile;
            }
        }

        SaveManifest(manifestPath, manifest);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the non-wildcard directory prefix of a glob pattern.
    /// e.g. "assets/sub/**/*.png" → "&lt;baseDir&gt;/assets/sub"
    /// </summary>
    private static string GetGlobRoot(string pattern, string baseDir)
    {
        var normalized = pattern.Replace('\\', '/');
        // Find the first segment that contains a wildcard
        var parts = normalized.Split('/');
        var rootParts = new List<string>();
        foreach (var part in parts)
        {
            if (part.Contains('*') || part.Contains('?')) break;
            rootParts.Add(part);
        }

        if (rootParts.Count == 0)
            return baseDir;

        // If the pattern was relative (not absolute), combine with baseDir
        if (!Path.IsPathRooted(pattern))
            return Path.GetFullPath(Path.Combine(baseDir, string.Join(Path.DirectorySeparatorChar, rootParts)));

        return Path.GetFullPath(string.Join(Path.DirectorySeparatorChar, rootParts));
    }

    private static IEnumerable<string> EnumerateFilesRecursive(string dir)
    {
        if (!Directory.Exists(dir)) yield break;
        foreach (var file in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories))
            yield return file;
    }

    public static Dictionary<string, string> LoadManifest(string manifestPath)
    {
        if (!File.Exists(manifestPath))
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            var json = File.ReadAllText(manifestPath, Encoding.UTF8);
            var loaded = JsonSerializer.Deserialize(json, CopyManifestJsonContext.Default.DictionaryStringString);
            return loaded is not null
                ? new Dictionary<string, string>(loaded, StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static void SaveManifest(string manifestPath, Dictionary<string, string> manifest)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(manifestPath)!);
        var json = JsonSerializer.Serialize(manifest, CopyManifestJsonContext.Default.DictionaryStringString);
        File.WriteAllText(manifestPath, json, Encoding.UTF8);
    }
}
