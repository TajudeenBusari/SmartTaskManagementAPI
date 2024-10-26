
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTaskManagementAPI.TaskCategory.model;
using TaskManagement.model;

//category like "Work," "Personal," Research, Administrative, Operational, Marketing etc
//one side
[Table("Categories")]
public class TaskCategory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  // Auto-increment long ID
    public long TaskCategoryId { get; set; }

    [Required] 
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; } = string.Empty;

    // One category can have many tasks
    //one to many
    public List<TaskManagement> Tasks { get; set; } = new List<TaskManagement>();

    public int getNumberOfTasksManagement()
    {
        return Tasks.Count;
    }

}