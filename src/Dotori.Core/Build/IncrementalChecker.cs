using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dotori.Core;

namespace Dotori.Core.Build;

// NativeAOT-safe serialization context for the hash dictionary
[JsonSerializable(typeof(Dictionary<string, string>))]
internal sealed partial class HashDbJsonContext : JsonSerializerContext { }

/// <summary>
/// Hash-based incremental build checker.
/// Stores file hashes in <c>.dotori-cache/hashes.db</c> (JSON format).
/// </summary>
public sealed class IncrementalChecker : IDisposable
{
    private readonly string _dbPath;
    private Dictionary<string, string> _hashes;  // path → SHA256 hex
    private bool _dirty;

    public IncrementalChecker(string projectDir)
    {
        var cacheDir = Path.Combine(projectDir, DotoriConstants.CacheDir);
        Directory.CreateDirectory(cacheDir);
        _dbPath  = Path.Combine(cacheDir, DotoriConstants.HashDbFileName);
        _hashes  = Load(_dbPath);
        _dirty   = false;
    }

    /// <summary>
    /// Returns true if the file has changed since it was last recorded.
    /// A file is considered changed if it doesn't exist in the DB or its hash differs.
    /// </summary>
    public bool IsChanged(string filePath)
    {
        var hash = ComputeHash(filePath);
        if (hash is null) return true;  // file doesn't exist — treat as changed

        if (_hashes.TryGetValue(filePath, out var stored))
            return stored != hash;

        return true;
    }

    /// <summary>
    /// Record the current hash of a file (call after successful compilation).
    /// </summary>
    public void Record(string filePath)
    {
        var hash = ComputeHash(filePath);
        if (hash is null) return;

        if (!_hashes.TryGetValue(filePath, out var existing) || existing != hash)
        {
            _hashes[filePath] = hash;
            _dirty = true;
        }
    }

    /// <summary>Flush the hash DB to disk if it has been modified.</summary>
    public void Save()
    {
        if (!_dirty) return;
        var json = JsonSerializer.Serialize(_hashes, HashDbJsonContext.Default.DictionaryStringString);
        File.WriteAllText(_dbPath, json, Encoding.UTF8);
        _dirty = false;
    }

    public void Dispose() => Save();

    private static Dictionary<string, string> Load(string path)
    {
        if (!File.Exists(path))
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            var json   = File.ReadAllText(path, Encoding.UTF8);
            var loaded = JsonSerializer.Deserialize(json, HashDbJsonContext.Default.DictionaryStringString);
            return loaded is not null
                ? new Dictionary<string, string>(loaded, StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static string? ComputeHash(string filePath)
    {
        if (!File.Exists(filePath)) return null;
        try
        {
            using var stream = File.OpenRead(filePath);
            var bytes = SHA256.HashData(stream);
            return Convert.ToHexStringLower(bytes);
        }
        catch
        {
            return null;
        }
    }
}
