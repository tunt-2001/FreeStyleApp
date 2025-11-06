using FreeStyleApp.Application.DTOs;
using FreeStyleApp.Application.Interfaces;
using FreeStyleApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace FreeStyleApp.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAppDbContext _context;

        public AccountService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<User> ValidateUserAsync(LoginRequest model)
        {
            var user = await _context.Users
                .Include(u => u.UserPermissions)
                    .ThenInclude(up => up.Permission)
                .FirstOrDefaultAsync(u => u.UserName == model.Username);

            if (user == null || HashPassword(model.Password) != user.PasswordHash)
            {
                throw new AppException("Tên đăng nhập hoặc mật khẩu không chính xác.");
            }

            return user;
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