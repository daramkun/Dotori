using System.ComponentModel.DataAnnotations;

namespace Dotori.Registry.Database.Models;

public sealed class UserModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(64)]
    public required string Username { get; set; }

    [MaxLength(255)]
    public string? Email { get; set; }

    [MaxLength(32)]
    public required string OAuthProvider { get; set; }

    [MaxLength(255)]
    public required string OAuthId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PackageCollaboratorModel> Collaborations { get; set; } = [];
    public ICollection<ApiTokenModel> ApiTokens { get; set; } = [];
}
