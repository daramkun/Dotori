using System.Security.Claims;
using Dotori.Registry.Api.Dtos;
using Dotori.Registry.Auth;
using Dotori.Registry.Database;
using Dotori.Registry.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dotori.Registry.Api;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(
    RegistryDbContext db,
    OAuthProviderRegistry providers,
    TokenService tokenService,
    IConfiguration config) : ControllerBase
{
    // POST /api/v1/auth/device/code
    [HttpPost("device/code")]
    public async Task<IActionResult> StartDeviceFlow([FromBody] DeviceCodeRequestDto req, CancellationToken ct)
    {
        var provider = providers.Get(req.Provider);
        if (provider is null)
            return BadRequest(new { error = $"OAuth provider '{req.Provider}' is not enabled" });

        var response = await provider.StartDeviceFlowAsync(ct);

        // DB에 device code 저장
        var expiryMinutes = int.TryParse(config["OAuth:DeviceCodeExpiryMinutes"], out var m) ? m : 15;
        db.DeviceCodes.Add(new DeviceCodeModel
        {
            Code = response.DeviceCode,
            UserCode = response.UserCode,
            Provider = req.Provider,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
        });
        await db.SaveChangesAsync(ct);

        return Ok(new DeviceCodeResponseDto
        {
            DeviceCode = response.DeviceCode,
            UserCode = response.UserCode,
            VerificationUri = response.VerificationUri,
            ExpiresIn = response.ExpiresIn,
            Interval = response.Interval,
        });
    }

    // POST /api/v1/auth/device/token
    [HttpPost("device/token")]
    public async Task<IActionResult> PollDeviceToken([FromBody] DeviceTokenRequestDto req, CancellationToken ct)
    {
        var deviceCode = await db.DeviceCodes
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Code == req.DeviceCode && d.Provider == req.Provider, ct);

        if (deviceCode is null)
            return BadRequest(new { error = "Invalid device code" });

        if (deviceCode.ExpiresAt < DateTime.UtcNow)
            return BadRequest(new { error = "Device code expired" });

        // 이미 verified 된 코드이면 즉시 토큰 발급
        if (deviceCode.Verified && deviceCode.UserId.HasValue)
        {
            var user = deviceCode.User ?? await db.Users.FindAsync([deviceCode.UserId.Value], ct)!;
            db.DeviceCodes.Remove(deviceCode);
            await db.SaveChangesAsync(ct);

            return Ok(new TokenResponseDto
            {
                AccessToken = tokenService.CreateJwt(user!.Id, user.Username),
            });
        }

        // GitHub에 polling
        var provider = providers.Get(req.Provider);
        if (provider is null)
            return BadRequest(new { error = $"OAuth provider '{req.Provider}' is not enabled" });

        OAuthUserInfo? userInfo;
        try
        {
            userInfo = await provider.PollDeviceTokenAsync(req.DeviceCode, ct);
        }
        catch (Exception)
        {
            return BadRequest(new { error = "OAuth provider error" });
        }

        if (userInfo is null)
            return StatusCode(429, new { error = "authorization_pending" });

        // 사용자 생성 또는 조회
        var dbUser = await db.Users.FirstOrDefaultAsync(
            u => u.OAuthProvider == req.Provider && u.OAuthId == userInfo.ProviderId, ct);

        if (dbUser is null)
        {
            dbUser = new UserModel
            {
                Username = await ResolveUniqueUsernameAsync(userInfo.Username, ct),
                Email = userInfo.Email,
                OAuthProvider = req.Provider,
                OAuthId = userInfo.ProviderId,
            };
            db.Users.Add(dbUser);
        }
        else
        {
            dbUser.Email = userInfo.Email;
        }

        deviceCode.Verified = true;
        deviceCode.UserId = dbUser.Id;
        await db.SaveChangesAsync(ct);
        db.DeviceCodes.Remove(deviceCode);
        await db.SaveChangesAsync(ct);

        return Ok(new TokenResponseDto
        {
            AccessToken = tokenService.CreateJwt(dbUser.Id, dbUser.Username),
        });
    }

    // POST /api/v1/auth/token  — 새 API 토큰 발급
    [HttpPost("token")]
    [Authorize]
    public async Task<IActionResult> CreateApiToken([FromBody] CreateApiTokenRequestDto req, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var rawToken = TokenService.GenerateApiToken();
        var hash = TokenService.HashToken(rawToken);

        var model = new ApiTokenModel
        {
            UserId = userId,
            Name = req.Name,
            TokenHash = hash,
            ExpiresAt = req.ExpiryDays.HasValue
                ? DateTime.UtcNow.AddDays(req.ExpiryDays.Value)
                : null,
        };
        db.ApiTokens.Add(model);
        await db.SaveChangesAsync(ct);

        return Ok(new CreatedApiTokenDto
        {
            Id = model.Id,
            Name = model.Name,
            Token = rawToken,
            CreatedAt = model.CreatedAt,
            ExpiresAt = model.ExpiresAt,
            LastUsed = model.LastUsed,
        });
    }

    // GET /api/v1/auth/token
    [HttpGet("token")]
    [Authorize]
    public async Task<IActionResult> ListApiTokens(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var tokens = await db.ApiTokens
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new ApiTokenDto
            {
                Id = t.Id,
                Name = t.Name,
                CreatedAt = t.CreatedAt,
                ExpiresAt = t.ExpiresAt,
                LastUsed = t.LastUsed,
            })
            .ToListAsync(ct);

        return Ok(tokens);
    }

    // DELETE /api/v1/auth/token/{id}
    [HttpDelete("token/{id:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteApiToken(Guid id, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var token = await db.ApiTokens.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct);

        if (token is null)
            return NotFound();

        db.ApiTokens.Remove(token);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // GET /api/v1/auth/me
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await db.Users.FindAsync([userId], ct);
        if (user is null) return Unauthorized();

        return Ok(new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
        });
    }

    private async Task<string> ResolveUniqueUsernameAsync(string preferred, CancellationToken ct)
    {
        var name = preferred.ToLowerInvariant();
        if (!await db.Users.AnyAsync(u => u.Username == name, ct))
            return name;

        for (int i = 2; i < 1000; i++)
        {
            var candidate = $"{name}{i}";
            if (!await db.Users.AnyAsync(u => u.Username == candidate, ct))
                return candidate;
        }
        return $"{name}_{Guid.NewGuid():N}"[..64];
    }
}
