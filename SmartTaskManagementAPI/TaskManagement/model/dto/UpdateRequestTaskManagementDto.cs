using System.ComponentModel.DataAnnotations;

namespace SmartTaskManagementAPI.TaskManagement.model.dto;

public class UpdateRequestTaskManagementDto
{
    [Required]
    [MaxLength(10, ErrorMessage = "Title cannot be over 10 characters")]
    public string Title { get; set; } //(e.g., "Report")
    
    public string Description { get; set; } //(e.g., "Complete the quarterly financial report")

    //you can update date but not required
    public DateTime DueDate { get; set; } = DateTime.Now;
    
    [Required]
    [MaxLength(10, ErrorMessage = "Status cannot be over 10 characters")]
    public string Status { get; set; } //(e.g., "Pending", "Completed")
    
    [Required]
    [MaxLength(6, ErrorMessage = "Priority cannot be over 6 characters")]
    public string Priority { get; set; } //(e.g., "High", "Medium", "Low")

}