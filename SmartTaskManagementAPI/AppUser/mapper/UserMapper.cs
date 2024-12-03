using Microsoft.AspNetCore.Identity;
using SmartTaskManagementAPI.AppUser.models;
using SmartTaskManagementAPI.AppUser.models.dto;

namespace SmartTaskManagementAPI.AppUser.mapper;

public class UserMapper
{
    
    //Application user to userDto
    public UserDto MapFromAppUserToUserDto(ApplicationUser appUser)
    {
        
        if (appUser == null)
        {
            return null;
        }
        return new UserDto()
        {
            Id = appUser.Id,
            Email = appUser.Email,
            FirstName = appUser.FirstName,
            LastName = appUser.LastName,
            Username = appUser.UserName,
            Roles =new List<string>() // Roles will be set separately
        };
    }
    
    //List of Application users to userDtos
    public List<UserDto> MapFromAppUsersToUserDtos(IEnumerable<ApplicationUser> appUsers)
    {
        if (appUsers == null)
        {
            return (new List<UserDto>());
        }
        return appUsers.Select( MapFromAppUserToUserDto).ToList();
    }
}