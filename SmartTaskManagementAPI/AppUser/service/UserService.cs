using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartTaskManagementAPI.AppUser.models;
using SmartTaskManagementAPI.AppUser.models.dto;
using SmartTaskManagementAPI.AppUser.service.impl;
using SmartTaskManagementAPI.Data;
using SmartTaskManagementAPI.Exceptions.modelNotFound;

namespace SmartTaskManagementAPI.AppUser.service;

public class UserService: IUserService
{
    //private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }
    
    public async Task<ApplicationUser> CreateUserAsync(RegistrationRequestDto registrationRequestDto)
    {
        var user = new ApplicationUser
        {
            UserName = registrationRequestDto.Username,
            Email = registrationRequestDto.Email,
            EmailConfirmed = true,
            FirstName = registrationRequestDto.FirstName,
            LastName = registrationRequestDto.LastName,

        };
        // Create the user in the database
        var result = await _userManager.CreateAsync(user, registrationRequestDto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"User creation failed: {errors}");
           //throw new Exception(result.Errors.FirstOrDefault()?.Description); 
        }

        // Ensure roles exist and assign them to the user
        if (registrationRequestDto?.Roles != null && registrationRequestDto.Roles.Any())
        {
            foreach (var role in registrationRequestDto.Roles)
            {
                // Check if the role exists, create if not
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole(role));
                    if (!roleResult.Succeeded)
                    {
                        var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                        throw new Exception($"Role creation failed: {errors}");
                    }
                }
                
            }
            
            // Assign the role to the user
            var roleAssignments = await _userManager.AddToRolesAsync(user, registrationRequestDto.Roles);
            if (!roleAssignments.Succeeded)
            {
                var errors = string.Join(", ", roleAssignments.Errors.Select(e => e.Description));
                throw new Exception($"Assigning role roles to user failed: {errors}");
            }
            
        }
        
        //await _userManager.AddToRoleAsync(user, registrationRequestDto.Roles.ToString());
        
        return user;
    }
    
    public async Task<ApplicationUser> GetUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new UserNotFoundException(userId);
        }
        return user;
    }
    
    public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
    {
        // Use asynchronous enumeration to fetch users
        var users = await  _userManager.Users.ToListAsync();
        return users;
    }

    public async Task DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new UserNotFoundException(userId);
        }
        await _userManager.DeleteAsync(user);
        
    }

    
    public async Task<ApplicationUser> UpdateUserByIdAsync(string userId, UpdateRequestDto updateRequestDto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new UserNotFoundException(userId);
        }
        user.FirstName = updateRequestDto.FirstName;
        user.LastName = updateRequestDto.LastName;
        user.Email = updateRequestDto.Email;
        user.UserName = updateRequestDto.Username;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new Exception(result.Errors.FirstOrDefault()?.Description);
        }
        return user;
        
    }
}