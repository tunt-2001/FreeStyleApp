using FreeStyleApp.Application.DTOs;
using FreeStyleApp.Application.Interfaces;
using FreeStyleApp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FreeStyleApp.Controllers
{
    [Authorize]
    public class FeatureGroupController : Controller
    {
        private readonly IFeatureGroupService _featureGroupService;
        private readonly IAppDbContext _context;
        private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;

        public FeatureGroupController(IFeatureGroupService featureGroupService, IAppDbContext context, IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
        {
            _featureGroupService = featureGroupService;
            _context = context;
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
        }

        public async Task<IActionResult> Index()
        {
            if (!UserHasPermission("Admin")) return Forbid();

            var groups = await _featureGroupService.GetAllFeatureGroupsAsync();
            return View(groups);
        }

        [HttpGet]
        public async Task<IActionResult> GetFeatureGroup(string id)
        {
            if (!UserHasPermission("Admin")) return Forbid();

            var group = await _featureGroupService.GetFeatureGroupByIdAsync(id);
            if (group == null)
                return Json(new { success = false, message = "Không tìm thấy nhóm chức năng" });

            return Json(new { success = true, data = group });
        }

        [HttpPost]
        public async Task<IActionResult> SaveFeatureGroup([FromBody] FeatureGroupCreateDto dto)
        {
            if (!UserHasPermission("Admin")) return Forbid();

            try
            {
                var result = await _featureGroupService.CreateFeatureGroupAsync(dto);
                await LogAuditAsync("Tạo nhóm chức năng", $"Tạo nhóm chức năng: {result.Name}");
                return Json(new { success = true, message = "Tạo nhóm chức năng thành công", data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateFeatureGroup([FromBody] FeatureGroupUpdateDto dto)
        {
            if (!UserHasPermission("Admin")) return Forbid();

            try
            {
                var result = await _featureGroupService.UpdateFeatureGroupAsync(dto);
                await LogAuditAsync("Cập nhật nhóm chức năng", $"Cập nhật nhóm chức năng: {result.Name}");
                return Json(new { success = true, message = "Cập nhật nhóm chức năng thành công", data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFeatureGroup(string id)
        {
            if (!UserHasPermission("Admin")) return Forbid();

            try
            {
                await _featureGroupService.DeleteFeatureGroupAsync(id);
                await LogAuditAsync("Xóa nhóm chức năng", $"Xóa nhóm chức năng ID: {id}");
                return Json(new { success = true, message = "Xóa nhóm chức năng thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFeaturesByGroupId(string groupId)
        {
            if (!UserHasPermission("Admin")) return Forbid();

            try
            {
                var features = await _featureGroupService.GetFeaturesByGroupIdAsync(groupId);
                return Json(new { success = true, data = features });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFeature(string id)
        {
            if (!UserHasPermission("Admin")) return Forbid();

            var feature = await _context.Features
                .Include(f => f.FeatureGroup)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (feature == null)
                return Json(new { success = false, message = "Không tìm thấy chức năng" });

            var dto = new FeatureDto
            {
                Id = feature.Id,
                Name = feature.Name,
                Controller = feature.Controller,
                Action = feature.Action,
                Icon = feature.Icon,
                FeatureGroupId = feature.FeatureGroupId,
                FeatureGroupName = feature.FeatureGroup.Name,
                DisplayOrder = feature.DisplayOrder,
                IsActive = feature.IsActive
            };

            return Json(new { success = true, data = dto });
        }

        [HttpPost]
        public async Task<IActionResult> SaveFeature([FromBody] FeatureCreateDto dto)
        {
            if (!UserHasPermission("Admin")) return Forbid();

            try
            {
                var result = await _featureGroupService.CreateFeatureAsync(dto);
                await LogAuditAsync("Tạo chức năng", $"Tạo chức năng: {result.Name}");
                return Json(new { success = true, message = "Tạo chức năng thành công", data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateFeature([FromBody] FeatureUpdateDto dto)
        {
            if (!UserHasPermission("Admin")) return Forbid();

            try
            {
                var result = await _featureGroupService.UpdateFeatureAsync(dto);
                await LogAuditAsync("Cập nhật chức năng", $"Cập nhật chức năng: {result.Name}");
                return Json(new { success = true, message = "Cập nhật chức năng thành công", data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFeature(string id)
        {
            if (!UserHasPermission("Admin")) return Forbid();

            try
            {
                await _featureGroupService.DeleteFeatureAsync(id);
                await LogAuditAsync("Xóa chức năng", $"Xóa chức năng ID: {id}");
                return Json(new { success = true, message = "Xóa chức năng thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserFeatureGroups(string userId)
        {
            if (!UserHasPermission("Admin")) return Forbid();

            var userGroups = await _featureGroupService.GetFeatureGroupsForUserAsync(userId);
            var allGroups = await _featureGroupService.GetAllFeatureGroupsAsync();

            return Json(new
            {
                success = true,
                assignedGroups = userGroups.Select(g => g.Id).ToList(),
                allGroups = allGroups.Select(g => new { g.Id, g.Name, g.Icon }).ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> AssignFeatureGroupsToUser([FromBody] AssignFeatureGroupsToUserDto dto)
        {
            if (!UserHasPermission("Admin")) return Forbid();

            try
            {
                await _featureGroupService.AssignFeatureGroupsToUserAsync(dto.UserId, dto.FeatureGroupIds);
                var user = await _context.Users.FindAsync(dto.UserId);
                await LogAuditAsync("Gán nhóm chức năng", $"Gán nhóm chức năng cho người dùng: {user?.UserName}");
                return Json(new { success = true, message = "Gán nhóm chức năng thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả Controllers và Actions có trong project.
        /// Hệ thống tự động quét tất cả các Controller trong thư mục Controllers/
        /// và lấy tất cả các Action methods (public IActionResult hoặc Task&lt;IActionResult&gt;).
        /// 
        /// CÁCH HOẠT ĐỘNG:
        /// - IActionDescriptorCollectionProvider tự động lấy tất cả actions đã được đăng ký trong routing
        /// - Bất kỳ Controller mới nào bạn tạo trong thư mục Controllers/ sẽ TỰ ĐỘNG xuất hiện ở đây
        /// - Bất kỳ Action mới nào bạn thêm vào Controller cũng sẽ TỰ ĐỘNG xuất hiện
        /// 
        /// LƯU Ý:
        /// - Chỉ lấy các action có return type IActionResult hoặc Task&lt;IActionResult&gt;
        /// - Loại bỏ Account controller (vì là login/logout)
        /// - Loại bỏ các action không phù hợp (DoLogin, Logout)
        /// </summary>
        [HttpGet]
        public IActionResult GetControllersAndActions()
        {
            if (!UserHasPermission("Admin")) return Forbid();

            // IActionDescriptorCollectionProvider tự động lấy TẤT CẢ các actions đã được đăng ký trong routing
            // Điều này có nghĩa là bất kỳ Controller mới nào bạn tạo sẽ TỰ ĐỘNG xuất hiện ở đây
            var controllers = _actionDescriptorCollectionProvider.ActionDescriptors.Items
                .OfType<ControllerActionDescriptor>()
                // Loại bỏ Account controller (login/logout không cần trong menu)
                .Where(ad => !ad.ControllerName.Equals("Account", StringComparison.OrdinalIgnoreCase))
                // Loại bỏ các action không phù hợp (các action trả về JSON, API endpoints, helper methods)
                .Where(ad => !ad.ActionName.Equals("DoLogin", StringComparison.OrdinalIgnoreCase) && 
                            !ad.ActionName.Equals("Logout", StringComparison.OrdinalIgnoreCase) &&
                            !ad.ActionName.StartsWith("Get", StringComparison.OrdinalIgnoreCase) && // Loại bỏ các Get* methods (API endpoints)
                            !ad.ActionName.StartsWith("Save", StringComparison.OrdinalIgnoreCase) && // Loại bỏ các Save* methods
                            !ad.ActionName.StartsWith("Update", StringComparison.OrdinalIgnoreCase) && // Loại bỏ các Update* methods
                            !ad.ActionName.StartsWith("Delete", StringComparison.OrdinalIgnoreCase) && // Loại bỏ các Delete* methods
                            !ad.ActionName.StartsWith("Export", StringComparison.OrdinalIgnoreCase) && // Loại bỏ các Export* methods
                            !ad.ActionName.StartsWith("Send", StringComparison.OrdinalIgnoreCase) && // Loại bỏ các Send* methods
                            !ad.ActionName.StartsWith("Assign", StringComparison.OrdinalIgnoreCase) && // Loại bỏ các Assign* methods
                            !ad.ActionName.Equals("TestDatabaseConnection", StringComparison.OrdinalIgnoreCase) &&
                            !ad.ActionName.Equals("ClearCache", StringComparison.OrdinalIgnoreCase) &&
                            !ad.ActionName.Equals("ChangePassword", StringComparison.OrdinalIgnoreCase) &&
                            !ad.ActionName.Equals("AccessDenied", StringComparison.OrdinalIgnoreCase))
                // Chỉ lấy các action trả về View (không phải JSON/API)
                .Where(ad => ad.MethodInfo.ReturnType == typeof(IActionResult) || 
                            ad.MethodInfo.ReturnType == typeof(Task<IActionResult>))
                .GroupBy(ad => ad.ControllerName)
                .Select(g => new
                {
                    ControllerName = g.Key,
                    Actions = g.Select(a => a.ActionName)
                        .Distinct()
                        .OrderBy(a => a == "Index" ? 0 : 1) // Index luôn ở đầu
                        .ThenBy(a => a)
                        .ToList()
                })
                .OrderBy(c => c.ControllerName)
                .ToList();

            return Json(new { success = true, data = controllers });
        }

        #region Helper Methods

        private bool UserHasPermission(string permissionCode)
        {
            var permissionsClaim = User.FindFirst("Permissions");
            return permissionsClaim != null && permissionsClaim.Value.Contains(permissionCode);
        }

        private async Task LogAuditAsync(string actionType, string details)
        {
            _context.AuditLogs.Add(new Domain.Entities.AuditLog
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

