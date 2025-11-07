using FreeStyleApp.Application.DTOs;
using FreeStyleApp.Application.Interfaces;
using FreeStyleApp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreeStyleApp.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IAppDbContext _context;
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IAppDbContext context, IUserService userService, ILogger<UserController> logger)
        {
            _context = context;
            _userService = userService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string searchString, string sortOrder, int? pageNumber)
        {
            if (!UserHasPermission("Admin")) return Forbid();

            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["UserNameSortParm"] = sortOrder == "username" ? "username_desc" : "username";
            ViewData["CurrentFilter"] = searchString;

            var usersQuery = _context.Users.AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                usersQuery = usersQuery.Where(u => u.UserName.Contains(searchString) || u.FullName.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    usersQuery = usersQuery.OrderByDescending(u => u.FullName);
                    break;
                case "username":
                    usersQuery = usersQuery.OrderBy(u => u.UserName);
                    break;
                case "username_desc":
                    usersQuery = usersQuery.OrderByDescending(u => u.UserName);
                    break;
                default:
                    usersQuery = usersQuery.OrderBy(u => u.FullName);
                    break;
            }

            int pageSize = 10;
            var paginatedUsers = await PaginatedList<User>.CreateAsync(usersQuery.AsNoTracking(), pageNumber ?? 1, pageSize);

            ViewBag.AllPermissions = await _context.Permissions.ToListAsync();
            return View(paginatedUsers);
        }

        [HttpGet]
        public async Task<IActionResult> GetUser(string id)
        {
            if (!UserHasPermission("Admin")) return Forbid();

            var user = await _context.Users
                .Include(u => u.UserPermissions)
                    .ThenInclude(up => up.Permission)
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.FullName,
                    u.Email,
                    u.IsActive,
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

            var actionUser = User.Identity?.Name ?? "System";
            var savedUser = await _userService.SaveUserAsync(model, actionUser);
            string actionType = string.IsNullOrEmpty(model.Id) ? "Tạo người dùng" : "Cập nhật người dùng";
            await LogAuditAsync(actionType, $"Lưu thành công người dùng '{savedUser.UserName}'.");

            return Ok(new { success = true, message = "Lưu người dùng thành công." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (!UserHasPermission("Admin")) return Forbid();

            var userToDelete = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            var actionUser = User.Identity?.Name ?? "System";
            await _userService.DeleteUserAsync(id, actionUser);

            if (userToDelete != null)
            {
                await LogAuditAsync("Xóa người dùng", $"Xóa thành công người dùng '{userToDelete.UserName}'.");
            }

            return Ok(new { success = true, message = "Xóa người dùng thành công." });
        }

        [HttpPost]
        public async Task<IActionResult> SendPassword(string id)
        {
            if (!UserHasPermission("Admin")) return Forbid();

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return BadRequest(new { success = false, message = "Không tìm thấy người dùng." });
            }

            var actionUser = User.Identity?.Name ?? "System";
            try
            {
                var newPassword = await _userService.GenerateAndSendPasswordAsync(id, actionUser);
                await LogAuditAsync("Gửi mật khẩu", $"Đã gửi mật khẩu mới đến email của người dùng '{user.UserName}'.");
                return Ok(new { success = true, message = $"Đã gửi mật khẩu mới đến email {user.Email}." });
            }
            catch (Exception ex)
            {
                await LogAuditAsync("Gửi mật khẩu thất bại", $"Không thể gửi mật khẩu cho người dùng '{user.UserName}': {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        #region Helper Methods

        private bool UserHasPermission(string permissionCode)
        {
            var permissionsClaim = User.FindFirst("Permissions");
            return permissionsClaim != null && permissionsClaim.Value.Contains(permissionCode);
        }

        private async Task LogAuditAsync(string actionType, string details)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                UserName = User.Identity?.Name ?? "System",
                ActionType = actionType,
                Details = details
            });
            await _context.SaveChangesAsync();
        }

        #endregion
    }
}