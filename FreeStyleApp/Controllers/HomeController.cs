using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using FreeStyleApp.Data; 

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
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalPermissions = await _context.Permissions.CountAsync();
            ViewBag.LoginsToday = await _context.AuditLogs
                .CountAsync(log => log.ActionType == "Đăng nhập thành công" && log.Timestamp.Date == DateTime.UtcNow.Date);

            var recentActivities = await _context.AuditLogs
                .OrderByDescending(log => log.Timestamp)
                .Take(5)
                .ToListAsync();

            return View(recentActivities);
        }

        public IActionResult AccessDenied() => View();
    }
}