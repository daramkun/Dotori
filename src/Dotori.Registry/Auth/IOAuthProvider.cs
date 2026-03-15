namespace Dotori.Registry.Auth;

public sealed class DeviceCodeResponse
{
    public required string DeviceCode { get; init; }
    public required string UserCode { get; init; }
    public required string VerificationUri { get; init; }
    public int ExpiresIn { get; init; }
    public int Interval { get; init; } = 5;
}

public sealed class OAuthUserInfo
{
    public required string ProviderId { get; init; }
    public required string Username { get; init; }
    public string? Email { get; init; }
}

public interface IOAuthProvider
{
    string Name { get; }

    Task<DeviceCodeResponse> StartDeviceFlowAsync(CancellationToken ct = default);

    /// <returns>null if still pending</returns>
    Task<OAuthUserInfo?> PollDeviceTokenAsync(string deviceCode, CancellationToken ct = default);
}
