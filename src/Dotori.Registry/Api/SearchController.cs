using Dotori.Registry.Api.Dtos;
using Dotori.Registry.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dotori.Registry.Api;

[ApiController]
[Route("api/v1/packages")]
public sealed class SearchController(RegistryDbContext db) : ControllerBase
{
    // GET /api/v1/packages/search?q=fmt&page=1&per_page=20
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery(Name = "per_page")] int perPage = 20,
        CancellationToken ct = default)
    {
        perPage = Math.Clamp(perPage, 1, 100);
        page = Math.Max(1, page);

        var query = db.Packages
            .Include(p => p.Owner)
            .Include(p => p.Versions)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var qNorm = q.ToLowerInvariant();
            query = query.Where(p =>
                p.Name.Contains(qNorm) ||
                p.Owner.Username.Contains(qNorm) ||
                (p.Description != null && p.Description.Contains(qNorm)));
        }

        var total = await query.CountAsync(ct);

        var packages = await query
            .OrderByDescending(p => p.Versions.Sum(v => v.DownloadCount))
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync(ct);

        return Ok(new PackageSearchResultDto
        {
            Total = total,
            Page = page,
            PerPage = perPage,
            Items = packages.Select(p => new PackageListDto
            {
                Owner = p.Owner.Username,
                Name = p.Name,
                FullName = $"{p.Owner.Username}/{p.Name}",
                LatestVersion = p.Versions.OrderByDescending(v => v.PublishedAt).FirstOrDefault(v => !v.Yanked)?.Version,
                Description = p.Description,
                TotalDownloads = p.Versions.Sum(v => v.DownloadCount),
                CreatedAt = p.CreatedAt,
            }).ToList(),
        });
    }
}
