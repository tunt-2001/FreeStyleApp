using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreeStyleApp.Data;
using FreeStyleApp.Models;

[Authorize]
public class PermissionController : Controller
{
    private readonly AppDbContext _context;
    public PermissionController(AppDbContext context) { _context = context; }

    public async Task<IActionResult> Index()
    {
        if (!User.Claims.Any(c => c.Value.Contains("Admin"))) return Forbid();
        var permissions = await _context.Permissions.ToListAsync();
        return View(permissions);
    }

    [HttpPost]
    public async Task<IActionResult> SavePermission([FromBody] Permission model)
    {
        if (!User.Claims.Any(c => c.Value.Contains("Admin"))) return Forbid();

        if (string.IsNullOrEmpty(model.Id))
        {
            model.Id = Guid.NewGuid().ToString();
            _context.Permissions.Add(model);
        }
        else
        {
            var existing = await _context.Permissions.FindAsync(model.Id);
            if (existing == null) return NotFound();
            existing.Name = model.Name;
        }
        await _context.SaveChangesAsync();
        return Json(new { success = true, message = "Lưu vai trò thành công." });
    }

    [HttpPost]
    public async Task<IActionResult> DeletePermission(string id)
    {
        if (!User.Claims.Any(c => c.Value.Contains("Admin"))) return Forbid();
        var permission = await _context.Permissions.FindAsync(id);
        if (permission == null) return NotFound();
        _context.Permissions.Remove(permission);
        await _context.SaveChangesAsync();
        return Json(new { success = true, message = "Xóa vai trò thành công." });
    }
}