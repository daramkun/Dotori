using Dotori.LanguageServer.Protocol;

namespace Dotori.LanguageServer.Providers;

/// <summary>
/// Provides Go-to-Definition for <c>path = "..."</c> inside a dependencies block.
/// </summary>
public static class DefinitionProvider
{
    /// <summary>
    /// Try to find the definition location for the symbol at the given position.
    /// Returns null when no definition is available.
    /// </summary>
    public static LspLocation? GetDefinition(string text, int line, int character, string? localFilePath)
    {
        if (localFilePath is null) return null;

        var lines = text.Split('\n');
        if (line >= lines.Length) return null;

        var currentLine = lines[line];

        // We only resolve `path = "..."` values.
        // Detect: cursor is on a quoted string value
        var pathValue = ExtractPathValueAtCursor(lines, line, character);
        if (pathValue is null) return null;

        var projectDir = Path.GetDirectoryName(localFilePath) ?? ".";
        var resolved = Path.IsPathRooted(pathValue)
            ? pathValue
            : Path.GetFullPath(Path.Combine(projectDir, pathValue));

        var targetDotori = Path.Combine(resolved, ".dotori");
        if (!File.Exists(targetDotori)) return null;

        return new LspLocation
        {
            Uri   = PathToUri(targetDotori),
            Range = new LspRange
            {
                Start = new LspPosition { Line = 0, Character = 0 },
                End   = new LspPosition { Line = 0, Character = 0 },
            },
        };
    }

    /// <summary>
    /// Determine if the cursor is on a path string value in a dependencies block.
    /// Returns the path string if yes, null otherwise.
    /// </summary>
    private static string? ExtractPathValueAtCursor(string[] lines, int line, int character)
    {
        var currentLine = lines[line];

        // Check if cursor is inside a string literal
        var strRange = FindStringAtCursor(currentLine, character);
        if (strRange is null) return null;

        var (strStart, strEnd) = strRange.Value;
        var stringValue = currentLine[(strStart + 1)..strEnd]; // strip quotes

        // Check that this line is `path = "..."` pattern
        var beforeStr = currentLine[..strStart].TrimEnd();
        if (!beforeStr.EndsWith("=", StringComparison.Ordinal)) return null;

        var propPart = beforeStr[..^1].Trim(); // remove trailing =
        if (!propPart.Equals("path", StringComparison.OrdinalIgnoreCase)) return null;

        // Confirm we're inside a dependencies block by scanning backwards
        if (!IsInsideDependenciesBlock(lines, line))
            return null;

        return stringValue;
    }

    /// <summary>
    /// Find the string literal (quoted) under the cursor.
    /// Returns (openQuoteIndex, closeQuoteIndex) or null.
    /// </summary>
    private static (int start, int end)? FindStringAtCursor(string line, int character)
    {
        // Find the nearest opening quote before cursor
        int openIdx = -1;
        for (int i = character - 1; i >= 0; i--)
        {
            if (line[i] == '"')
            {
                openIdx = i;
                break;
            }
        }
        if (openIdx < 0)
        {
            // cursor might be on the quote itself
            if (character < line.Length && line[character] == '"')
                openIdx = character;
            else
                return null;
        }

        // Find closing quote
        int closeIdx = -1;
        for (int i = openIdx + 1; i < line.Length; i++)
        {
            if (line[i] == '"')
            {
                closeIdx = i;
                break;
            }
        }
        if (closeIdx < 0) return null;
        if (character < openIdx || character > closeIdx) return null;

        return (openIdx, closeIdx);
    }

    /// <summary>
    /// Scan backwards to determine if a line is inside a `dependencies { }` block.
    /// </summary>
    private static bool IsInsideDependenciesBlock(string[] lines, int currentLine)
    {
        int depth = 0;
        for (int i = currentLine; i >= 0; i--)
        {
            var l = lines[i];
            for (int j = (i == currentLine ? currentLine - 1 : l.Length - 1); j >= 0; j--)
            {
                // Simple scan – not perfectly handling strings/comments but good enough
                if (j < l.Length)
                {
                    if (l[j] == '}') depth++;
                    else if (l[j] == '{') depth--;
                }
            }

            if (depth < 0)
            {
                // We found the opening brace. Check what keyword precedes it.
                var trimmed = l.TrimStart();
                if (trimmed.StartsWith("dependencies", StringComparison.OrdinalIgnoreCase))
                    return true;
                // Could be a nested complex dep value: `depname = { path = "..." }`
                // Just check one more level
                return false;
            }
        }
        return false;
    }

    private static string PathToUri(string absolutePath)
    {
        return new Uri(absolutePath).AbsoluteUri;
    }
}
