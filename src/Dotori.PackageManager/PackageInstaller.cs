using System.Formats.Tar;
using System.IO.Compression;
using System.Security.Cryptography;

namespace Dotori.PackageManager;

/// <summary>
/// 레지스트리에서 패키지를 다운로드하고 프로젝트 로컬 deps/ 디렉토리에 압축해제합니다.
/// 저장 위치: &lt;depsRoot&gt;/&lt;name&gt;/
/// </summary>
public static class PackageInstaller
{
    /// <summary>
    /// Returns the local deps directory for the given package.
    /// </summary>
    /// <param name="depsRoot">Absolute path to the project's deps/ directory.</param>
    /// <param name="name">Package name (short name, without owner prefix).</param>
    public static string GetLocalDepDir(string depsRoot, string name) =>
        Path.Combine(depsRoot, name);

    /// <summary>
    /// Returns true if the package has already been installed into deps/.
    /// </summary>
    public static bool IsInstalled(string depsRoot, string name)
    {
        var dir = GetLocalDepDir(depsRoot, name);
        return Directory.Exists(dir) && File.Exists(Path.Combine(dir, ".dotori"));
    }

    public static async Task<string> InstallAsync(
        string depsRoot,
        RegistryClient client,
        string registryUrl,
        string owner,
        string name,
        string version,
        CancellationToken ct = default)
    {
        var depDir = GetLocalDepDir(depsRoot, name);
        if (IsInstalled(depsRoot, name))
            return depDir;

        Directory.CreateDirectory(depDir);

        await using var archiveStream = await client.DownloadAsync(owner, name, version, ct);

        // 임시 파일에 다운로드
        var tmpFile = Path.Combine(depDir, $".{name}-{version}.dotori-pkg.tmp");
        try
        {
            await using (var fs = new FileStream(tmpFile, FileMode.Create, FileAccess.Write, FileShare.None,
                             bufferSize: 81920, useAsync: true))
            {
                await archiveStream.CopyToAsync(fs, ct);
            }

            // 압축 해제
            await ExtractAsync(tmpFile, depDir, ct);
        }
        finally
        {
            if (File.Exists(tmpFile))
                File.Delete(tmpFile);
        }

        return depDir;
    }

    private static async Task ExtractAsync(string archivePath, string targetDir, CancellationToken ct)
    {
        await using var fs = new FileStream(archivePath, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 81920, useAsync: true);
        await using var gz = new GZipStream(fs, CompressionMode.Decompress);
        using var tar = new TarReader(gz);

        while (await tar.GetNextEntryAsync(false, ct) is { } entry)
        {
            // 경로 트래버설 방지
            var entryName = entry.Name.TrimStart('.', '/');
            if (entryName.Contains("..")) continue;

            var destPath = Path.GetFullPath(Path.Combine(targetDir, entryName));
            if (!destPath.StartsWith(targetDir, StringComparison.Ordinal)) continue;

            if (entry.EntryType == TarEntryType.Directory)
            {
                Directory.CreateDirectory(destPath);
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
            await using var dest = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None,
                bufferSize: 81920, useAsync: true);
            if (entry.DataStream is not null)
                await entry.DataStream.CopyToAsync(dest, ct);
        }
    }

    /// <summary>프로젝트 디렉토리를 .dotori-pkg 아카이브로 패키징합니다.</summary>
    public static async Task<string> PackAsync(
        string projectDir,
        string outputPath,
        IEnumerable<string> filePaths,
        CancellationToken ct = default)
    {
        await using var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None,
            bufferSize: 81920, useAsync: true);
        await using var gz = new GZipStream(fs, CompressionLevel.Optimal);
        await using var tar = new TarWriter(gz, TarEntryFormat.Gnu);

        foreach (var filePath in filePaths)
        {
            var relativePath = Path.GetRelativePath(projectDir, filePath).Replace('\\', '/');
            if (relativePath.StartsWith("..")) continue;

            var entry = new GnuTarEntry(TarEntryType.RegularFile, relativePath);
            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: 81920, useAsync: true);
            entry.DataStream = fileStream;
            await tar.WriteEntryAsync(entry, ct);
        }

        return outputPath;
    }

    public static async Task<string> ComputeHashAsync(string archivePath, CancellationToken ct = default)
    {
        await using var fs = new FileStream(archivePath, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 81920, useAsync: true);
        var hash = await SHA256.HashDataAsync(fs, ct);
        return $"sha256:{Convert.ToHexString(hash).ToLowerInvariant()}";
    }
}
