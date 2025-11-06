using FreeStyleApp.Domain.Entities;

namespace FreeStyleApp.Application.Interfaces
{
    public interface IPermissionService
    {
        Task<Permission> SavePermissionAsync(Permission model, string actionUser);
        Task DeletePermissionAsync(string id, string actionUser);
    }
}