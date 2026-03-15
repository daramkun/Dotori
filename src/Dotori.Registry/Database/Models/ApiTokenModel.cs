using System.ComponentModel.DataAnnotations;

namespace Dotori.Registry.Database.Models;

public sealed class ApiTokenModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public UserModel User { get; set; } = null!;

    [MaxLength(128)]
    public required string Name { get; set; }

    // SHA-256(token) — 원본은 발급 시 1회만 표시
    [MaxLength(80)]
    public required string TokenHash { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUsed { get; set; }

    public DateTime? ExpiresAt { get; set; }
}
