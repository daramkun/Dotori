namespace Dotori.PackageManager;

/// <summary>
/// Represents a parsed semantic version (major.minor.patch[-prerelease]).
/// </summary>
public readonly struct SemanticVersion : IComparable<SemanticVersion>, IEquatable<SemanticVersion>
{
    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }
    public string? PreRelease { get; }

    public SemanticVersion(int major, int minor, int patch, string? preRelease = null)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        PreRelease = preRelease;
    }

    public static bool TryParse(string s, out SemanticVersion result)
    {
        s = s.Trim();
        // Strip leading 'v'
        if (s.StartsWith('v'))
            s = s[1..];

        string? preRelease = null;
        var dashIdx = s.IndexOf('-');
        if (dashIdx >= 0)
        {
            preRelease = s[(dashIdx + 1)..];
            s = s[..dashIdx];
        }

        var parts = s.Split('.');
        if (parts.Length < 1 || !int.TryParse(parts[0], out int major))
        {
            result = default;
            return false;
        }
        int minor = parts.Length > 1 && int.TryParse(parts[1], out int m) ? m : 0;
        int patch = parts.Length > 2 && int.TryParse(parts[2], out int p) ? p : 0;

        result = new SemanticVersion(major, minor, patch, preRelease);
        return true;
    }

    public static SemanticVersion Parse(string s)
    {
        if (!TryParse(s, out var v))
            throw new ArgumentException($"Invalid semantic version: {s}");
        return v;
    }

    public int CompareTo(SemanticVersion other)
    {
        int c = Major.CompareTo(other.Major);
        if (c != 0) return c;
        c = Minor.CompareTo(other.Minor);
        if (c != 0) return c;
        c = Patch.CompareTo(other.Patch);
        if (c != 0) return c;

        // Pre-release has lower precedence than release
        if (PreRelease is null && other.PreRelease is null) return 0;
        if (PreRelease is null) return 1;   // release > pre-release
        if (other.PreRelease is null) return -1;
        return string.Compare(PreRelease, other.PreRelease, StringComparison.OrdinalIgnoreCase);
    }

    public bool Equals(SemanticVersion other) => CompareTo(other) == 0;
    public override bool Equals(object? obj) => obj is SemanticVersion v && Equals(v);
    public override int GetHashCode() => HashCode.Combine(Major, Minor, Patch, PreRelease);

    public static bool operator ==(SemanticVersion a, SemanticVersion b) => a.Equals(b);
    public static bool operator !=(SemanticVersion a, SemanticVersion b) => !a.Equals(b);
    public static bool operator  <(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) < 0;
    public static bool operator  >(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) > 0;
    public static bool operator <=(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) <= 0;
    public static bool operator >=(SemanticVersion a, SemanticVersion b) => a.CompareTo(b) >= 0;

    public override string ToString() =>
        PreRelease is not null
            ? $"{Major}.{Minor}.{Patch}-{PreRelease}"
            : $"{Major}.{Minor}.{Patch}";
}

/// <summary>
/// A version constraint that can match one or more versions.
/// Supports: exact "1.2.3", caret "^1.2.3" (≥1.2.3 &lt;2.0.0),
/// tilde "~1.2.3" (≥1.2.3 &lt;1.3.0), range ">=1.0 &lt;2.0", wildcard "*".
/// </summary>
public sealed class VersionConstraint
{
    /// <summary>Lower bound (inclusive).</summary>
    public SemanticVersion? Min { get; }
    /// <summary>Upper bound (exclusive).</summary>
    public SemanticVersion? Max { get; }
    /// <summary>Exact version (when both Min == Max conceptually as an exact pin).</summary>
    public SemanticVersion? Exact { get; }
    /// <summary>True if this is a wildcard ("*") that matches any version.</summary>
    public bool IsWildcard { get; }

    private VersionConstraint(SemanticVersion? min, SemanticVersion? max, SemanticVersion? exact, bool isWildcard)
    {
        Min = min;
        Max = max;
        Exact = exact;
        IsWildcard = isWildcard;
    }

    public static VersionConstraint Any => new(null, null, null, isWildcard: true);

