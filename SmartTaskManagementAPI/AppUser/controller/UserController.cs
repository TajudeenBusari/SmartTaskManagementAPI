using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartTaskManagementAPI.AppUser.mapper;
using SmartTaskManagementAPI.AppUser.models;
using SmartTaskManagementAPI.AppUser.models.dto;
using SmartTaskManagementAPI.AppUser.service.impl;
using SmartTaskManagementAPI.System;

namespace SmartTaskManagementAPI.AppUser.controller;

[Route("api/v1/user")]
[ApiController]

public class UserController: ControllerBase
{
    private readonly IUserService _userService;
    private readonly UserMapper _userMapper;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserController(IUserService userService, UserManager<ApplicationUser> userManager)
    {
        _userService = userService;
        _userManager = userManager;
        _userMapper = new UserMapper();
    }

    /// <summary>
    /// Create a new user (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Result>> CreateUser([FromBody] RegistrationRequestDto request)
    {
        var authenticated = User.Identity.IsAuthenticated;
        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin || !authenticated)
        {
            return Unauthorized(new Result(false, System.StatusCode.UNAUTHORIZED, "User creation failed."));
            
        }
        var user = await _userService.CreateUserAsync(request);
        //Map to userDto
        var roles = await _userManager.GetRolesAsync(user);
        var userDto = _userMapper.MapFromAppUserToUserDto(user);
        userDto.Roles = roles;
        
        return Ok(new Result(true, System.StatusCode.SUCCESS, "A new user has been created", userDto));
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Result>> GetAllUsers()
    {
        
        var authenticated = User.Identity.IsAuthenticated;
        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin || !authenticated)
        {
            return Unauthorized(new Result(false, System.StatusCode.UNAUTHORIZED, "You are not authorized to access this resource."));
            
        }
        
        var users = await _userService.GetAllUsersAsync();
        //map to Dto
        var userDtoList = users.Select(async user =>
        {
            var roles = await _userManager.GetRolesAsync(user);
            var userDto = _userMapper.MapFromAppUserToUserDto(user);
            userDto.Roles = roles;
            return userDto;
        }).Select(t => t.Result).ToList(); // Wait for tasks to complete
        
        //var userDtoList = _userMapper.MapFromAppUsersToUserDtos(users);
        
        
        return Ok (new Result(true, System.StatusCode.SUCCESS, "Find All Success", userDtoList));
    }
    
    /// <summary>
    /// Get user by ID (Admin can access any user; users can access only their own data)
    /// </summary>
    [HttpGet]
    [Route("{userId}")]
    [Authorize]
    public async Task<ActionResult<Result>> GetUserById([FromRoute] string userId)
    {
        
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
            if (currentUserRole == "Admin" || currentUserId == userId)
            {
                var user = await _userService.GetUserAsync(userId);
                var roles = await _userManager.GetRolesAsync(user);
                //map to dto
                var userDto = _userMapper.MapFromAppUserToUserDto(user);
                userDto.Roles = roles;
                return Ok(new Result(true, System.StatusCode.SUCCESS, "Find One Success", userDto));
            }
            //actually returns the onChallenge error in the program.cs (swagger returns this)
            return Forbid(); //unit test returns this
            
    }

    /// <summary>
    /// Update user info (Admin or the user themselves)
    /// </summary>
    [HttpPut]
    [Route("{userId}")]
    [Authorize]
    public async Task<ActionResult<Result>> UpdateUser([FromRoute] string userId, [FromBody] UpdateRequestDto request)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
        if (currentUserRole == "Admin" || currentUserId == userId)
        {
            var updatedUser = await _userService.UpdateUserByIdAsync(userId, request);
            var roles = await _userManager.GetRolesAsync(updatedUser);
            var userDto = _userMapper.MapFromAppUserToUserDto(updatedUser);
            userDto.Roles = roles;
            return Ok(new Result(true, System.StatusCode.SUCCESS, "Update Success", userDto));
        }
        
        return Forbid();
    }

    [HttpDelete]
    [Route("{userId}")]
    [Authorize]
    public async Task<ActionResult<Result>> DeleteUser([FromRoute] string userId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
        if (currentUserRole == "Admin" || currentUserId == userId)
        {
            await _userService.DeleteUserAsync(userId);
            return Ok(new Result(true, System.StatusCode.SUCCESS, "Delete Success"));
        }
        return Forbid();
    }
    
}

/***
 * {
  "username": "admin",
  "password": "Admin123!"
  }
 */
