using System.ComponentModel.DataAnnotations;

namespace SmartTaskManagementAPI.Authentication.model;

public class LoginRequestDto
{
    [Required]
    public string? Username { get; set; }
    [Required]
    public string? Password { get; set; }
}