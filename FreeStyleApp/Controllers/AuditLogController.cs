using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreeStyleApp.Infrastructure;
using FreeStyleApp.Domain.Entities;

namespace FreeStyleApp.Controllers
{
    [Authorize]
    public class AuditLogController : Controller
    {
        private readonly AppDbContext _context;
        public AuditLogController(AppDbContext context) { _context = context; }

        public async Task<IActionResult> Index(
            string searchUser,
            string actionType,
            DateTime? fromDate,
            DateTime? toDate,
            int? pageNumber)
        {
            if (!User.Claims.Any(c => c.Value.Contains("Admin"))) return Forbid();

            ViewData["CurrentFilterUser"] = searchUser;
            ViewData["CurrentFilterAction"] = actionType;
            ViewData["CurrentFromDate"] = fromDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentToDate"] = toDate?.ToString("yyyy-MM-dd");

            var logsQuery = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(searchUser))
            {
                logsQuery = logsQuery.Where(log => log.UserName.Contains(searchUser));
            }
            if (!string.IsNullOrEmpty(actionType))
            {
                logsQuery = logsQuery.Where(log => log.ActionType == actionType);
            }
            if (fromDate.HasValue)
            {
                logsQuery = logsQuery.Where(log => log.Timestamp >= fromDate.Value.Date);
            }
            if (toDate.HasValue)
            {
                logsQuery = logsQuery.Where(log => log.Timestamp < toDate.Value.Date.AddDays(1));
            }

            logsQuery = logsQuery.OrderByDescending(log => log.Timestamp);

            const int pageSize = 15;
            var paginatedLogs = await PaginatedList<Domain.Entities.AuditLog>.CreateAsync(logsQuery.AsNoTracking(), pageNumber ?? 1, pageSize);
            ViewBag.ActionTypes = await _context.AuditLogs.Select(l => l.ActionType).Distinct().ToListAsync();
            return View(paginatedLogs);
        }
    }
}