using FreeStyleApp.Application.DTOs;

namespace FreeStyleApp.Application.Interfaces
{
    public interface IProfileService
    {
        Task ChangePasswordAsync(string userId, ChangePasswordDto model, string actionUser);
    }
}