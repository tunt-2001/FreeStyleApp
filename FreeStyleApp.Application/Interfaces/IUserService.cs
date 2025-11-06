using FreeStyleApp.Application.DTOs;
using FreeStyleApp.Domain.Entities;

namespace FreeStyleApp.Application.Interfaces
{
    public interface IUserService
    {
        Task<User> SaveUserAsync(UserDto model, string actionUser);
        Task DeleteUserAsync(string id, string actionUser);
    }
}