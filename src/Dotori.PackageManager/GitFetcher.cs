using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Dotori.PackageManager;

/// <summary>
/// Fetches packages from git repositories.
/// Packages are stored in the project-local deps/ directory: &lt;depsRoot&gt;/&lt;name&gt;/
/// </summary>
public static class GitFetcher
{
    /// <summary>
    /// Returns the local deps directory for the given package if it already exists,
    /// otherwise null.
    /// </summary>
    /// <param name="depsRoot">Absolute path to the project's deps/ directory.</param>
    /// <param name="name">Package name.</param>
    public static string? GetLocalDepDir(string depsRoot, string name)
    {
        var depDir = Path.Combine(depsRoot, name);
        if (Directory.Exists(Path.Combine(depDir, ".git")) ||
            (Directory.Exists(depDir) && Directory.GetFiles(depDir).Length > 0))
            return depDir;
        return null;
    }

    /// <summary>
    /// Fetch (or reuse existing) a git dependency into the project-local deps/ directory.
    /// </summary>
    /// <param name="depsRoot">Absolute path to the project's deps/ directory.</param>
    /// <param name="name">Package name.</param>
    /// <param name="gitUrl">Git repository URL.</param>
    /// <param name="tagOrCommit">Tag or commit hash to check out.</param>
    /// <returns>Absolute path to the local checkout directory.</returns>
    public static async Task<string> FetchAsync(
        string depsRoot,
        string name,
        string gitUrl,
        string tagOrCommit,
        CancellationToken ct = default)
    {
        var depDir = Path.Combine(depsRoot, name);
        if (Directory.Exists(Path.Combine(depDir, ".git")) ||
            Directory.Exists(depDir) && Directory.GetFiles(depDir).Length > 0)
        {
            // Already fetched
            return depDir;
        }

        Directory.CreateDirectory(depDir);

        // Clone with depth 1 if it looks like a tag; full clone for commits
        bool isTag    = tagOrCommit.StartsWith('v') || !IsLikelyCommitHash(tagOrCommit);
        string branch = isTag ? $"--branch {tagOrCommit} --depth 1" : "--depth 1";

        await RunGitAsync($"clone {branch} {gitUrl} \"{depDir}\"", ct);

        if (!isTag)
        {
            // Checkout specific commit
            await RunGitAsync($"-C \"{depDir}\" checkout {tagOrCommit}", ct);
        }

        return depDir;
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
