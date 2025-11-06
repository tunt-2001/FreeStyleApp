using FreeStyleApp.Application.DTOs;
using FreeStyleApp.Application.Interfaces;
using FreeStyleApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace FreeStyleApp.Application.Services
{
    public class AppException : Exception
    {
        public AppException(string message) : base(message) { }
    }

    public class UserService : IUserService
    {
        private readonly IAppDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(IAppDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User> SaveUserAsync(UserDto model, string actionUser)
        {
            if (string.IsNullOrEmpty(model.Id))
            {
                if (await _context.Users.AnyAsync(u => u.UserName.Equals(model.UserName)))
                {
                    throw new AppException($"Tên đăng nhập '{model.UserName}' đã tồn tại.");
                }
            }

            User userInDb;
            if (string.IsNullOrEmpty(model.Id)) // Tạo mới
            {
                userInDb = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = model.UserName,
                    FullName = model.FullName,
                    PasswordHash = HashPassword(model.Password)
                };
                _context.Users.Add(userInDb);
            }
            else // Cập nhật
            {
                userInDb = await _context.Users.FindAsync(model.Id);
                if (userInDb == null)
                {
                    throw new AppException("Không tìm thấy người dùng để cập nhật.");
                }

                userInDb.FullName = model.FullName;
                if (!string.IsNullOrEmpty(model.Password))
                {
                    userInDb.PasswordHash = HashPassword(model.Password);
                }
            }

            var oldPermissions = await _context.UserPermissions.Where(p => p.UserId == userInDb.Id).ToListAsync();
            _context.UserPermissions.RemoveRange(oldPermissions);

            if (model.Permissions != null && model.Permissions.Any())
            {
                var permissionIdsToAssign = await _context.Permissions
                    .Where(p => model.Permissions.Contains(p.Code))
                    .Select(p => p.Id)
                    .ToListAsync();

                var newUserPermissions = permissionIdsToAssign.Select(pId => new UserPermission { UserId = userInDb.Id, PermissionId = pId });
                _context.UserPermissions.AddRange(newUserPermissions);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Lưu thành công người dùng '{UserName}'.", userInDb.UserName);
            return userInDb;
        }

        public async Task DeleteUserAsync(string id, string actionUser)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new AppException("Không tìm thấy người dùng để xóa.");
            }
            if (user.UserName == "admin")
            {
                throw new AppException("Không thể xóa tài khoản gốc 'admin'.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Xóa thành công người dùng '{UserName}'.", user.UserName);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var builder = new StringBuilder();
            foreach (var b in bytes) { builder.Append(b.ToString("x2")); }
            return builder.ToString().ToUpper();
        }
    }
}