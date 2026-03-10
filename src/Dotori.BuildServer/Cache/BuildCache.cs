using System.Security.Cryptography;

namespace Dotori.BuildServer.Cache;

/// <summary>
/// Content-addressable cache for compiled object files.
/// Key = SHA-256(source_hash + compiler_args_hash).
/// Stored under <c>~/.dotori/build-cache/</c> by default.
/// </summary>
public sealed class BuildCache
{
    private readonly string _cacheDir;

    public BuildCache(string? cacheDir = null)
    {
        _cacheDir = cacheDir
            ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".dotori", "build-cache");
        Directory.CreateDirectory(_cacheDir);
    }

    /// <summary>Compute cache key from source hash and compiler args.</summary>
    public static string ComputeKey(string sourceHash, IEnumerable<string> args)
    {
        var combined = sourceHash + string.Concat(args);
        var bytes    = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(combined));
        return Convert.ToHexStringLower(bytes);
    }

    /// <summary>Try to get a cached object file.</summary>
    public bool TryGet(string key, out byte[] objBytes)
    {
        var path = CachePath(key);
        if (!File.Exists(path)) { objBytes = []; return false; }
        objBytes = File.ReadAllBytes(path);
        return true;
    }

    /// <summary>Store compiled object bytes under the given key.</summary>
    public void Put(string key, byte[] objBytes)
    {
        File.WriteAllBytes(CachePath(key), objBytes);
    }

    private string CachePath(string key) => Path.Combine(_cacheDir, key + ".o");
}
