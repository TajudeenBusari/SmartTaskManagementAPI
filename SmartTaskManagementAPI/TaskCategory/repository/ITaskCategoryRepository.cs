namespace SmartTaskManagementAPI.TaskCategory.repository;
using model;
public interface ITaskCategoryRepository
{
    Task<List<TaskCategory>> GetAllTaskCategory();
    Task<TaskCategory?> GetByIdAsync(long id);
    Task<TaskCategory> CreateAsync(TaskCategory taskCategory);

    Task<TaskCategory?> UpdateAsync(long id, TaskCategory taskCategory);
    Task DeleteAsync(long id);
    Task AssignTaskManagementToCategoryAsync(long categoryId, Guid taskId);
    Task<bool> CategoryExists(long id);

}