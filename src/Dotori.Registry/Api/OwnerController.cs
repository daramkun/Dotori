using System.Security.Claims;
using Dotori.Registry.Api.Dtos;
using Dotori.Registry.Database;
using Dotori.Registry.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dotori.Registry.Api;

[ApiController]
[Route("api/v1/packages/{owner}/{name}/owners")]
[Authorize]
public sealed class OwnerController(RegistryDbContext db) : ControllerBase
{
    // GET /api/v1/packages/{owner}/{name}/owners
    [HttpGet]
    public async Task<IActionResult> List(string owner, string name, CancellationToken ct)
    {
        var pkg = await FindPackageAsync(owner, name, ct);
        if (pkg is null) return NotFound();

        var collaborators = await db.PackageCollaborators
            .Include(c => c.User)
            .Where(c => c.PackageId == pkg.Id)
            .ToListAsync(ct);

        return Ok(collaborators.Select(c => new CollaboratorDto
        {
            Username = c.User.Username,
            Role = c.Role,
            AddedAt = c.AddedAt,
        }));
    }

    // POST /api/v1/packages/{owner}/{name}/owners
    [HttpPost]
    public async Task<IActionResult> Add(string owner, string name, [FromBody] AddCollaboratorRequestDto req, CancellationToken ct)
    {
        var pkg = await FindPackageAsync(owner, name, ct);
        if (pkg is null) return NotFound();

        if (!await IsOwnerAsync(pkg.Id, ct)) return Forbid();

        var role = req.Role is "owner" or "collaborator" ? req.Role : "collaborator";
        var targetUser = await db.Users.FirstOrDefaultAsync(
            u => u.Username == req.Username.ToLowerInvariant(), ct);

        if (targetUser is null)
            return NotFound(new { error = $"User '{req.Username}' not found" });

        var existing = await db.PackageCollaborators.FindAsync(
            [pkg.Id, targetUser.Id], ct);
        if (existing is not null)
        {
            existing.Role = role;
        }
        else
        {
            db.PackageCollaborators.Add(new PackageCollaboratorModel
            {
                PackageId = pkg.Id,
                UserId = targetUser.Id,
                Role = role,
            });
        }
        await db.SaveChangesAsync(ct);
        return Ok();
    }

    // DELETE /api/v1/packages/{owner}/{name}/owners/{username}
    [HttpDelete("{username}")]
    public async Task<IActionResult> Remove(string owner, string name, string username, CancellationToken ct)
    {
        var pkg = await FindPackageAsync(owner, name, ct);
        if (pkg is null) return NotFound();

        if (!await IsOwnerAsync(pkg.Id, ct)) return Forbid();

        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var targetUser = await db.Users.FirstOrDefaultAsync(
            u => u.Username == username.ToLowerInvariant(), ct);

        if (targetUser is null) return NotFound();

        // owner 자신은 제거 불가
        if (targetUser.Id == currentUserId)
            return BadRequest(new { error = "Cannot remove yourself as owner" });

        var collab = await db.PackageCollaborators.FindAsync([pkg.Id, targetUser.Id], ct);
        if (collab is null) return NotFound();

        db.PackageCollaborators.Remove(collab);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // POST /api/v1/packages/{owner}/{name}/owners/transfer
    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer(string owner, string name, [FromBody] TransferOwnershipRequestDto req, CancellationToken ct)
    {
        var pkg = await FindPackageAsync(owner, name, ct);
        if (pkg is null) return NotFound();

        if (!await IsOwnerAsync(pkg.Id, ct)) return Forbid();

        var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var newOwner = await db.Users.FirstOrDefaultAsync(
            u => u.Username == req.NewOwner.ToLowerInvariant(), ct);

        if (newOwner is null)
            return NotFound(new { error = $"User '{req.NewOwner}' not found" });

        // 현재 owner → collaborator 강등
        var currentCollab = await db.PackageCollaborators.FindAsync([pkg.Id, currentUserId], ct);
        if (currentCollab is not null)
            currentCollab.Role = "collaborator";

        // 새 owner 설정 또는 생성
        var newCollab = await db.PackageCollaborators.FindAsync([pkg.Id, newOwner.Id], ct);
        if (newCollab is not null)
        {
            newCollab.Role = "owner";
        }
        else
        {
            db.PackageCollaborators.Add(new PackageCollaboratorModel
            {
                PackageId = pkg.Id,
                UserId = newOwner.Id,
                Role = "owner",
            });
        }

        // PackageModel.OwnerId 업데이트
        pkg.OwnerId = newOwner.Id;
        await db.SaveChangesAsync(ct);

        return Ok(new { newOwner = newOwner.Username });
    }

    private async Task<PackageModel?> FindPackageAsync(string owner, string name, CancellationToken ct) =>
        await db.Packages
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p =>
                p.Owner.Username == owner.ToLowerInvariant() &&
                p.Name == name.ToLowerInvariant(), ct);

    private async Task<bool> IsOwnerAsync(Guid packageId, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await db.PackageCollaborators.AnyAsync(
            c => c.PackageId == packageId && c.UserId == userId && c.Role == "owner", ct);
    }
}
