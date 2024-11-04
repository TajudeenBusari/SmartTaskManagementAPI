using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTaskManagementAPI.TaskManagement.model;
using TaskCategory.model;
[Table("Tasks")]
public class TaskManagement
{
    //Many side
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-generate GUID
    public Guid TaskId { get; set; } = Guid.NewGuid(); // Use Guid for the primary key
    
    [Required(ErrorMessage = "Title is missing")]
    public string? Title { get; set; } //(e.g., "Report")
    
    public string? Description { get; set; } //(e.g., "Complete the quarterly financial report")

    [Required(ErrorMessage = "Date is missing")] 
    public DateTime DueDate { get; set; } = DateTime.Now;
    
    [Required(ErrorMessage = "Status is missing")]
    public string Status { get; set; } //(e.g., "Pending", "Completed")
    
    [Required(ErrorMessage = "Priority is missing")]
    public string Priority { get; set; } //(e.g., "High", "Medium", "Low")
    
    //[ForeignKey("TaskCategoryId")]
    //navigation property
    public TaskCategory? TaskCategory { get; set; } //many tasks can be owned by one TaskCategory
    public long? TaskCategoryId { get; set; }

    public void SetTaskCategory(TaskCategory category)
    {
        TaskCategory = category;
    }

    
}

/*
 *many TaskManagements can be under a single category
 *
 */