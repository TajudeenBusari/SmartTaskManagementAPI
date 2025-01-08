using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SmartTaskManagementAPI.AppUser.models;
using SmartTaskManagementAPI.AppUser.models.dto;
using SmartTaskManagementAPI.Client;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace SmartTaskManagementAPI.Authentication.service.impl;

public class TokenService: ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly SymmetricSecurityKey _symmetricSecuritykey;
    private readonly RedisCacheClient _redisCacheClient;
    
    public TokenService(IConfiguration configuration, RedisCacheClient redisCacheClient)
    {
        _configuration = configuration;
        _symmetricSecuritykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:signInKey"]));
        _redisCacheClient = redisCacheClient;
    }
    public async Task <string> GenerateToken(ApplicationUser user, IList<string> roles)
    {
        
        //create claim
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.UserName)
        };
        
        //Add role as claim
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        
        var signInCredentials = new SigningCredentials(_symmetricSecuritykey, SecurityAlgorithms.HmacSha256);
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(60),
            Audience = _configuration["Jwt:Audience"],
            Issuer = _configuration["Jwt:Issuer"],
            SigningCredentials = signInCredentials
        };
        
        //Generate token
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
       var tokenString = tokenHandler.WriteToken(token);
       //var redisKey = $"whitelist:{user.Id}"; //$"whiteList:{userId}"
       var redisKey = "whitelist:" + user.Id;
       await _redisCacheClient.SetAsync(redisKey, tokenString, TimeSpan.FromMinutes(60));
       return tokenString;
    }
}