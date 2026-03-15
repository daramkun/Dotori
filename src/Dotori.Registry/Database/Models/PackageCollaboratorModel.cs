using System.ComponentModel.DataAnnotations;

namespace Dotori.Registry.Database.Models;

public sealed class PackageCollaboratorModel
{
    public Guid PackageId { get; set; }
    public PackageModel Package { get; set; } = null!;

    public Guid UserId { get; set; }
    public UserModel User { get; set; } = null!;

    // "owner" | "collaborator"
    [MaxLength(16)]
    public required string Role { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
