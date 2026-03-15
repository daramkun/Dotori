namespace Dotori.Registry.Storage;

public interface IPackageStorage
{
    Task<Stream> GetArchiveAsync(string owner, string name, string version, CancellationToken ct = default);
    Task SaveArchiveAsync(string owner, string name, string version, Stream data, CancellationToken ct = default);
    Task<bool> ExistsAsync(string owner, string name, string version, CancellationToken ct = default);
    Task DeleteArchiveAsync(string owner, string name, string version, CancellationToken ct = default);
}
