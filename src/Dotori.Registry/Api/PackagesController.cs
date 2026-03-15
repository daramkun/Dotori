using System.Security.Claims;
using Dotori.Registry.Api.Dtos;
using Dotori.Registry.Database;
using Dotori.Registry.Database.Models;
using Dotori.Registry.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dotori.Registry.Api;

[ApiController]
[Route("api/v1/packages")]
public sealed class PackagesController(
    RegistryDbContext db,
    IPackageStorage storage,
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory) : ControllerBase
{
    // GET /api/v1/packages/{owner}/{name}
    [HttpGet("{owner}/{name}")]
    public async Task<IActionResult> GetPackage(string owner, string name, CancellationToken ct)
    {
        var ownerNorm = owner.ToLowerInvariant();
        var nameNorm = name.ToLowerInvariant();

        var pkg = await db.Packages
            .Include(p => p.Owner)
            .Include(p => p.Versions.OrderByDescending(v => v.PublishedAt))
            .ThenInclude(v => v.PublishedBy)
            .FirstOrDefaultAsync(p => p.Owner.Username == ownerNorm && p.Name == nameNorm, ct);

        if (pkg is null)
            return await ProxyGetAsync($"api/v1/packages/{owner}/{name}", ct)
                   ?? NotFound(new { error = $"Package '{owner}/{name}' not found" });

        return Ok(ToListDto(pkg));
    }

    // GET /api/v1/packages/{owner}/{name}/{version}
    [HttpGet("{owner}/{name}/{version}")]
    public async Task<IActionResult> GetVersion(string owner, string name, string version, CancellationToken ct)
    {
        var ownerNorm = owner.ToLowerInvariant();
        var nameNorm = name.ToLowerInvariant();
        var versionNorm = version.ToLowerInvariant();

        var ver = await db.PackageVersions
            .Include(v => v.Package).ThenInclude(p => p.Owner)
            .Include(v => v.PublishedBy)
            .FirstOrDefaultAsync(v =>
                v.Package.Owner.Username == ownerNorm &&
                v.Package.Name == nameNorm &&
                v.Version == versionNorm, ct);

        if (ver is null)
            return await ProxyGetAsync($"api/v1/packages/{owner}/{name}/{version}", ct)
                   ?? NotFound(new { error = $"Package '{owner}/{name}@{version}' not found" });

        return Ok(ToVersionDto(ver));
    }

    // GET /api/v1/packages/{owner}/{name}/{version}/download
    [HttpGet("{owner}/{name}/{version}/download")]
    public async Task<IActionResult> Download(string owner, string name, string version, CancellationToken ct)
    {
        var ownerNorm = owner.ToLowerInvariant();
        var nameNorm = name.ToLowerInvariant();
        var versionNorm = version.ToLowerInvariant();

        var ver = await db.PackageVersions
            .Include(v => v.Package).ThenInclude(p => p.Owner)
            .FirstOrDefaultAsync(v =>
                v.Package.Owner.Username == ownerNorm &&
                v.Package.Name == nameNorm &&
                v.Version == versionNorm, ct);

        if (ver is null)
        {
            // In proxy mode, stream from upstream and optionally cache
            var proxied = await ProxyDownloadAsync(ownerNorm, nameNorm, versionNorm, ct);
            if (proxied is not null) return proxied;
            return NotFound(new { error = $"Package '{owner}/{name}@{version}' not found" });
        }

        if (ver.Yanked)
            return BadRequest(new { error = $"Version {version} has been yanked" });

        ver.DownloadCount++;
        await db.SaveChangesAsync(ct);

        var stream = await storage.GetArchiveAsync(ownerNorm, nameNorm, versionNorm, ct);
        return File(stream, "application/octet-stream", $"{nameNorm}-{versionNorm}.dotori-pkg");
    }

    // POST /api/v1/packages/publish
    [HttpPost("publish")]
    [Authorize]
    public async Task<IActionResult> Publish(IFormFile archive, CancellationToken ct)
    {
        if (archive is null || archive.Length == 0)
            return BadRequest(new { error = "Archive file is required" });

        if (!archive.FileName.EndsWith(".dotori-pkg", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Archive must be a .dotori-pkg file" });

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // 아카이브에서 manifest 파일 추출
        string manifest;
        string pkgName;
        string pkgVersion;
        try
        {
            (manifest, pkgName, pkgVersion) = await ExtractManifestAsync(archive, ct);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        var ownerUser = await db.Users.FindAsync([userId], ct);
        if (ownerUser is null) return Unauthorized();

        var ownerNorm = ownerUser.Username.ToLowerInvariant();
        var nameNorm = pkgName.ToLowerInvariant();
        var versionNorm = pkgVersion.ToLowerInvariant();

        // 중복 버전 확인
        var existingPkg = await db.Packages
            .Include(p => p.Collaborators)
            .FirstOrDefaultAsync(p => p.OwnerId == userId && p.Name == nameNorm, ct);

        if (existingPkg is not null)
        {
            // 발행 권한 확인 (owner 또는 collaborator)
            var isAllowed = existingPkg.Collaborators.Any(c => c.UserId == userId);
            if (!isAllowed)
                return Forbid();

            var existing = await db.PackageVersions.AnyAsync(
                v => v.PackageId == existingPkg.Id && v.Version == versionNorm, ct);
            if (existing)
                return Conflict(new { error = $"Version '{pkgVersion}' already exists" });
        }

        // SHA-256 계산
        await using var archiveStream = archive.OpenReadStream();
        var hashBytes = await System.Security.Cryptography.SHA256.HashDataAsync(archiveStream, ct);
        var hash = $"sha256:{Convert.ToHexString(hashBytes).ToLowerInvariant()}";

        // 스토리지 저장
        archiveStream.Position = 0;
        await storage.SaveArchiveAsync(ownerNorm, nameNorm, versionNorm, archiveStream, ct);

        // DB 저장
        if (existingPkg is null)
        {
            existingPkg = new PackageModel
            {
                Name = nameNorm,
                OwnerId = userId,
                Description = null,
            };
            db.Packages.Add(existingPkg);
            await db.SaveChangesAsync(ct);

            // 첫 publish → owner로 등록
            db.PackageCollaborators.Add(new PackageCollaboratorModel
            {
                PackageId = existingPkg.Id,
                UserId = userId,
                Role = "owner",
            });
        }

        var newVersion = new PackageVersionModel
        {
            PackageId = existingPkg.Id,
            Version = versionNorm,
            Hash = hash,
            ArchiveSize = archive.Length,
            PublishedById = userId,
            Manifest = manifest,
        };
        db.PackageVersions.Add(newVersion);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetVersion),
            new { owner = ownerNorm, name = nameNorm, version = versionNorm },
            new { owner = ownerNorm, name = nameNorm, version = versionNorm, hash });
    }

    // DELETE /api/v1/packages/{owner}/{name}/{version}/yank
    [HttpDelete("{owner}/{name}/{version}/yank")]
    [Authorize]
    public async Task<IActionResult> Yank(string owner, string name, string version, CancellationToken ct) =>
        await SetYanked(owner, name, version, true, ct);

    // POST /api/v1/packages/{owner}/{name}/{version}/unyank
    [HttpPost("{owner}/{name}/{version}/unyank")]
    [Authorize]
    public async Task<IActionResult> Unyank(string owner, string name, string version, CancellationToken ct) =>
        await SetYanked(owner, name, version, false, ct);

    private async Task<IActionResult> SetYanked(string owner, string name, string version, bool yanked, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ownerNorm = owner.ToLowerInvariant();
        var nameNorm = name.ToLowerInvariant();
        var versionNorm = version.ToLowerInvariant();

        var ver = await db.PackageVersions
            .Include(v => v.Package).ThenInclude(p => p.Collaborators)
            .FirstOrDefaultAsync(v =>
                v.Package.Owner.Username == ownerNorm &&
                v.Package.Name == nameNorm &&
                v.Version == versionNorm, ct);

        if (ver is null)
            return NotFound(new { error = $"Package '{owner}/{name}@{version}' not found" });

        if (!ver.Package.Collaborators.Any(c => c.UserId == userId))
            return Forbid();

        ver.Yanked = yanked;
        await db.SaveChangesAsync(ct);
        return Ok(new { yanked });
    }

    private static async Task<(string manifest, string name, string version)> ExtractManifestAsync(
        IFormFile archive, CancellationToken ct)
    {
        await using var stream = archive.OpenReadStream();
        using var gz = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress);
        using var tar = new System.Formats.Tar.TarReader(gz);

        while (await tar.GetNextEntryAsync(false, ct) is { } entry)
        {
            if (entry.Name is ".dotori" or "./.dotori")
            {
                await using var ms = new MemoryStream();
                if (entry.DataStream is not null)
                    await entry.DataStream.CopyToAsync(ms, ct);
                var text = System.Text.Encoding.UTF8.GetString(ms.ToArray());
                var (name, version) = ParsePackageInfo(text);
                return (text, name, version);
            }
        }

        throw new InvalidOperationException("Archive does not contain a .dotori file");
    }

    private static (string name, string version) ParsePackageInfo(string manifest)
    {
        // 간단한 파싱: package { name = "..." version = "..." }
        string? name = null, version = null;
        bool inPackage = false;
        int depth = 0;

        foreach (var rawLine in manifest.Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.StartsWith("package") && line.Contains('{')) { inPackage = true; depth = 1; continue; }
            if (!inPackage) continue;

            depth += line.Count(c => c == '{') - line.Count(c => c == '}');
            if (depth <= 0) break;

            if (line.StartsWith("name") && line.Contains('='))
                name = line.Split('=', 2)[1].Trim().Trim('"');
            else if (line.StartsWith("version") && line.Contains('='))
                version = line.Split('=', 2)[1].Trim().Trim('"');
        }

        if (name is null) throw new InvalidOperationException("package block missing 'name'");
        if (version is null) throw new InvalidOperationException("package block missing 'version'");
        return (name, version);
    }

    // Proxy helpers ─────────────────────────────────────────────────────────

    private string? ProxyUpstream =>
        configuration["Registry:Mode"] == "proxy"
            ? configuration["Registry:ProxyUpstream"]
            : null;

    /// <summary>Forward a GET request to upstream and return the response, or null if not proxy mode.</summary>
    private async Task<IActionResult?> ProxyGetAsync(string path, CancellationToken ct)
    {
        var upstream = ProxyUpstream;
        if (upstream is null) return null;

        try
        {
            var http = httpClientFactory.CreateClient("proxy");
            var url = $"{upstream.TrimEnd('/')}/{path}";
            var resp = await http.GetAsync(url, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            return StatusCode((int)resp.StatusCode, body);
        }
        catch { return null; }
    }

    /// <summary>Forward a download request to upstream. Caches locally if successful.</summary>
    private async Task<IActionResult?> ProxyDownloadAsync(
        string owner, string name, string version, CancellationToken ct)
    {
        var upstream = ProxyUpstream;
        if (upstream is null) return null;

        try
        {
            var http = httpClientFactory.CreateClient("proxy");
            var url = $"{upstream.TrimEnd('/')}/api/v1/packages/{owner}/{name}/{version}/download";
            var resp = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!resp.IsSuccessStatusCode) return null;

            var archiveStream = await resp.Content.ReadAsStreamAsync(ct);

            // Cache to local storage asynchronously (fire-and-forget for streaming response)
            var ms = new MemoryStream();
            await archiveStream.CopyToAsync(ms, ct);
            ms.Position = 0;

            // Save to storage cache (ignore errors)
            try { await storage.SaveArchiveAsync(owner, name, version, ms, ct); } catch { }
            ms.Position = 0;

            return File(ms, "application/octet-stream", $"{name}-{version}.dotori-pkg");
        }
        catch { return null; }
    }

    private static PackageListDto ToListDto(PackageModel pkg) => new()
    {
        Owner = pkg.Owner.Username,
        Name = pkg.Name,
        FullName = $"{pkg.Owner.Username}/{pkg.Name}",
        LatestVersion = pkg.Versions.FirstOrDefault(v => !v.Yanked)?.Version,
        Description = pkg.Description,
        TotalDownloads = pkg.Versions.Sum(v => v.DownloadCount),
        CreatedAt = pkg.CreatedAt,
        Versions = pkg.Versions.Select(ToVersionDto).ToList(),
    };

    private static PackageVersionDto ToVersionDto(PackageVersionModel v) => new()
    {
        Owner = v.Package.Owner.Username,
        Name = v.Package.Name,
        FullName = $"{v.Package.Owner.Username}/{v.Package.Name}",
        Version = v.Version,
        Hash = v.Hash,
        DownloadCount = v.DownloadCount,
        Yanked = v.Yanked,
        PublishedAt = v.PublishedAt,
        PublishedBy = v.PublishedBy.Username,
    };
}
