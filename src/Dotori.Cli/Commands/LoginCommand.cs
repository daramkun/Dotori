using System.CommandLine;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dotori.PackageManager.Config;

namespace Dotori.Cli.Commands;

internal static class LoginCommandFactory
{
    public static Command Create()
    {
        var command = new Command("login", "Authenticate with a dotori package registry");

        var registryOption = new Option<string?>("--registry") { Description = "Registry URL (default: https://registry.dotori.dev)" };
        var providerOption = new Option<string?>("--provider") { Description = "OAuth provider (default: github)" };

        command.Add(registryOption);
        command.Add(providerOption);

        command.SetAction(async (parseResult, ct) =>
        {
            var registryUrl = parseResult.GetValue(registryOption)
                ?? DotoriConfigManager.Load().DefaultRegistry;
            var provider = parseResult.GetValue(providerOption) ?? "github";

            using var http = new HttpClient();
            http.DefaultRequestHeaders.UserAgent.ParseAdd("dotori/1.0");

            // 1. device/code 요청
            Console.WriteLine($"Connecting to {registryUrl}...");
            HttpResponseMessage codeResp;
            try
            {
                codeResp = await http.PostAsJsonAsync(
                    $"{registryUrl.TrimEnd('/')}/api/v1/auth/device/code",
                    new AnonymousDeviceCodeRequest { Provider = provider },
                    LoginJsonContext.Default.AnonymousDeviceCodeRequest,
                    ct);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"error: cannot reach registry: {ex.Message}");
                return;
            }

            if (!codeResp.IsSuccessStatusCode)
            {
                var body = await codeResp.Content.ReadAsStringAsync(ct);
                Console.Error.WriteLine($"error: {body}");
                return;
            }

            var codeData = await codeResp.Content.ReadFromJsonAsync(
                LoginJsonContext.Default.DeviceCodeDto, ct);
            if (codeData is null) { Console.Error.WriteLine("error: empty response"); return; }

            // 2. 사용자 안내
            Console.WriteLine();
            Console.WriteLine($"Open the following URL in your browser:");
            Console.WriteLine($"  {codeData.VerificationUri}");
            Console.WriteLine();
            Console.WriteLine($"Enter the code: {codeData.UserCode}");
            Console.WriteLine();
            Console.WriteLine("Waiting for authentication...");

            // 3. polling
            var interval = TimeSpan.FromSeconds(Math.Max(codeData.Interval, 5));
            var deadline = DateTime.UtcNow.AddSeconds(codeData.ExpiresIn);

            while (DateTime.UtcNow < deadline)
            {
                await Task.Delay(interval, ct);

                HttpResponseMessage tokenResp;
                try
                {
                    tokenResp = await http.PostAsJsonAsync(
                        $"{registryUrl.TrimEnd('/')}/api/v1/auth/device/token",
                        new AnonymousDeviceTokenRequest { DeviceCode = codeData.DeviceCode, Provider = provider },
                        LoginJsonContext.Default.AnonymousDeviceTokenRequest,
                        ct);
                }
                catch { continue; }

                if (tokenResp.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    continue; // authorization_pending

                if (!tokenResp.IsSuccessStatusCode)
                    continue;

                var tokenData = await tokenResp.Content.ReadFromJsonAsync(
                    LoginJsonContext.Default.TokenDto, ct);

                if (tokenData?.AccessToken is not null)
                {
                    DotoriConfigManager.SetToken(registryUrl, tokenData.AccessToken);
                    Console.WriteLine($"Authenticated successfully!");
                    Console.WriteLine($"Token saved to ~/.dotori/config.toml");
                    return;
                }
            }

            Console.Error.WriteLine("error: authentication timed out");
        });

        return command;
    }
}

// JSON DTOs for NativeAOT
internal sealed class DeviceCodeDto
{
    [JsonPropertyName("deviceCode")]       public string DeviceCode { get; init; } = "";
    [JsonPropertyName("userCode")]         public string UserCode { get; init; } = "";
    [JsonPropertyName("verificationUri")] public string VerificationUri { get; init; } = "";
    [JsonPropertyName("expiresIn")]        public int ExpiresIn { get; init; } = 900;
    [JsonPropertyName("interval")]         public int Interval { get; init; } = 5;
}

internal sealed class TokenDto
{
    [JsonPropertyName("accessToken")] public string? AccessToken { get; init; }
}

internal sealed class AnonymousDeviceCodeRequest
{
    [JsonPropertyName("provider")] public string Provider { get; init; } = "";
}

internal sealed class AnonymousDeviceTokenRequest
{
    [JsonPropertyName("deviceCode")] public string DeviceCode { get; init; } = "";
    [JsonPropertyName("provider")]   public string Provider { get; init; } = "";
}

[JsonSerializable(typeof(DeviceCodeDto))]
[JsonSerializable(typeof(TokenDto))]
[JsonSerializable(typeof(AnonymousDeviceCodeRequest))]
[JsonSerializable(typeof(AnonymousDeviceTokenRequest))]
internal partial class LoginJsonContext : JsonSerializerContext { }
