using Microsoft.AspNetCore.Identity;

namespace SmartTaskManagementAPI.AppUser.models;

public class ApplicationUser: IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}