using Dotori.Registry.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Dotori.Registry.Database;

public sealed class RegistryDbContext(DbContextOptions<RegistryDbContext> options) : DbContext(options)
{
    public DbSet<UserModel> Users => Set<UserModel>();
    public DbSet<PackageModel> Packages => Set<PackageModel>();
    public DbSet<PackageVersionModel> PackageVersions => Set<PackageVersionModel>();
    public DbSet<PackageCollaboratorModel> PackageCollaborators => Set<PackageCollaboratorModel>();
    public DbSet<ApiTokenModel> ApiTokens => Set<ApiTokenModel>();
    public DbSet<DeviceCodeModel> DeviceCodes => Set<DeviceCodeModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserModel>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Username).IsUnique();
            e.HasIndex(u => new { u.OAuthProvider, u.OAuthId }).IsUnique();
        });

        modelBuilder.Entity<PackageModel>(e =>
        {
            e.HasKey(p => p.Id);
            // (owner_id, name) 조합이 유일
            e.HasIndex(p => new { p.OwnerId, p.Name }).IsUnique();
            e.HasOne(p => p.Owner)
             .WithMany()
             .HasForeignKey(p => p.OwnerId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PackageVersionModel>(e =>
        {
            e.HasKey(v => v.Id);
            e.HasIndex(v => new { v.PackageId, v.Version }).IsUnique();
            e.HasOne(v => v.Package)
             .WithMany(p => p.Versions)
             .HasForeignKey(v => v.PackageId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(v => v.PublishedBy)
             .WithMany()
             .HasForeignKey(v => v.PublishedById)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PackageCollaboratorModel>(e =>
        {
            e.HasKey(c => new { c.PackageId, c.UserId });
            e.HasOne(c => c.Package)
             .WithMany(p => p.Collaborators)
             .HasForeignKey(c => c.PackageId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(c => c.User)
             .WithMany(u => u.Collaborations)
             .HasForeignKey(c => c.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApiTokenModel>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasIndex(t => t.TokenHash).IsUnique();
            e.HasOne(t => t.User)
             .WithMany(u => u.ApiTokens)
             .HasForeignKey(t => t.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DeviceCodeModel>(e =>
        {
            e.HasKey(d => d.Code);
            e.HasIndex(d => d.UserCode).IsUnique();
            e.HasOne(d => d.User)
             .WithMany()
             .HasForeignKey(d => d.UserId)
             .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
