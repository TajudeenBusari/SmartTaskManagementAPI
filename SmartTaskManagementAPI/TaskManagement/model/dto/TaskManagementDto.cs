using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartTaskManagementAPI.TaskCategory.model.dto;

namespace SmartTaskManagementAPI.TaskManagement.model.dto;
using TaskCategory.model;

public class TaskManagementDto
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  
    public Guid TaskId { get; set; }  
    
    [Required(ErrorMessage = "Title is missing")]
    public string Title { get; set; } 
    
    public string? Description { get; set; } //(e.g., "Complete the quarterly financial report")

    [Required(ErrorMessage = "Date is missing")] 
    public DateTime DueDate { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Status is missing")]
    public string Status { get; set; } = string.Empty; //(e.g., "Pending", "Completed")
    
    [Required(ErrorMessage = "Priority is missing")]
    public string Priority { get; set; } = string.Empty;//(e.g., "High", "Medium", "Low")
    
    //[ForeignKey("TaskCategoryId")]
    //navigation property
    
    //public TaskCategory? TaskCategory { get; set; } we are not returning this to the client
    
    public long? TaskCategoryId { get; set; }
    public TaskCategoryDto TaskOwner;
}