using SmartTaskManagementAPI.AppUser.models;
using SmartTaskManagementAPI.AppUser.models.dto;

namespace SmartTaskManagementAPI.AppUser.service.impl;

public interface IUserService
{
    Task<ApplicationUser> CreateUserAsync(RegistrationRequestDto registrationRequestDto);
    Task<ApplicationUser> GetUserAsync(string userId);
    Task DeleteUserAsync(string userId);
    Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
    Task<ApplicationUser> UpdateUserByIdAsync(string userId, UpdateRequestDto updateRequestDto);
    Task UpdateUserRolesAsync(string userId, IEnumerable<string> roles);
    Task ChangePasswordAsync(string userId, string currentPassword, string newPassword, string confirmPassword);
}

