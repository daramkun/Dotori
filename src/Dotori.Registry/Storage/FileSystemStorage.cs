namespace Dotori.Registry.Storage;

public sealed class FileSystemStorage(IConfiguration config) : IPackageStorage
{
    private string Root => config["Registry:StorageRoot"] ?? "packages";

    private string ArchivePath(string owner, string name, string version) =>
        Path.Combine(Root, owner, name, version, $"{name}-{version}.dotori-pkg");

    public Task<Stream> GetArchiveAsync(string owner, string name, string version, CancellationToken ct = default)
    {
        var path = ArchivePath(owner, name, version);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Package archive not found: {owner}/{name}@{version}");

        Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 81920, useAsync: true);
        return Task.FromResult(stream);
    }

    public async Task SaveArchiveAsync(string owner, string name, string version, Stream data, CancellationToken ct = default)
    {
        var path = ArchivePath(owner, name, version);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None,
            bufferSize: 81920, useAsync: true);
        await data.CopyToAsync(file, ct);
    }

    public Task<bool> ExistsAsync(string owner, string name, string version, CancellationToken ct = default) =>
        Task.FromResult(File.Exists(ArchivePath(owner, name, version)));

    public Task DeleteArchiveAsync(string owner, string name, string version, CancellationToken ct = default)
    {
        var path = ArchivePath(owner, name, version);
        if (File.Exists(path))
            File.Delete(path);
        return Task.CompletedTask;
    }
}
