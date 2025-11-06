using FreeStyleApp.Application.Interfaces;
using FreeStyleApp.Domain.Entities;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .Property(u => u.Id)
                .HasConversion(v => Guid.Parse(v), v => v.ToString());

            modelBuilder.Entity<Permission>()
                .Property(p => p.Id)
                .HasConversion(v => Guid.Parse(v), v => v.ToString());

            modelBuilder.Entity<UserPermission>(entity =>
            {
                entity.HasKey(up => new { up.UserId, up.PermissionId });

                entity.Property(up => up.UserId)
                      .HasConversion(v => Guid.Parse(v), v => v.ToString());

                entity.Property(up => up.PermissionId)
                      .HasConversion(v => Guid.Parse(v), v => v.ToString());

                entity.HasOne(up => up.User)
                      .WithMany(u => u.UserPermissions)
                      .HasForeignKey(up => up.UserId);

                entity.HasOne(up => up.Permission)
                      .WithMany()
                      .HasForeignKey(up => up.PermissionId);
            });
        }
    }
}
