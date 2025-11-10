using FreeStyleApp.Application.DTOs;
using FreeStyleApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreeStyleApp.ViewComponents
{
    public class MenuViewComponent : ViewComponent
    {
        private readonly IFeatureGroupService _featureGroupService;
        private readonly IAppDbContext _context;

        public MenuViewComponent(IFeatureGroupService featureGroupService, IAppDbContext context)
        {
            _featureGroupService = featureGroupService;
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var userId = UserClaimsPrincipal?.FindFirst("UserId")?.Value;
                
                // Nếu không có userId trong claims, lấy từ UserName
                if (string.IsNullOrEmpty(userId))
                {
                    var userName = UserClaimsPrincipal?.Identity?.Name;
                    if (!string.IsNullOrEmpty(userName))
                    {
                        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
                        userId = user?.Id;
                    }
                }

                List<FeatureGroupDto> featureGroups = new();

                // Chỉ lấy các FeatureGroups mà user được gán (từ UserFeatureGroups)
                if (!string.IsNullOrEmpty(userId))
                {
                    try
                    {
                        featureGroups = await _featureGroupService.GetFeatureGroupsForUserAsync(userId);
                        // Debug: Log số lượng FeatureGroups
                        System.Diagnostics.Debug.WriteLine($"MenuViewComponent: Found {featureGroups.Count} FeatureGroups for user {userId}");
                    }
                    catch (Exception ex)
                    {
                        // Nếu có lỗi (có thể do table chưa tồn tại), trả về danh sách rỗng
                        System.Diagnostics.Debug.WriteLine($"MenuViewComponent Error: {ex.Message}");
                        featureGroups = new List<FeatureGroupDto>();
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("MenuViewComponent: userId is null or empty");
                }

                // Không hiển thị tất cả FeatureGroups cho admin nữa
                // Chỉ hiển thị những FeatureGroups đã được gán cho user
                // Nếu không có FeatureGroups nào được gán, sẽ hiển thị menu mặc định (fallback trong View)

                return View(featureGroups);
            }
            catch
            {
                // Nếu có lỗi, trả về danh sách rỗng để hiển thị menu mặc định
                return View(new List<FeatureGroupDto>());
            }
        }
    }
}

