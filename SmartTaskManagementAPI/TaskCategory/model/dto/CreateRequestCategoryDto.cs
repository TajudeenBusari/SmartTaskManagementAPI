using System.ComponentModel.DataAnnotations;

namespace SmartTaskManagementAPI.TaskCategory.model.dto;

public class CreateRequestCategoryDto
{
    [Required]
    [MaxLength(25, ErrorMessage = "Category Name cannot be over 15 characters")]
    [MinLength(4, ErrorMessage = "Category Name cannot be less than 4 characters")]
    public string? Name { get; set; } = string.Empty;
    
    [MaxLength(100, ErrorMessage = "Category Description cannot be over 100 characters")]
    [MinLength(4, ErrorMessage = "Category Description cannot be less than 4 characters")]
    public string? Description { get; set; } = string.Empty;
    
    
}