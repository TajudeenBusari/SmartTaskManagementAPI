using SmartTaskManagementAPI.TaskManagement.model.Objects;

namespace SmartTaskManagementAPI.TaskManagement.repository;
using model;
public interface ITaskManagementRepository
{
    //Get all taskManagements
    Task <List<TaskManagement>>GetAllAsync();
    //Get taskManagement by Id
    Task<TaskManagement?> GetByIdAsync(Guid id);
    
    //Create a taskManagement
    Task<TaskManagement> CreateAsync(TaskManagement taskManagement);
    
    //update a taskManagement
    Task<TaskManagement?> UpdateAsync(Guid id, TaskManagement taskManagement);
    
    //Delete a taskManagement by id
    Task DeleteAsync(Guid id);
    
    //Get a list of TaskManagements by query object
    Task<List<TaskManagement>> GetAllByQueryAsync(TaskManagementQueryObject taskManagementQueryObject);
}