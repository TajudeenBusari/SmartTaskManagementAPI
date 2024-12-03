using System.ComponentModel.DataAnnotations;

namespace SmartTaskManagementAPI.AppUser.models.dto;

public class UpdateRequestDto
{
    [Required]
    public string? Username { get; set; }
    [Required]
    public string? FirstName { get; set; }
    [Required]
    public string? LastName { get; set; }
    [Required]
    public string? Email { get; set; }
   
}