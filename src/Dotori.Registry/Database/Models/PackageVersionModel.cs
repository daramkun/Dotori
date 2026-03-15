using System.ComponentModel.DataAnnotations;

namespace Dotori.Registry.Database.Models;

public sealed class PackageVersionModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PackageId { get; set; }
    public PackageModel Package { get; set; } = null!;

    [MaxLength(64)]
    public required string Version { get; set; }

    public bool Yanked { get; set; }

    // "sha256:abcdef..."
    [MaxLength(80)]
    public required string Hash { get; set; }

    public long ArchiveSize { get; set; }

    public long DownloadCount { get; set; }

    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;

    public Guid PublishedById { get; set; }
    public UserModel PublishedBy { get; set; } = null!;

    // .dotori 파일 원본 텍스트
    public required string Manifest { get; set; }
}
