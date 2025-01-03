using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace SmartTaskManagementAPI.TaskManagement.model.dto;

/***
 * to create a TaskManagement, user does not need to add TaskId(it will be generated automatically),
 * category it belongs to or categoryId, 
 */
public class CreateRequestTaskManagementDto
{
    [Required]
    [MinLength(5, ErrorMessage = "Title cannot be less than 5 characters")]
    [MaxLength(10, ErrorMessage = "Title cannot be over 10 characters")]
    
    public string Title { get; set; } 
    
    [MinLength(5, ErrorMessage = "Description must be 10 characters")]
    [MaxLength(100, ErrorMessage = "Description cannot be over 100 characters")]
    public string? Description { get; set; } //(e.g., "Complete the quarterly financial report")

    [Required(ErrorMessage = "Date is missing")] 
    public DateTime DueDate { get; set; } = DateTime.Now;

    [Required]
    [MaxLength(10, ErrorMessage = "Status cannot be over 10 characters")]
    public string Status { get; set; } = string.Empty; //(e.g., "Pending", "Completed")
    
    [Required]
    [MaxLength(6, ErrorMessage = "Priority cannot be over 6 characters")]
    public string Priority { get; set; } = string.Empty;//(e.g., "High", "Medium", "Low")
    
    
    public long? TaskCategoryId { get; set; } 
}