using FreeStyleApp.Application.DTOs;
using FreeStyleApp.Application.Interfaces;
using FreeStyleApp.Application.Services;
using FreeStyleApp.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FreeStyleApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IAppDbContext _context;

        public AccountController(IAccountService accountService, IAppDbContext context)
        {
            _accountService = accountService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DoLogin([FromBody] LoginRequest model)
        {
            try
            {
                var user = await _accountService.ValidateUserAsync(model);
                await LogAuditAsync(model.Username, "Đăng nhập thành công", "Đăng nhập thành công.");

                var permissionCodes = user.UserPermissions.Select(p => p.Permission.Code).ToList();
                var permissionsString = string.Join(",", permissionCodes);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim("UserId", user.Id),
                    new Claim("UserFullName", user.FullName),
                    new Claim("Permissions", permissionsString)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                return Ok(new { success = true, redirectUrl = Url.Action("Index", "Home") });
            }
            catch (AppException ex) 
            {
                await LogAuditAsync(model.Username, "Đăng nhập thất bại", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await LogAuditAsync(User.Identity.Name, "Đăng xuất", "Người dùng đã đăng xuất.");
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

        private async Task LogAuditAsync(string userName, string actionType, string details)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                UserName = userName,
                ActionType = actionType,
                Details = details
            });
            await _context.SaveChangesAsync();
        }
    }
}