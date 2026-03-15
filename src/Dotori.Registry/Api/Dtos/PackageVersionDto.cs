namespace Dotori.Registry.Api.Dtos;

public sealed class PackageVersionDto
{
    public required string Owner { get; init; }
    public required string Name { get; init; }
    public required string FullName { get; init; }  // "owner/name"
    public required string Version { get; init; }
    public string? Description { get; init; }
    public required string Hash { get; init; }
    public long DownloadCount { get; init; }
    public bool Yanked { get; init; }
    public DateTime PublishedAt { get; init; }
    public required string PublishedBy { get; init; }
}

public sealed class PackageListDto
{
    public required string Owner { get; init; }
    public required string Name { get; init; }
    public required string FullName { get; init; }  // "owner/name"
    public string? LatestVersion { get; init; }
    public string? Description { get; init; }
    public long TotalDownloads { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<PackageVersionDto> Versions { get; init; } = [];
}

public sealed class PackageSearchResultDto
{
    public int Total { get; init; }
    public int Page { get; init; }
    public int PerPage { get; init; }
    public List<PackageListDto> Items { get; init; } = [];
}
