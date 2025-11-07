using FreeStyleApp.Application.DTOs;
using FreeStyleApp.Application.Exceptions;
using FreeStyleApp.Application.Interfaces;
using FreeStyleApp.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace FreeStyleApp.Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IAppDbContext _context;
        private readonly ILogger<ProfileService> _logger;
        private readonly PasswordValidator _passwordValidator;

        public ProfileService(IAppDbContext context, ILogger<ProfileService> logger, PasswordValidator passwordValidator)
        {
            _context = context;
            _logger = logger;
            _passwordValidator = passwordValidator;
        }

        public async Task ChangePasswordAsync(string userId, ChangePasswordDto model, string actionUser)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogError("UserId không tồn tại: {UserId}", userId);
                throw new AppException("Không tìm thấy người dùng.");
            }

            if (HashPassword(model.OldPassword) != user.PasswordHash)
            {
                _context.AuditLogs.Add(new AuditLog { Timestamp = DateTime.UtcNow, UserName = actionUser, ActionType = "Đổi mật khẩu thất bại", Details = "Nhập sai mật khẩu cũ." });
                await _context.SaveChangesAsync();
                throw new AppException("Mật khẩu cũ không chính xác.");
            }

            // Validate mật khẩu mới theo settings
            var (isValid, errorMessage) = await _passwordValidator.ValidatePasswordAsync(model.NewPassword);
            if (!isValid)
            {
                throw new AppException(errorMessage);
            }

            user.PasswordHash = HashPassword(model.NewPassword);

            _context.AuditLogs.Add(new AuditLog { Timestamp = DateTime.UtcNow, UserName = actionUser, ActionType = "Đổi mật khẩu thành công", Details = "Người dùng đã tự đổi mật khẩu." });

            await _context.SaveChangesAsync();
            _logger.LogInformation("Người dùng '{UserName}' đã đổi mật khẩu thành công.", actionUser);
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