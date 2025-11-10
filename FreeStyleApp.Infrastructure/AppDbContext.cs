using FreeStyleApp.Application.Interfaces;
using FreeStyleApp.Domain.Entities;
using FreeStyleApp.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
    
namespace FreeStyleApp.Infrastructure
{
    public class AppDbContext : DbContext, IAppDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<FeatureGroup> FeatureGroups { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<UserFeatureGroup> UserFeatureGroups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyGuidToStringConversion();

            // Cấu hình UserPermission
            modelBuilder.Entity<UserPermission>(entity =>
            {
                entity.HasKey(up => new { up.UserId, up.PermissionId });

                entity.HasOne(up => up.User)
                      .WithMany(u => u.UserPermissions)
                      .HasForeignKey(up => up.UserId);

                entity.HasOne(up => up.Permission)
                      .WithMany()
                      .HasForeignKey(up => up.PermissionId);
            });

            // Cấu hình UserFeatureGroup
            modelBuilder.Entity<UserFeatureGroup>(entity =>
            {
                entity.HasKey(ufg => new { ufg.UserId, ufg.FeatureGroupId });

                entity.HasOne(ufg => ufg.User)
                      .WithMany()
                      .HasForeignKey(ufg => ufg.UserId);

                entity.HasOne(ufg => ufg.FeatureGroup)
                      .WithMany(fg => fg.UserFeatureGroups)
                      .HasForeignKey(ufg => ufg.FeatureGroupId);
            });
        }
    }
}
