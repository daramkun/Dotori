using System.ComponentModel.DataAnnotations;

namespace Dotori.Registry.Database.Models;

public sealed class PackageModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // 소문자 정규화된 패키지명 (owner 제외, 순수 name)
    [MaxLength(64)]
    public required string Name { get; set; }

    public Guid OwnerId { get; set; }
    public UserModel Owner { get; set; } = null!;

    [MaxLength(512)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PackageVersionModel> Versions { get; set; } = [];
    public ICollection<PackageCollaboratorModel> Collaborators { get; set; } = [];
}
