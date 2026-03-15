using System.ComponentModel.DataAnnotations;

namespace Dotori.Registry.Database.Models;

public sealed class DeviceCodeModel
{
    // device_code (서버↔클라이언트 polling용)
    [MaxLength(64)]
    public required string Code { get; set; }

    // user_code (사용자가 브라우저에서 입력하는 8자리 코드)
    [MaxLength(16)]
    public required string UserCode { get; set; }

    // OAuth provider name: "github", etc.
    [MaxLength(32)]
    public required string Provider { get; set; }

    // 인증 완료 후 채워짐
    public Guid? UserId { get; set; }
    public UserModel? User { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool Verified { get; set; }
}
