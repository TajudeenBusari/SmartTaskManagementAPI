using System.ComponentModel.DataAnnotations;

namespace SmartTaskManagementAPI.AppUser.models.dto;

public class RegistrationRequestDto
{
    [Required]
    public string Username { get; set; }
    
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    [Required]
    public string? Password { get; set; }
    [Required]
    public string? FirstName { get; set; }
    [Required]
    public string? LastName { get; set; }
    public IEnumerable<string> Roles { get; set; }
    //or
    //public List<string> Roles { get; set; }
}