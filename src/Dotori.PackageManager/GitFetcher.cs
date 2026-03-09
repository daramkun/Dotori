using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Dotori.PackageManager;

/// <summary>
/// Fetches packages from git repositories.
/// Cache location: ~/.dotori/packages/&lt;name&gt;/&lt;version&gt;/
/// </summary>
public static class GitFetcher
{
    private static string CacheRoot =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".dotori", "packages");

    /// <summary>
    /// Fetch (or use cached) a git dependency.
    /// </summary>
    /// <param name="name">Package name.</param>
    /// <param name="gitUrl">Git repository URL.</param>
    /// <param name="tagOrCommit">Tag or commit hash to check out.</param>
    /// <returns>Absolute path to the local checkout directory.</returns>
    public static async Task<string> FetchAsync(
        string name,
        string gitUrl,
        string tagOrCommit,
        CancellationToken ct = default)
    {
        var cacheDir = Path.Combine(CacheRoot, name, SanitizeVersion(tagOrCommit));
        if (Directory.Exists(Path.Combine(cacheDir, ".git")) ||
            Directory.Exists(cacheDir) && Directory.GetFiles(cacheDir).Length > 0)
        {
            // Already cached
            return cacheDir;
        }

        Directory.CreateDirectory(cacheDir);

        // Clone with depth 1 if it looks like a tag; full clone for commits
        bool isTag    = tagOrCommit.StartsWith('v') || !IsLikelyCommitHash(tagOrCommit);
        string branch = isTag ? $"--branch {tagOrCommit} --depth 1" : "--depth 1";

        await RunGitAsync($"clone {branch} {gitUrl} \"{cacheDir}\"", ct);

        if (!isTag)
        {
            // Checkout specific commit
            await RunGitAsync($"-C \"{cacheDir}\" checkout {tagOrCommit}", ct);
        }

        return cacheDir;
    }

    /// <summary>
    /// Compute a SHA256 hash of all files in a directory (for the lock file).
    /// </summary>
    public static string ComputeDirectoryHash(string dir)
    {
        var files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories)
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToList();

        using var sha = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (var file in files)
        {
            sha.AppendData(Encoding.UTF8.GetBytes(file));
            sha.AppendData(File.ReadAllBytes(file));
        }

        return "sha256:" + Convert.ToHexStringLower(sha.GetHashAndReset());
    }

    private static bool IsLikelyCommitHash(string s) =>
        s.Length >= 7 && s.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'));

    private static string SanitizeVersion(string v) =>
        string.Concat(v.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));

    private static async Task RunGitAsync(string args, CancellationToken ct)
    {
        var psi = new ProcessStartInfo("git", args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
            CreateNoWindow         = true,
        };

        using var proc = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start git");

        await proc.WaitForExitAsync(ct);

        if (proc.ExitCode != 0)
        {
            var stderr = await proc.StandardError.ReadToEndAsync(ct);
            throw new PackageManagerException(
                $"git {args} failed (exit {proc.ExitCode}): {stderr}");
        }
    }
}