    /// <summary>
    /// Parse a version constraint string.
    /// Supported formats:
    ///   *           → any version
    ///   1.2.3       → exact
    ///   ^1.2.3      → caret range  (≥1.2.3 &lt;2.0.0)
    ///   ~1.2.3      → tilde range  (≥1.2.3 &lt;1.3.0)
    ///   >=1.0       → lower bound only
    ///   >=1.0 &lt;2.0  → range
    ///   &lt;2.0        → upper bound only
    /// </summary>
    public static VersionConstraint Parse(string s)
    {
        s = s.Trim();

        if (s == "*" || s == "")
            return Any;

        // Caret: ^major.minor.patch → >=major.minor.patch <(major+1).0.0
        if (s.StartsWith('^'))
        {
            var ver = SemanticVersion.Parse(s[1..]);
            var upper = new SemanticVersion(ver.Major + 1, 0, 0);
            return new VersionConstraint(ver, upper, null, false);
        }

        // Tilde: ~major.minor.patch → >=major.minor.patch <major.(minor+1).0
        if (s.StartsWith('~'))
        {
            var ver = SemanticVersion.Parse(s[1..]);
            var upper = new SemanticVersion(ver.Major, ver.Minor + 1, 0);
            return new VersionConstraint(ver, upper, null, false);
        }

        // Range with operators (may have two parts)
        if (s.StartsWith(">=") || s.StartsWith("<=") || s.StartsWith(">") || s.StartsWith("<") || s.StartsWith("="))
        {
            return ParseRange(s);
        }

        // Plain version → exact
        var exactVer = SemanticVersion.Parse(s);
        return new VersionConstraint(exactVer, null, exactVer, false);
    }

    private static VersionConstraint ParseRange(string s)
    {
        // Split on whitespace to find possible two-part ranges like ">=1.0 <2.0"
        var parts = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        SemanticVersion? min = null;
        SemanticVersion? max = null;

        foreach (var part in parts)
        {
            if (part.StartsWith(">="))
            {
                min = SemanticVersion.Parse(part[2..]);
            }
            else if (part.StartsWith("<="))
            {
                // Inclusive upper → use next patch as exclusive
                var v = SemanticVersion.Parse(part[2..]);
                max = new SemanticVersion(v.Major, v.Minor, v.Patch + 1);
            }
            else if (part.StartsWith('<'))
            {
                max = SemanticVersion.Parse(part[1..]);
            }
            else if (part.StartsWith('>'))
            {
                // Exclusive lower → use next patch as inclusive lower
                var v = SemanticVersion.Parse(part[1..]);
                min = new SemanticVersion(v.Major, v.Minor, v.Patch + 1);
            }
            else if (part.StartsWith('='))
            {
                var v = SemanticVersion.Parse(part[1..]);
                return new VersionConstraint(v, null, v, false);
            }
        }

        return new VersionConstraint(min, max, null, false);
    }

    /// <summary>Returns true if <paramref name="version"/> satisfies this constraint.</summary>
    public bool Allows(SemanticVersion version)
    {
        if (IsWildcard) return true;
        if (Exact is not null) return version == Exact;
        if (Min is not null && version < Min) return false;
        if (Max is not null && version >= Max) return false;
        return true;
    }

    /// <summary>
    /// Compute the intersection of two constraints.
    /// Returns null if the constraints are incompatible (empty intersection).
    /// </summary>
    public static VersionConstraint? Intersect(VersionConstraint a, VersionConstraint b)
    {
        if (a.IsWildcard) return b;
        if (b.IsWildcard) return a;

        // Both exact: must be equal
        if (a.Exact is not null && b.Exact is not null)
            return a.Exact == b.Exact ? a : null;

        // One exact, one range
        if (a.Exact is not null)
            return b.Allows(a.Exact.Value) ? a : null;
        if (b.Exact is not null)
            return a.Allows(b.Exact.Value) ? b : null;

        // Both ranges: compute tighter bounds
        SemanticVersion? newMin = a.Min is null ? b.Min :
                                  b.Min is null ? a.Min :
                                  a.Min > b.Min ? a.Min : b.Min;

        SemanticVersion? newMax = a.Max is null ? b.Max :
                                  b.Max is null ? a.Max :
                                  a.Max < b.Max ? a.Max : b.Max;

        // Empty intersection check
        if (newMin is not null && newMax is not null && newMin >= newMax)
            return null;

        return new VersionConstraint(newMin, newMax, null, false);
    }

    public override string ToString()
    {
        if (IsWildcard) return "*";
        if (Exact is not null) return Exact.Value.ToString();
        var min = Min is not null ? $">={Min}" : "";
        var max = Max is not null ? $"<{Max}" : "";
        return string.Join(" ", new[] { min, max }.Where(s => s != ""));
    }
}
