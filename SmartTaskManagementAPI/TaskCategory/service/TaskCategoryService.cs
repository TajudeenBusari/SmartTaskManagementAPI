using SmartTaskManagementAPI.Exceptions.modelNotFound;
using SmartTaskManagementAPI.TaskCategory.repository;

namespace SmartTaskManagementAPI.TaskCategory.service;
using model;
public class TaskCategoryService
{
    private readonly ITaskCategoryRepository _categoryRepository;
    
    public TaskCategoryService(ITaskCategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }
    
    //Get a category
    public async Task<TaskCategory> GetByIdAsync(long id)
    {
        var foundTaskCategory = await _categoryRepository.GetByIdAsync(id);
        if (foundTaskCategory == null)
        {
            throw new TaskCategoryNotFoundException(id);
        }

        return foundTaskCategory;
    }
    
}