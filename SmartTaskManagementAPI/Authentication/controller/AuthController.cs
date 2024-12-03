using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTaskManagementAPI.AppUser.models;
using SmartTaskManagementAPI.AppUser.models.dto;
using SmartTaskManagementAPI.Authentication.model;
using SmartTaskManagementAPI.Authentication.service;
using SmartTaskManagementAPI.System;

namespace SmartTaskManagementAPI.Authentication.controller;
[ApiController]
[Route("api/v1/auth")]
public class AuthController: ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;

    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }
    
    [HttpPost("login")]
    public async Task<ActionResult<Result>> Login(LoginRequestDto loginRequestDto)
    {

        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == loginRequestDto.Username.ToLower());

        if (user == null)
        {
            return Unauthorized("Inavlid Username or Password");
        }
        var result  = await _signInManager.CheckPasswordSignInAsync(user, loginRequestDto.Password, false);
        if (result.Succeeded)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.GenerateToken(user, roles);
            return Ok(new Result(true, System.StatusCode.SUCCESS, "Login Success", token));
        }

        return Unauthorized("Username not found and/or password is incorrect");
    }
}