using FreeStyleApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FreeStyleApp.Application.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<User> Users { get; }
        DbSet<Permission> Permissions { get; }
        DbSet<UserPermission> UserPermissions { get; }
        DbSet<AuditLog> AuditLogs { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}