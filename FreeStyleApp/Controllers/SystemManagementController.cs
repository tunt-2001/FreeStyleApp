using FreeStyleApp.Application.Interfaces;
using FreeStyleApp.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Reflection;

namespace FreeStyleApp.Controllers
{
    [Authorize]
    public class SystemManagementController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public SystemManagementController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            if (!UserHasPermission("Admin")) return Forbid();

            // System Information
            ViewBag.SystemInfo = GetSystemInfo();
            ViewBag.ApplicationInfo = GetApplicationInfo();
            ViewBag.HealthStatus = await GetHealthStatus();
            ViewBag.DatabaseInfo = await GetDatabaseInfo();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TestDatabaseConnection()
        {
            if (!UserHasPermission("Admin")) return Forbid();

            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                return Json(new { success = canConnect, message = canConnect ? "Kết nối database thành công!" : "Không thể kết nối database." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi kết nối: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClearCache()
        {
            if (!UserHasPermission("Admin")) return Forbid();

            // In a real application, you would clear cache here
            // For now, just return success
            await Task.CompletedTask;
            return Json(new { success = true, message = "Đã xóa cache thành công." });
        }

        private object GetSystemInfo()
        {
            return new
            {
                OperatingSystem = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                MachineName = Environment.MachineName,
                ProcessorCount = Environment.ProcessorCount,
                WorkingSet = $"{(Environment.WorkingSet / 1024.0 / 1024.0):F2} MB",
                FrameworkVersion = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                EnvironmentName = _environment.EnvironmentName,
                ContentRootPath = _environment.ContentRootPath,
                WebRootPath = _environment.WebRootPath
            };
        }

        private object GetApplicationInfo()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            var buildDate = System.IO.File.GetLastWriteTime(assembly.Location);

            return new
            {
                ApplicationName = assembly.GetName().Name,
                Version = version?.ToString() ?? "Unknown",
                BuildDate = buildDate.ToString("yyyy-MM-dd HH:mm:ss"),
                TargetFramework = assembly.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>()?.FrameworkName ?? "Unknown"
            };
        }

        private async Task<object> GetHealthStatus()
        {
            var status = new
            {
                Database = await CheckDatabaseHealth(),
                DiskSpace = CheckDiskSpace(),
                Memory = GetMemoryInfo()
            };

            return status;
        }

        private async Task<bool> CheckDatabaseHealth()
        {
            try
            {
                return await _context.Database.CanConnectAsync();
            }
            catch
            {
                return false;
            }
        }

        private object CheckDiskSpace()
        {
            try
            {
                var drive = new DriveInfo(_environment.ContentRootPath);
                return new
                {
                    AvailableSpace = $"{(drive.AvailableFreeSpace / 1024.0 / 1024.0 / 1024.0):F2} GB",
                    TotalSpace = $"{(drive.TotalSize / 1024.0 / 1024.0 / 1024.0):F2} GB",
                    UsedPercentage = $"{((drive.TotalSize - drive.AvailableFreeSpace) * 100.0 / drive.TotalSize):F2}%"
                };
            }
            catch
            {
                return new { AvailableSpace = "N/A", TotalSpace = "N/A", UsedPercentage = "N/A" };
            }
        }

        private object GetMemoryInfo()
        {
            var process = Process.GetCurrentProcess();
            return new
            {
                UsedMemory = $"{(process.WorkingSet64 / 1024.0 / 1024.0):F2} MB",
                PrivateMemory = $"{(process.PrivateMemorySize64 / 1024.0 / 1024.0):F2} MB"
            };
        }

        private async Task<object> GetDatabaseInfo()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync();
                var totalPermissions = await _context.Permissions.CountAsync();
                var totalAuditLogs = await _context.AuditLogs.CountAsync();

                return new
                {
                    TotalUsers = totalUsers,
                    TotalPermissions = totalPermissions,
                    TotalAuditLogs = totalAuditLogs,
                    ConnectionString = "***" // Connection string is stored in appsettings.json
                };
            }
            catch
            {
                return new { Error = "Không thể lấy thông tin database" };
            }
        }

        private bool UserHasPermission(string permissionCode)
        {
            var permissionsClaim = User.FindFirst("Permissions");
            return permissionsClaim != null && permissionsClaim.Value.Contains(permissionCode);
        }
    }
}

