using System.Text;
using Dotori.Core;

namespace Dotori.PackageManager;

/// <summary>
/// Reads and writes .dotori.lock files.
/// Format: TOML-like (manual serialization to stay NativeAOT-compatible).
/// </summary>
public static class LockManager
{
    /// <summary>Load the lock file from the given project directory. Returns empty if not found.</summary>
    public static LockFile Load(string projectDir)
    {
        var path = Path.Combine(projectDir, DotoriConstants.LockFileName);
        if (!File.Exists(path)) return new LockFile();
        return Parse(File.ReadAllText(path, Encoding.UTF8));
    }

    /// <summary>Write the lock file to the given project directory.</summary>
    public static void Save(LockFile lockFile, string projectDir)
    {
        var path = Path.Combine(projectDir, DotoriConstants.LockFileName);
        File.WriteAllText(path, Serialize(lockFile), Encoding.UTF8);
    }

    // ─── Serialization ───────────────────────────────────────────────────────

    public static string Serialize(LockFile lockFile)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"lock-version = {lockFile.LockVersion}");
        sb.AppendLine();

        foreach (var pkg in lockFile.Packages)
        {
            sb.AppendLine("[[package]]");
            sb.AppendLine($"name    = \"{pkg.Name}\"");
            sb.AppendLine($"version = \"{pkg.Version}\"");
            sb.AppendLine($"source  = \"{pkg.Source}\"");
            if (pkg.Hash is not null)
                sb.AppendLine($"hash    = \"{pkg.Hash}\"");
            if (pkg.Deps.Count > 0)
            {
                var depsStr = string.Join(", ", pkg.Deps.Select(d => $"\"{d}\""));
                sb.AppendLine($"deps    = [{depsStr}]");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    // ─── Parsing ─────────────────────────────────────────────────────────────

    public static LockFile Parse(string text)
    {
        var lockFile = new LockFile();
        LockEntry? current = null;

        foreach (var rawLine in text.Split('\n'))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith('#')) continue;

            if (line == "[[package]]")
            {
                if (current is not null) lockFile.Packages.Add(current);
                current = null;
                continue;
            }

            var eqIdx = line.IndexOf('=');
            if (eqIdx < 0) continue;

            var key   = line[..eqIdx].Trim();
            var value = line[(eqIdx + 1)..].Trim();

            if (key == "lock-version")
            {
                if (int.TryParse(value, out int v))
                    lockFile.LockVersion = v;
                continue;
            }

            // We're inside a [[package]] block
            if (current is null)
            {
                // Lazy init after seeing first key
                current = new LockEntry
                {
                    Name    = string.Empty,
                    Version = string.Empty,
                    Source  = string.Empty,
                };
            }

            switch (key)
            {
                case "name":
                    current = new LockEntry
                    {
                        Name    = UnquoteString(value),
                        Version = current.Version,
                        Source  = current.Source,
                        Hash    = current.Hash,
                    };
                    foreach (var d in current.Deps) { /* deps not transferred here */ }
                    break;
                case "version":
                    current = CreateWith(current, version: UnquoteString(value));
                    break;
                case "source":
                    current = CreateWith(current, source: UnquoteString(value));
                    break;
                case "hash":
                    current = CreateWith(current, hash: UnquoteString(value));
                    break;
                case "deps":
                    // Parse ["a@1", "b@2"] format
                    var deps = ParseStringArray(value);
                    foreach (var d in deps) current.Deps.Add(d);
                    break;
            }
        }

        if (current is not null) lockFile.Packages.Add(current);
        return lockFile;
    }

    private static LockEntry CreateWith(
        LockEntry src,
        string? name    = null,
        string? version = null,
        string? source  = null,
        string? hash    = null)
    {
        var entry = new LockEntry
        {
            Name    = name    ?? src.Name,
            Version = version ?? src.Version,
            Source  = source  ?? src.Source,
            Hash    = hash    ?? src.Hash,
        };
        foreach (var d in src.Deps) entry.Deps.Add(d);
        return entry;
    }

    private static string UnquoteString(string value)
    {
        value = value.Trim();
        if (value.StartsWith('"') && value.EndsWith('"'))
            return value[1..^1];
        return value;
    }

    private static IReadOnlyList<string> ParseStringArray(string value)
    {
        // Minimal parser for ["a", "b"] format
        var result = new List<string>();
        value = value.Trim();
        if (value.StartsWith('[')) value = value[1..];
        if (value.EndsWith(']'))  value = value[..^1];

        foreach (var part in value.Split(','))
        {
            var s = UnquoteString(part.Trim());
            if (!string.IsNullOrEmpty(s)) result.Add(s);
        }
        return result;
    }
}
