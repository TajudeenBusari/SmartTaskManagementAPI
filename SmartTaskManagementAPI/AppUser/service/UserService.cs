using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartTaskManagementAPI.AppUser.models;
using SmartTaskManagementAPI.AppUser.models.dto;
using SmartTaskManagementAPI.AppUser.service.impl;
using SmartTaskManagementAPI.Data;
using SmartTaskManagementAPI.Exceptions;
using SmartTaskManagementAPI.Exceptions.modelNotFound;

namespace SmartTaskManagementAPI.AppUser.service;

public class UserService: IUserService
{
    /***
     * I am using the UserManager to create, update, delete, and retrieve users from the database.
     * So, I doo not need the UserRepository class/interface and AppDbContext class.
     */
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

    public async Task UpdateUserRolesAsync(string userId, IEnumerable<string> roles)
    {
        throw new NotImplementedException();
    }

    public async Task ChangePasswordAsync(string userId, string currentPassword, string newPassword, string confirmPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new UserNotFoundException(userId);
        }

        // Check if the new password is the same as the old password
        if (newPassword == currentPassword)
        {
            throw new PasswordChangeIllegalArgument("New password cannot be the same as the old password.");
        }
        
        //check if old password is not correct
        var passwordHasher = new PasswordHasher<ApplicationUser>();
        if(user.PasswordHash != null && passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword) == PasswordVerificationResult.Failed)
        {
            throw new PasswordChangeIllegalArgument("Old password is incorrect.");
        }
        
        //check if new password and confirm new password are not same.
        if (newPassword != confirmPassword)
        {
            throw new PasswordChangeIllegalArgument("New password and confirm password do not match.");
        }
        
        
        //check password policy
        /***
         * Password policy
         * 1. At least one lowercase letter
         * 2. At least one uppercase letter
         * 3. At least one digit
         * 4. At least one special character
         * 5. Minimum length of 8 characters
         * 6. Maximum length of 15 characters
         */
        var passwordPolicy = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$";
        if (!Regex.IsMatch(newPassword, passwordPolicy))
        {
            throw new PasswordChangeIllegalArgument("New password must contain at least one lowercase letter, one uppercase letter, one digit, one special character, and be between 8 and 15 characters long.");
        }
        
        // Update the user's password
        var passwordHash = passwordHasher.HashPassword(user, newPassword);
        user.PasswordHash = passwordHash;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new Exception(result.Errors.FirstOrDefault()?.Description);
        }
        
    }
}