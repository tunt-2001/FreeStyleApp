using FreeStyleApp.Application.DTOs;
using FreeStyleApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FreeStyleApp.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        public IActionResult Index() => View();

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ. Mật khẩu mới phải có ít nhất 6 ký tự và khớp với mật khẩu xác nhận." });
            }

            var userId = User.FindFirstValue("UserId");
            var actionUser = User.Identity?.Name;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(actionUser))
            {
                return BadRequest(new { message = "Không thể xác định người dùng." });
            }

            await _profileService.ChangePasswordAsync(userId, model, actionUser);

            return Ok(new { success = true, message = "Đổi mật khẩu thành công." });
        }
    }
}