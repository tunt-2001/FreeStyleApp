using FreeStyleApp.Application.DTOs;
using FreeStyleApp.Application.Interfaces;
using FreeStyleApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using FreeStyleApp.Application.Exceptions;

namespace FreeStyleApp.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IAppDbContext _context;
        private readonly ILogger<UserService> _logger;
        private readonly PasswordValidator _passwordValidator;
        private readonly IEmailService _emailService;

        public UserService(IAppDbContext context, ILogger<UserService> logger, PasswordValidator passwordValidator, IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _passwordValidator = passwordValidator;
            _emailService = emailService;
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
                // Validate mật khẩu khi tạo mới
                if (!string.IsNullOrEmpty(model.Password))
                {
                    var (isValid, errorMessage) = await _passwordValidator.ValidatePasswordAsync(model.Password);
                    if (!isValid)
                    {
                        throw new AppException(errorMessage);
                    }
                }

                userInDb = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = model.UserName,
                    FullName = model.FullName,
                    Email = model.Email,
                    IsActive = model.IsActive,
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
                userInDb.Email = model.Email;
                userInDb.IsActive = model.IsActive;
                if (!string.IsNullOrEmpty(model.Password))
                {
                    // Validate mật khẩu khi cập nhật
                    var (isValid, errorMessage) = await _passwordValidator.ValidatePasswordAsync(model.Password);
                    if (!isValid)
                    {
                        throw new AppException(errorMessage);
                    }
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

        public async Task<string> GenerateAndSendPasswordAsync(string userId, string actionUser)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new AppException("Không tìm thấy người dùng.");
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new AppException("Người dùng chưa có email. Vui lòng cập nhật email trước khi gửi mật khẩu.");
            }

            // Tạo mật khẩu ngẫu nhiên (8 ký tự: chữ hoa, chữ thường, số)
            var random = new Random();
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            var newPassword = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            // Cập nhật mật khẩu mới
            user.PasswordHash = HashPassword(newPassword);
            await _context.SaveChangesAsync();

            // Gửi email
            try
            {
                await _emailService.SendPasswordEmailAsync(user.Email, user.UserName, newPassword);
                _logger.LogInformation("Đã tạo và gửi mật khẩu mới cho người dùng '{UserName}' bởi '{ActionUser}'.", user.UserName, actionUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email mật khẩu cho người dùng '{UserName}'.", user.UserName);
                throw new AppException($"Đã tạo mật khẩu mới nhưng không thể gửi email: {ex.Message}");
            }

            return newPassword;
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