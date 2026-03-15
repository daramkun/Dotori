using Dotori.Registry.Database;
using Dotori.Registry.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Dotori.Tests.Registry;

[TestClass]
public sealed class DbContextTests
{
    private static RegistryDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<RegistryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new RegistryDbContext(options);
    }

    [TestMethod]
    public async Task CanCreateUser()
    {
        await using var db = CreateInMemoryDb();
        db.Users.Add(new UserModel
        {
            Username = "testuser",
            OAuthProvider = "github",
            OAuthId = "12345",
        });
        await db.SaveChangesAsync();

        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
        Assert.IsNotNull(user);
        Assert.AreEqual("github", user.OAuthProvider);
    }

    [TestMethod]
    public async Task CanCreatePackageWithVersion()
    {
        await using var db = CreateInMemoryDb();

        var user = new UserModel { Username = "alice", OAuthProvider = "github", OAuthId = "1" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var pkg = new PackageModel { Name = "mylib", OwnerId = user.Id };
        db.Packages.Add(pkg);
        await db.SaveChangesAsync();

        var ver = new PackageVersionModel
        {
            PackageId = pkg.Id,
            Version = "1.0.0",
            Hash = "sha256:abc",
            PublishedById = user.Id,
            Manifest = "package { name = \"mylib\" version = \"1.0.0\" }",
        };
        db.PackageVersions.Add(ver);
        await db.SaveChangesAsync();

        var loaded = await db.Packages
            .Include(p => p.Versions)
            .FirstOrDefaultAsync(p => p.Name == "mylib");
        Assert.IsNotNull(loaded);
        Assert.AreEqual(1, loaded.Versions.Count);
        Assert.AreEqual("1.0.0", loaded.Versions.First().Version);
    }

    [TestMethod]
    public async Task CanCreateCollaborator()
    {
        await using var db = CreateInMemoryDb();

        var owner = new UserModel { Username = "owner", OAuthProvider = "github", OAuthId = "2" };
        var collab = new UserModel { Username = "collab", OAuthProvider = "github", OAuthId = "3" };
        db.Users.AddRange(owner, collab);
        await db.SaveChangesAsync();

        var pkg = new PackageModel { Name = "shared-lib", OwnerId = owner.Id };
        db.Packages.Add(pkg);
        await db.SaveChangesAsync();

        db.PackageCollaborators.Add(new PackageCollaboratorModel
        {
            PackageId = pkg.Id,
            UserId = owner.Id,
            Role = "owner",
        });
        db.PackageCollaborators.Add(new PackageCollaboratorModel
        {
            PackageId = pkg.Id,
            UserId = collab.Id,
            Role = "collaborator",
        });
        await db.SaveChangesAsync();

        var collaborators = await db.PackageCollaborators
            .Include(c => c.User)
            .Where(c => c.PackageId == pkg.Id)
            .ToListAsync();

        Assert.AreEqual(2, collaborators.Count);
        Assert.IsTrue(collaborators.Any(c => c.Role == "owner"));
        Assert.IsTrue(collaborators.Any(c => c.Role == "collaborator"));
    }

    [TestMethod]
    public async Task CanCreateApiToken()
    {
        await using var db = CreateInMemoryDb();

        var user = new UserModel { Username = "dev", OAuthProvider = "github", OAuthId = "4" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var token = new ApiTokenModel
        {
            UserId = user.Id,
            Name = "ci-token",
            TokenHash = "sha256:deadbeef",
        };
        db.ApiTokens.Add(token);
        await db.SaveChangesAsync();

        var loaded = await db.ApiTokens.FirstOrDefaultAsync(t => t.Name == "ci-token");
        Assert.IsNotNull(loaded);
        Assert.AreEqual("sha256:deadbeef", loaded.TokenHash);
    }
}
