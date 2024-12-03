namespace SmartTaskManagementAPI.AppUser.models.dto;

//this class will be returned after registration
public class UserDto
{
    public string? Id { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    //public string? Roles { get; set; }
    public IEnumerable<string>? Roles { get; set; } = new List<string>();
    
    
}