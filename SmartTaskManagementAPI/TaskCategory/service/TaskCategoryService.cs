using SmartTaskManagementAPI.Exceptions.modelNotFound;
using SmartTaskManagementAPI.TaskCategory.repository;
using SmartTaskManagementAPI.TaskManagement.repository;

namespace SmartTaskManagementAPI.TaskCategory.service;
using model;
public class TaskCategoryService
{
    private readonly ITaskCategoryRepository _categoryRepository;

    private readonly ITaskManagementRepository _managementRepository;
    
    public TaskCategoryService(ITaskCategoryRepository categoryRepository, ITaskManagementRepository managementRepository)
    {
        _categoryRepository = categoryRepository;
        _managementRepository = managementRepository;
    }
    
    //Get all category
    public async Task<List<TaskCategory>> GetAllAsync()
    {
        return await _categoryRepository.GetAllTaskCategory();
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
    
    //Create a category
    public async Task<TaskCategory> CreateAsync(TaskCategory taskCategory)
    {
        var newSavedTaskCategory = await _categoryRepository.CreateAsync(taskCategory);
        return newSavedTaskCategory;
    }
    
    //Update a category
    public async Task<TaskCategory> UpdateByIdAsync(long id, TaskCategory taskCategory)
    {
        //first check if exist
        var foundCategory = await _categoryRepository.GetByIdAsync(id);
        if (foundCategory == null)
        {
            throw new TaskCategoryNotFoundException(id);
        }

        await _categoryRepository.UpdateAsync(id, taskCategory);
        
        return foundCategory;
    }
    
    //Delete category
    public async Task DeleteByIdAsync(long id)
    {
        var foundCategory = await _categoryRepository.GetByIdAsync(id);
        if (foundCategory == null)
        {
            throw new TaskCategoryNotFoundException(id);
        }
        
        //before removing category, we must first unassign all taskManagement under it
        
        foundCategory.RemoveAllTaskManagement();

        await _categoryRepository.DeleteAsync(id);
    }
    
    //TODO Assign Task to category

    public async Task AssignTaskManagementAsync(long categoryId, Guid taskManagementId)
    {
        // Perform any additional validation or checks if necessary
        var foundTask = await _managementRepository.GetByIdAsync(taskManagementId);
        var foundCategory = await _categoryRepository.GetByIdAsync(categoryId);
        if (foundTask == null )
        {
            throw new TaskManagementNotFoundException(taskManagementId);
            
        }

        if (foundCategory == null)
        {
            throw new TaskCategoryNotFoundException(categoryId);
        }
        
        if (foundTask.TaskCategoryId == categoryId)
        {
            throw new InvalidOperationException();
        }
        
        
        await _categoryRepository.AssignTaskManagementToCategoryAsync(categoryId, taskManagementId);
    }
    
    
}