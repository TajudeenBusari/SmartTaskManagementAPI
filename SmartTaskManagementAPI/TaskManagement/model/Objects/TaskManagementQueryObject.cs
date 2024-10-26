namespace SmartTaskManagementAPI.TaskManagement.model.Objects;

public class TaskManagementQueryObject
{
    public string? Title { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public DateTime? DueDate { get; set; }
}

//TODO:  Search tasks by keywords (e.g title, description).
