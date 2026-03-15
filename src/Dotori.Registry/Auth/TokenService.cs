using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Dotori.Registry.Auth;

public sealed class TokenService(IConfiguration config)
{
    private string JwtSecret => config["OAuth:Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret not configured");
    private int ExpiryDays => int.TryParse(config["OAuth:Jwt:ExpiryDays"], out var d) ? d : 365;
    private string Issuer => config["OAuth:Jwt:Issuer"] ?? "dotori-registry";
    private string Audience => config["OAuth:Jwt:Audience"] ?? "dotori-client";

    public string CreateJwt(Guid userId, string username)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(ExpiryDays),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public TokenValidationParameters GetValidationParameters()
    {
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = Issuer,
            ValidateAudience = true,
            ValidAudience = Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret)),
            ClockSkew = TimeSpan.FromMinutes(5),
        };
    }

    // API 토큰: "dt_live_{random}" 형식 — SHA-256 해시를 DB에 저장
    public static string GenerateApiToken()
    {
        var random = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
        return $"dt_live_{random}";
    }

    public static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return $"sha256:{Convert.ToHexString(bytes).ToLowerInvariant()}";
    }
}
