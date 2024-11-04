using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTaskManagementAPI.TaskCategory.model.dto;

public class TaskCategoryDto
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  // Auto-increment long ID
    public long TaskCategoryId { get; set; }

    [Required(ErrorMessage = "Name is required")] 
    public string? Name { get; set; } = string.Empty;

    public string? Description { get; set; } = string.Empty;
    
    public int NumberOfTaskManagements { get; set; }
}