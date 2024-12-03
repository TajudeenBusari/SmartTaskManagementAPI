using SmartTaskManagementAPI.AppUser.models;
using SmartTaskManagementAPI.AppUser.models.dto;

namespace SmartTaskManagementAPI.Authentication.service;

public interface ITokenService
{
    
    string GenerateToken(ApplicationUser user, IList<string> roles);
    
    
    /*Task<RegistrationRequestDto> Register(RegistrationRequestDto registrationRequestDto);
    Task<LoginRequestDto> Login(LoginRequestDto loginRequestDto);
    Task<bool> AssignRole(string email, string roleName);*/
}