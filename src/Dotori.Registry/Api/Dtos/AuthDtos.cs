namespace Dotori.Registry.Api.Dtos;

public sealed class DeviceCodeRequestDto
{
    public required string Provider { get; init; }
}

public sealed class DeviceCodeResponseDto
{
    public required string DeviceCode { get; init; }
    public required string UserCode { get; init; }
    public required string VerificationUri { get; init; }
    public int ExpiresIn { get; init; }
    public int Interval { get; init; }
}

public sealed class DeviceTokenRequestDto
{
    public required string DeviceCode { get; init; }
    public required string Provider { get; init; }
}

public sealed class TokenResponseDto
{
    public required string AccessToken { get; init; }
    public string TokenType { get; init; } = "Bearer";
}

public sealed class CreateApiTokenRequestDto
{
    public required string Name { get; init; }
    public int? ExpiryDays { get; init; }
}

public class ApiTokenDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public DateTime? LastUsed { get; init; }
}

public sealed class CreatedApiTokenDto : ApiTokenDto
{
    public required string Token { get; init; }  // 발급 시 1회만 반환
}

public sealed class UserDto
{
    public required Guid Id { get; init; }
    public required string Username { get; init; }
    public string? Email { get; init; }
    public DateTime CreatedAt { get; init; }
}
