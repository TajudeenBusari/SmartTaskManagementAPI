using Microsoft.EntityFrameworkCore;
using SmartTaskManagementAPI.Data;

namespace SmartTaskManagementAPI.TaskCategory.repository.impl;
using model;
public class TaskCategoryRepository: ITaskCategoryRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public TaskCategoryRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
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
}