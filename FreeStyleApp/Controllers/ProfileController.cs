using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FreeStyleApp.Data;
using System.Security.Claims;
using FreeStyleApp.Models;
using System.Security.Cryptography;
using System.Text;
using FreeStyleApp.DTOs;

namespace FreeStyleApp.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly AppDbContext _context;
        public ProfileController(AppDbContext context) { _context = context; }

        public IActionResult Index() => View();

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại." });
            }

            var userId = User.FindFirstValue("UserId");
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return NotFound();

            if (HashPassword(model.OldPassword) != user.PasswordHash)
            {
                return Json(new { success = false, message = "Mật khẩu cũ không chính xác." });
            }

            user.PasswordHash = HashPassword(model.NewPassword);
            await _context.SaveChangesAsync();

            var log = new AuditLog { Timestamp = DateTime.UtcNow, UserName = user.UserName, ActionType = "Đổi mật khẩu", Details = "Người dùng đã tự đổi mật khẩu thành công." };
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đổi mật khẩu thành công." });
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