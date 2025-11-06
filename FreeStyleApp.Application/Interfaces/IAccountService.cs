using FreeStyleApp.Application.DTOs;
using FreeStyleApp.Domain.Entities;

namespace FreeStyleApp.Application.Interfaces
{
    public interface IAccountService
    {
        Task<User> ValidateUserAsync(LoginRequest model);
    }
}