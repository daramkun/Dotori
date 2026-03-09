namespace Dotori.PackageManager;

/// <summary>A single package entry in the lock file.</summary>
public sealed class LockEntry
{
    public required string   Name     { get; init; }
    public required string   Version  { get; init; }
    public required string   Source   { get; init; }   // e.g. "git+https://..." or "path:../lib"
    public string?           Hash     { get; init; }   // sha256 of resolved content
    public List<string>      Deps     { get; } = new(); // "name@version" refs
}

/// <summary>The full lock file model.</summary>
public sealed class LockFile
{
    public int LockVersion { get; set; } = 1;
    public List<LockEntry> Packages { get; } = new();
}
