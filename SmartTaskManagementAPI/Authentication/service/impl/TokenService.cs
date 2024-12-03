using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SmartTaskManagementAPI.AppUser.models;
using SmartTaskManagementAPI.AppUser.models.dto;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace SmartTaskManagementAPI.Authentication.service.impl;

public class TokenService: ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly SymmetricSecurityKey _symmetricSecuritykey;
    
    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        _symmetricSecuritykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:signInKey"]));
    }
    public string GenerateToken(ApplicationUser user, IList<string> roles)
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
        var tokenDescriptor = new SecurityTokenDescriptor(
            
            //JwtSecurityToken
            /*issuer: _configuration["Jwt:issuer"],
            audience: _configuration["Jwt:audience"],
            //subject: claims,
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: signInCredentials*/
        )
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(30),
            Audience = _configuration["Jwt:audience"],
            Issuer = _configuration["Jwt:issuer"],
            SigningCredentials = signInCredentials
        };
        //return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}