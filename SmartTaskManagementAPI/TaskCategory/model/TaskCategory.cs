
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
    public string? Name { get; set; } = string.Empty;

    
    public string? Description { get; set; } = string.Empty;

    // One category can have many tasks
    //one to many
    public List<TaskManagement> Tasks { get;  set; } = new List<TaskManagement>()
    {
        
    };

    public int GetNumberOfTasksManagement()
    {
        return Tasks.Count;
    }

    public void RemoveAllTaskManagement()
    {
        foreach (var taskManagement in Tasks)
        {
            // Set the Owner(TaskCategory) of each taskManagement to null
            taskManagement.TaskCategory = null;
        }

        // Clear the TaskManagement list
        Tasks = new List<TaskManagement>();
    }

    public async void RemoveTaskManagement(TaskManagement foundTask)
    {
        foundTask.SetTaskCategory(null);
        Tasks.Remove(foundTask);
    }

    public async void AddTaskManagement(TaskManagement taskManagement)
    {
         taskManagement.SetTaskCategory(this);
         Tasks.Add(taskManagement);
    }
}