namespace Dotori.Core.Build;

/// <summary>
/// Expands glob patterns (include/exclude) relative to a base directory.
/// Supports ** (recursive), * (single component), ? (single char).
/// </summary>
public static class GlobExpander
{
    /// <summary>
    /// Expand a list of include/exclude source items into a deduplicated file list.
    /// </summary>
    /// <param name="baseDir">Base directory for relative patterns.</param>
    /// <param name="includes">Glob patterns to include.</param>
    /// <param name="excludes">Glob patterns to exclude from the included set.</param>
    /// <returns>Sorted, deduplicated absolute file paths.</returns>
    public static IReadOnlyList<string> Expand(
        string baseDir,
        IEnumerable<string> includes,
        IEnumerable<string> excludes)
    {
        var included = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var pattern in includes)
            foreach (var file in MatchGlob(baseDir, pattern))
                included.Add(file);

        foreach (var pattern in excludes)
            foreach (var file in MatchGlob(baseDir, pattern))
                included.Remove(file);

        return included.OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>Match a single glob pattern within <paramref name="baseDir"/>.</summary>
    /// <summary>Returns true if the pattern contains glob wildcards (* or ?).</summary>
    public static bool IsGlobPattern(string pattern) =>
        pattern.Contains('*') || pattern.Contains('?');

    public static IEnumerable<string> MatchGlob(string baseDir, string pattern)
    {
        // Normalize pattern separators
        pattern = pattern.Replace('\\', '/').TrimStart('/');

        var parts = pattern.Split('/');
        return MatchParts(baseDir, parts, 0);
    }

    private static IEnumerable<string> MatchParts(
        string current, string[] parts, int index)
    {
        if (index >= parts.Length)
            yield break;

        var part = parts[index];
        bool isLast = index == parts.Length - 1;

        if (part == "**")
        {
            // Match zero or more directories recursively
            // First try matching remaining parts without consuming any directory
            if (!isLast)
            {
                foreach (var f in MatchParts(current, parts, index + 1))
                    yield return f;
            }

            // Then recurse into each subdirectory
            if (!Directory.Exists(current)) yield break;

            foreach (var subDir in SafeGetDirectories(current))
            {
                foreach (var f in MatchParts(subDir, parts, index))
                    yield return f;
            }
        }
        else if (isLast)
        {
            // File pattern at the leaf
            if (!Directory.Exists(current)) yield break;
            foreach (var file in SafeGetFiles(current, part))
                yield return file;
        }
        else
        {
            // Directory segment (may contain wildcards)
            if (!Directory.Exists(current)) yield break;

            if (ContainsWildcard(part))
            {
                foreach (var subDir in SafeGetDirectories(current))
                {
                    if (MatchComponent(Path.GetFileName(subDir), part))
                        foreach (var f in MatchParts(subDir, parts, index + 1))
                            yield return f;
                }
            }
            else
            {
                var nextDir = Path.Combine(current, part);
                foreach (var f in MatchParts(nextDir, parts, index + 1))
                    yield return f;
            }
        }
    }

    private static IEnumerable<string> SafeGetFiles(string dir, string pattern)
    {
        string[] files;
        try
        {
            files = Directory.GetFiles(dir);
        }
        catch (UnauthorizedAccessException) { yield break; }
        catch (IOException) { yield break; }

        if (ContainsWildcard(pattern))
        {
            foreach (var file in files)
                if (MatchComponent(Path.GetFileName(file), pattern))
                    yield return file;
        }
        else
        {
            var exact = Path.Combine(dir, pattern);
            if (File.Exists(exact)) yield return exact;
        }
    }

    private static IEnumerable<string> SafeGetDirectories(string dir)
    {
        try
        {
            return Directory.GetDirectories(dir);
        }
        catch (UnauthorizedAccessException) { return []; }
        catch (IOException) { return []; }
    }

    private static bool ContainsWildcard(string s) => s.Contains('*') || s.Contains('?');

    /// <summary>Match a file or directory name against a glob component (no path separators).</summary>
    public static bool MatchComponent(string name, string pattern)
    {
        return MatchHelper(name.AsSpan(), pattern.AsSpan());
    }

    private static bool MatchHelper(ReadOnlySpan<char> name, ReadOnlySpan<char> pattern)
    {
        while (true)
        {
            if (pattern.IsEmpty) return name.IsEmpty;
            if (pattern[0] == '*')
            {
                pattern = pattern[1..];
                if (pattern.IsEmpty) return true;
                for (int i = 0; i <= name.Length; i++)
                    if (MatchHelper(name[i..], pattern)) return true;
                return false;
            }
            if (name.IsEmpty) return false;
            if (pattern[0] != '?' &&
                !char.ToLowerInvariant(pattern[0]).Equals(char.ToLowerInvariant(name[0])))
                return false;
            name    = name[1..];
            pattern = pattern[1..];
        }
    }
}
