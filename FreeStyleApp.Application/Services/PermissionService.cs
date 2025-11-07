using FreeStyleApp.Application.Exceptions;
using FreeStyleApp.Application.Interfaces;
using FreeStyleApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreeStyleApp.Application.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IAppDbContext _context;
        private readonly ILogger<PermissionService> _logger;

        public PermissionService(IAppDbContext context, ILogger<PermissionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Permission> SavePermissionAsync(Permission model, string actionUser)
        {
            if (string.IsNullOrEmpty(model.Id)) // Thêm mới
            {
                if (await _context.Permissions.AnyAsync(p => p.Code == model.Code))
                {
                    throw new AppException($"Mã vai trò '{model.Code}' đã tồn tại.");
                }
                model.Id = Guid.NewGuid().ToString();
                _context.Permissions.Add(model);
                _logger.LogInformation("Người dùng '{ActionUser}' đã tạo vai trò mới '{PermissionCode}'.", actionUser, model.Code);
            }
            else // Cập nhật
            {
                var existing = await _context.Permissions.FindAsync(model.Id);
                if (existing == null)
                {
                    throw new AppException("Không tìm thấy vai trò để cập nhật.");
                }

                existing.Name = model.Name;
                _logger.LogInformation("Người dùng '{ActionUser}' đã cập nhật tên của vai trò '{PermissionCode}'.", actionUser, existing.Code);
            }

            await _context.SaveChangesAsync();
            return model;
        }

        public async Task DeletePermissionAsync(string id, string actionUser)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null)
            {
                throw new AppException("Không tìm thấy vai trò để xóa.");
            }

            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Người dùng '{ActionUser}' đã xóa vai trò '{PermissionCode}'.", actionUser, permission.Code);
        }
    }
}