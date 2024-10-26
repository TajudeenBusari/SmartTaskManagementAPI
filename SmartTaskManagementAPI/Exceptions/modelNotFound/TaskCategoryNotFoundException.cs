namespace SmartTaskManagementAPI.Exceptions.modelNotFound;

public class TaskCategoryNotFoundException: Exception
{
    public TaskCategoryNotFoundException(long id)
        : base("Could not find TaskCategory with id " + id)
    {
        
    } 
}