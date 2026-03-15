using System.Security.Claims;
using Dotori.Registry.Database;
using Microsoft.EntityFrameworkCore;

namespace Dotori.Registry.Auth;

/// <summary>
/// "Authorization: ApiToken dt_live_..." 헤더를 처리하여 사용자를 주입합니다.
/// JWT Bearer는 ASP.NET Core 내장 미들웨어가 처리합니다.
/// </summary>
public sealed class ApiKeyMiddleware(RequestDelegate next)
{
    private const string ApiTokenPrefix = "ApiToken ";

    public async Task InvokeAsync(HttpContext context, RegistryDbContext db)
    {
        var auth = context.Request.Headers.Authorization.ToString();
        if (auth.StartsWith(ApiTokenPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var rawToken = auth[ApiTokenPrefix.Length..].Trim();
            var hash = TokenService.HashToken(rawToken);

            var token = await db.ApiTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TokenHash == hash);

            if (token is not null && (token.ExpiresAt is null || token.ExpiresAt > DateTime.UtcNow))
            {
                // LastUsed 업데이트 (fire-and-forget 스타일 — 실패해도 인증은 성공)
                token.LastUsed = DateTime.UtcNow;
                await db.SaveChangesAsync();

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, token.UserId.ToString()),
                    new Claim(ClaimTypes.Name, token.User.Username),
                };
                context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "ApiToken"));
            }
        }

        await next(context);
    }
}
