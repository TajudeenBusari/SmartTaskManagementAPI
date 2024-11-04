using Microsoft.EntityFrameworkCore;
using SmartTaskManagementAPI.Data;
using SmartTaskManagementAPI.Exceptions.modelNotFound;

namespace SmartTaskManagementAPI.TaskCategory.repository.impl;
using model;
public class TaskCategoryRepository: ITaskCategoryRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public TaskCategoryRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task<List<TaskCategory>> GetAllTaskCategory()
    {
        return await _applicationDbContext
            .Categories
            .Include(tc => tc.Tasks) //This will load the Tasks related to each TaskCategory
            .ToListAsync();
    }

    public async Task<TaskCategory?> GetByIdAsync(long id)
    {
        return await _applicationDbContext
            .Categories
            .FirstOrDefaultAsync(c => c.TaskCategoryId == id);
    }

    public async Task<TaskCategory> CreateAsync(TaskCategory taskCategory)
    {
        await _applicationDbContext.AddAsync(taskCategory);
        await _applicationDbContext.SaveChangesAsync();
        return taskCategory;
    }

    public async Task<TaskCategory?> UpdateAsync(long id, TaskCategory taskCategory)
    {
        var existingCategory = await _applicationDbContext
            .Categories
            .FirstOrDefaultAsync(x => x.TaskCategoryId == id);
        if (existingCategory == null)
        {
            return null;
        }
        //WE ARE UPDATING NAME AND DESCRIPTION
        existingCategory.Name = taskCategory.Name;
        existingCategory.Description = taskCategory.Description;
        await _applicationDbContext.SaveChangesAsync();
        return existingCategory;
    }

    public async Task DeleteAsync(long id)
    {
        var existingCategory = await _applicationDbContext
            .Categories
            .Include(c => c.Tasks)
            .FirstOrDefaultAsync(c =>
                c.TaskCategoryId == id);
        if (existingCategory == null)
        {
            return;
        }
        
        // Remove related tasks first
        _applicationDbContext.Tasks.RemoveRange(existingCategory.Tasks);

        _applicationDbContext.Categories.Remove(existingCategory);
        await _applicationDbContext.SaveChangesAsync();
        
    }

    public async Task AssignTaskManagementToCategoryAsync(long categoryId, Guid taskId)
    {
        
        //task to be assigned. First check if it exists in the DB/appContxt
        var taskToBeAssigned = await _applicationDbContext.Tasks
            .Include(t => t.TaskCategory)
            .FirstOrDefaultAsync(t => t.TaskId == taskId);
        if (taskToBeAssigned == null)
        {
            throw new TaskManagementNotFoundException(taskId);
        }
        
        // Check if the task already belongs to the category
        if (taskToBeAssigned.TaskCategoryId == categoryId)
        {
            throw new InvalidOperationException();
        }
        
        // Update the task to the new category
        taskToBeAssigned.TaskCategoryId = categoryId;
        
        // Save the changes to the database
        await _applicationDbContext.SaveChangesAsync();
        
    }

    public  Task<bool> CategoryExists(long id)
    {
        return _applicationDbContext
            .Categories
            .AnyAsync(c => c.TaskCategoryId == id);
    }
}