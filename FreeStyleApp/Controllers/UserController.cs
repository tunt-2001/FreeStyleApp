using FreeStyleApp.Data;
using FreeStyleApp.DTOs;
using FreeStyleApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace FreeStyleApp.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        public UserController(AppDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
        {
            if (!UserHasPermission("Admin")) return Forbid();

            ViewBag.AllPermissions = await _context.Permissions.ToListAsync();
            var users = await _context.Users.ToListAsync();
            return View(users);
        }


        [HttpGet]
        public async Task<IActionResult> GetUser(string id) 
        {
            if (!UserHasPermission("Admin")) return Forbid();

            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new {
                    u.Id,
                    u.UserName,
                    u.FullName,
                    Permissions = u.UserPermissions.Select(p => p.PermissionId).ToList()
                })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound();
            return Json(user);
        }

        [HttpPost]
        public async Task<IActionResult> SaveUser([FromBody] UserDto model)
        {
            if (!UserHasPermission("Admin")) return Forbid();

            if (string.IsNullOrEmpty(model.Id)) 
            {
                var newUser = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = model.UserName,
                    FullName = model.FullName,
                    PasswordHash = HashPassword(model.Password)
                };
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
                model.Id = newUser.Id;
            }

            var userInDb = await _context.Users.FindAsync(model.Id);
            if (userInDb == null) return NotFound();

            userInDb.FullName = model.FullName;
            if (!string.IsNullOrEmpty(model.Password))
            {
                userInDb.PasswordHash = HashPassword(model.Password);
            }

            var oldPermissions = _context.UserPermissions.Where(p => p.UserId == model.Id);
            _context.UserPermissions.RemoveRange(oldPermissions);

            if (model.Permissions != null)
            {
                var newPermissions = model.Permissions.Select(pId => new UserPermission { UserId = model.Id, PermissionId = pId });
                _context.UserPermissions.AddRange(newPermissions);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Lưu người dùng thành công." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id) 
        {
            if (!UserHasPermission("Admin")) return Forbid();
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (user.UserName == "admin")
            {
                return Json(new { success = false, message = "Không thể xóa tài khoản gốc 'admin'." });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Xóa người dùng thành công." });
        }

        private bool UserHasPermission(string permissionId)
        {
            return User.Claims.Any(c => c.Type == "Permissions" && c.Value.Contains(permissionId));
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
