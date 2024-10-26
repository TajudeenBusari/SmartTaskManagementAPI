namespace SmartTaskManagementAPI.TaskCategory.repository;
using model;
public interface ITaskCategoryRepository
{
    Task<TaskCategory?> GetByIdAsync(long id);
    Task<TaskCategory> CreateAsync(TaskCategory taskCategory);
    
}