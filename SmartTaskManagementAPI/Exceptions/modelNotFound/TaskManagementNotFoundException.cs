namespace SmartTaskManagementAPI.Exceptions.modelNotFound;

public class TaskManagementNotFoundException : Exception
{

    public TaskManagementNotFoundException(Guid id)
        : base("Could not find TaskManagement with id " + id)
    {
        
    }
}