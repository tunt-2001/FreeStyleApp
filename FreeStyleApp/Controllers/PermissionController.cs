using FreeStyleApp.Application.Interfaces;
using FreeStyleApp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreeStyleApp.Controllers
{
    [Authorize]
    public class PermissionController : Controller
    {
        private readonly IPermissionService _permissionService;
        private readonly IAppDbContext _context;

        public PermissionController(IPermissionService permissionService, IAppDbContext context)
        {
            _permissionService = permissionService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (!UserHasPermission("Admin")) return Forbid();
            var permissions = await _context.Permissions.ToListAsync();
            return View(permissions);
        }

        [HttpPost]
        public async Task<IActionResult> SavePermission([FromBody] Permission model)
        {
            if (!UserHasPermission("Admin")) return Forbid();

            await _permissionService.SavePermissionAsync(model, User.Identity.Name);

            return Ok(new { success = true, message = "Lưu quyền thành công." });
        }

        [HttpPost]
        public async Task<IActionResult> DeletePermission(string id)
        {
            if (!UserHasPermission("Admin")) return Forbid();

            await _permissionService.DeletePermissionAsync(id, User.Identity.Name);

            return Ok(new { success = true, message = "Xóa quyền thành công." });
        }

        private bool UserHasPermission(string permissionCode)
        {
            var permissionsClaim = User.FindFirst("Permissions");
            return permissionsClaim != null && permissionsClaim.Value.Contains(permissionCode);
        }
    }
}