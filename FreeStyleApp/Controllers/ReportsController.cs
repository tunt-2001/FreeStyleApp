using FreeStyleApp.Application.Interfaces;
using FreeStyleApp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace FreeStyleApp.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly IAppDbContext _context;

        public ReportsController(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (!UserHasPermission("Admin")) return Forbid();

            // Get statistics
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalPermissions = await _context.Permissions.CountAsync();
            ViewBag.TotalAuditLogs = await _context.AuditLogs.CountAsync();
            ViewBag.LoginsToday = await _context.AuditLogs
                .CountAsync(log => log.ActionType == "Đăng nhập thành công" && log.Timestamp.Date == DateTime.UtcNow.Date);

            // Get recent activities
            var recentActivities = await _context.AuditLogs
                .OrderByDescending(log => log.Timestamp)
                .Take(10)
                .ToListAsync();

            ViewBag.RecentActivities = recentActivities;

            // Get user activity statistics
            var userActivities = await _context.AuditLogs
                .Where(log => log.ActionType == "Đăng nhập thành công")
                .GroupBy(log => log.UserName)
                .Select(g => new { UserName = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            ViewBag.UserActivities = userActivities;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ExportAuditLogs(DateTime? fromDate, DateTime? toDate, string? actionType, string? userName)
        {
            if (!UserHasPermission("Admin")) return Forbid();

            var query = _context.AuditLogs.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(log => log.Timestamp >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(log => log.Timestamp < toDate.Value.Date.AddDays(1));

            if (!string.IsNullOrEmpty(actionType))
                query = query.Where(log => log.ActionType == actionType);

            if (!string.IsNullOrEmpty(userName))
                query = query.Where(log => log.UserName.Contains(userName));

            var logs = await query.OrderByDescending(log => log.Timestamp).ToListAsync();

            // Export to CSV
            var csv = new StringBuilder();
            csv.AppendLine("Thời gian, Người dùng, Loại hành động, Chi tiết");

            foreach (var log in logs)
            {
                csv.AppendLine($"{log.Timestamp:yyyy-MM-dd HH:mm:ss}, {log.UserName}, {log.ActionType}, \"{log.Details}\"");
            }

            var fileName = $"AuditLogs_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();

            return File(bytes, "text/csv; charset=utf-8", fileName);
        }

        [HttpPost]
        public async Task<IActionResult> ExportUsers()
        {
            if (!UserHasPermission("Admin")) return Forbid();

            var users = await _context.Users
                .Include(u => u.UserPermissions)
                    .ThenInclude(up => up.Permission)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("ID, Tên đăng nhập, Họ và tên, Quyền");

            foreach (var user in users)
            {
                var permissions = string.Join("; ", user.UserPermissions.Select(up => up.Permission.Code));
                csv.AppendLine($"{user.Id}, {user.UserName}, {user.FullName}, \"{permissions}\"");
            }

            var fileName = $"Users_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();

            return File(bytes, "text/csv; charset=utf-8", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> GetStatistics()
        {
            if (!UserHasPermission("Admin")) return Forbid();

            var stats = new
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalPermissions = await _context.Permissions.CountAsync(),
                TotalAuditLogs = await _context.AuditLogs.CountAsync(),
                LoginsToday = await _context.AuditLogs
                    .CountAsync(log => log.ActionType == "Đăng nhập thành công" && log.Timestamp.Date == DateTime.UtcNow.Date),
                LoginsThisWeek = await _context.AuditLogs
                    .CountAsync(log => log.ActionType == "Đăng nhập thành công" && 
                                     log.Timestamp >= DateTime.UtcNow.AddDays(-7)),
                LoginsThisMonth = await _context.AuditLogs
                    .CountAsync(log => log.ActionType == "Đăng nhập thành công" && 
                                     log.Timestamp >= DateTime.UtcNow.AddDays(-30))
            };

            return Json(stats);
        }

        private bool UserHasPermission(string permissionCode)
        {
            var permissionsClaim = User.FindFirst("Permissions");
            return permissionsClaim != null && permissionsClaim.Value.Contains(permissionCode);
        }
    }
}

