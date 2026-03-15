using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Dotori.Registry.Auth;

public sealed class GitHubOAuthProvider(IConfiguration config, IHttpClientFactory httpClientFactory) : IOAuthProvider
{
    public string Name => "github";

    private string ClientId => config["OAuth:Providers:github:ClientId"] ?? throw new InvalidOperationException("GitHub ClientId not configured");
    private string ClientSecret => config["OAuth:Providers:github:ClientSecret"] ?? throw new InvalidOperationException("GitHub ClientSecret not configured");

    public async Task<DeviceCodeResponse> StartDeviceFlowAsync(CancellationToken ct = default)
    {
        using var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Accept.ParseAdd("application/json");

        var resp = await client.PostAsync(
            $"https://github.com/login/device/code?client_id={ClientId}&scope=read:user,user:email",
            null, ct);

        resp.EnsureSuccessStatusCode();

        var body = await resp.Content.ReadFromJsonAsync(GitHubDeviceCodeResponseContext.Default.GitHubDeviceCodeResponse, ct)
            ?? throw new InvalidOperationException("Empty response from GitHub");

        return new DeviceCodeResponse
        {
            DeviceCode = body.DeviceCode,
            UserCode = body.UserCode,
            VerificationUri = body.VerificationUri,
            ExpiresIn = body.ExpiresIn,
            Interval = body.Interval,
        };
    }

    public async Task<OAuthUserInfo?> PollDeviceTokenAsync(string deviceCode, CancellationToken ct = default)
    {
        using var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Accept.ParseAdd("application/json");

        var resp = await client.PostAsync(
            $"https://github.com/login/oauth/access_token" +
            $"?client_id={ClientId}&client_secret={ClientSecret}" +
            $"&device_code={deviceCode}&grant_type=urn:ietf:params:oauth:grant-type:device_code",
            null, ct);

        resp.EnsureSuccessStatusCode();

        var body = await resp.Content.ReadFromJsonAsync(GitHubTokenResponseContext.Default.GitHubTokenResponse, ct)
            ?? throw new InvalidOperationException("Empty response from GitHub");

        if (body.Error is "authorization_pending" or "slow_down")
            return null;

        if (body.AccessToken is null)
            throw new InvalidOperationException($"GitHub OAuth error: {body.Error}");

        return await GetUserInfoInternalAsync(client, body.AccessToken, ct);
    }

    private static async Task<OAuthUserInfo> GetUserInfoInternalAsync(HttpClient client, string accessToken, CancellationToken ct)
    {
        client.DefaultRequestHeaders.Authorization = new("Bearer", accessToken);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("dotori-registry/1.0");

        var user = await client.GetFromJsonAsync(
            "https://api.github.com/user",
            GitHubUserContext.Default.GitHubUser, ct)
            ?? throw new InvalidOperationException("Empty user response from GitHub");

        string? email = user.Email;
        if (email is null)
        {
            var emails = await client.GetFromJsonAsync(
                "https://api.github.com/user/emails",
                GitHubEmailListContext.Default.ListGitHubEmail, ct);
            email = emails?.FirstOrDefault(e => e.Primary && e.Verified)?.Email;
        }

        return new OAuthUserInfo
        {
            ProviderId = user.Id.ToString(),
            Username = user.Login,
            Email = email,
        };
    }
}

// JSON DTOs (source-generation for NativeAOT compat on server side, though server doesn't need NativeAOT)
internal sealed class GitHubDeviceCodeResponse
{
    [JsonPropertyName("device_code")]  public string DeviceCode { get; init; } = "";
    [JsonPropertyName("user_code")]    public string UserCode { get; init; } = "";
    [JsonPropertyName("verification_uri")] public string VerificationUri { get; init; } = "";
    [JsonPropertyName("expires_in")]   public int ExpiresIn { get; init; }
    [JsonPropertyName("interval")]     public int Interval { get; init; }
}

internal sealed class GitHubTokenResponse
{
    [JsonPropertyName("access_token")] public string? AccessToken { get; init; }
    [JsonPropertyName("error")]        public string? Error { get; init; }
}

internal sealed class GitHubUser
{
    [JsonPropertyName("id")]    public long Id { get; init; }
    [JsonPropertyName("login")] public string Login { get; init; } = "";
    [JsonPropertyName("email")] public string? Email { get; init; }
}

internal sealed class GitHubEmail
{
    [JsonPropertyName("email")]    public string Email { get; init; } = "";
    [JsonPropertyName("primary")]  public bool Primary { get; init; }
    [JsonPropertyName("verified")] public bool Verified { get; init; }
}

[JsonSerializable(typeof(GitHubDeviceCodeResponse))]
internal partial class GitHubDeviceCodeResponseContext : JsonSerializerContext { }

[JsonSerializable(typeof(GitHubTokenResponse))]
internal partial class GitHubTokenResponseContext : JsonSerializerContext { }

[JsonSerializable(typeof(GitHubUser))]
internal partial class GitHubUserContext : JsonSerializerContext { }

[JsonSerializable(typeof(List<GitHubEmail>))]
internal partial class GitHubEmailListContext : JsonSerializerContext { }
