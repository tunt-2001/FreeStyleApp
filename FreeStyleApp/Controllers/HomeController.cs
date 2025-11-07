using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using FreeStyleApp.Infrastructure;
 

namespace FreeStyleApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Thông tin người dùng hiện tại
            var currentUserName = User?.Identity?.Name ?? string.Empty;
            var permissionsClaim = User?.FindFirst("Permissions")?.Value ?? string.Empty;
            var permissionCodes = permissionsClaim.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToList();
            ViewBag.IsAdmin = permissionCodes.Any(p => p.Contains("Admin"));

            // My section (dùng chung cho mọi vai trò)
            ViewBag.MyRecentActivities = await _context.AuditLogs
                .Where(l => l.UserName == currentUserName)
                .OrderByDescending(l => l.Timestamp)
                .Take(5)
                .ToListAsync();

            ViewBag.MyLastLogin = await _context.AuditLogs
                .Where(l => l.UserName == currentUserName && l.ActionType == "Đăng nhập thành công")
                .OrderByDescending(l => l.Timestamp)
                .Select(l => l.Timestamp)
                .FirstOrDefaultAsync();

            ViewBag.MyFailedLoginsToday = await _context.AuditLogs
                .Where(l => l.UserName == currentUserName && l.ActionType == "Đăng nhập thất bại" && l.Timestamp.Date == DateTime.UtcNow.Date)
                .CountAsync();

            ViewBag.MyPermissions = permissionCodes;

            // Admin section
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalPermissions = await _context.Permissions.CountAsync();
            ViewBag.LoginsToday = await _context.AuditLogs
                .CountAsync(log => log.ActionType == "Đăng nhập thành công" && log.Timestamp.Date == DateTime.UtcNow.Date);
            ViewBag.FailedLoginsToday = await _context.AuditLogs
                .CountAsync(log => log.ActionType == "Đăng nhập thất bại" && log.Timestamp.Date == DateTime.UtcNow.Date);
            ViewBag.ActiveUsers = await _context.Users.CountAsync(u => EF.Property<bool>(u, "IsActive") == true);
            ViewBag.LockedUsers = await _context.Users.CountAsync(u => EF.Property<bool>(u, "IsActive") == false);

            var recentActivities = await _context.AuditLogs
                .OrderByDescending(log => log.Timestamp)
                .Take(5)
                .ToListAsync();

            // System name cố định
            ViewBag.SystemName = "FreeStyleApp";

            return View(recentActivities);
        }

        public IActionResult AccessDenied() => View();
    }
}