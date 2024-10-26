using Microsoft.EntityFrameworkCore;
using SmartTaskManagementAPI.Data;
using SmartTaskManagementAPI.TaskManagement.model.Objects;

namespace SmartTaskManagementAPI.TaskManagement.repository.impl;
using model;

public class TaskManagementRepository: ITaskManagementRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public TaskManagementRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }
    
    public async Task<List<TaskManagement>> GetAllAsync()
    {
        return await _applicationDbContext.Tasks.ToListAsync();
    }

    public async Task<TaskManagement?> GetByIdAsync(Guid id)
    {
        return await _applicationDbContext
            .Tasks
            .FirstOrDefaultAsync(x => x.TaskId == id);
    }

    public async Task<TaskManagement> CreateAsync(TaskManagement taskManagement)
    {
        await _applicationDbContext.AddAsync(taskManagement);
        await _applicationDbContext.SaveChangesAsync();
        return taskManagement;
    }

    public async Task<TaskManagement?> UpdateAsync(Guid id, TaskManagement taskManagement)
    {
        var existingTaskManagement = await _applicationDbContext
            .Tasks
            .FirstOrDefaultAsync(x => x.TaskId == id);
        existingTaskManagement.Title = taskManagement.Title;
        existingTaskManagement.Priority = taskManagement.Priority;
        existingTaskManagement.Description = taskManagement.Description;
        existingTaskManagement.Status = taskManagement.Status;
        existingTaskManagement.DueDate = taskManagement.DueDate;
        //we are not updating the category it belongs to because many tasks can belong to several category
        
        await _applicationDbContext.SaveChangesAsync();
        return existingTaskManagement;
    }

    public async Task DeleteAsync(Guid id)
    {
        var existingTaskManagement = await _applicationDbContext
            .Tasks
            .FirstOrDefaultAsync(x => x.TaskId == id);
        _applicationDbContext.Remove(existingTaskManagement);
        await _applicationDbContext.SaveChangesAsync();
        
    }

    public async Task<List<TaskManagement>> GetAllByQueryAsync(TaskManagementQueryObject taskManagementQueryObject)
    {
        var queryable = _applicationDbContext
            .Tasks
            .AsQueryable();
        // Apply filtering based on the query object properties
        
        // Filter by Title if specified
        if (!string.IsNullOrWhiteSpace(taskManagementQueryObject.Title))
        {
            queryable = queryable.Where(t => 
                t.Title.Contains(taskManagementQueryObject.Title));
        }
        
        // Filter by Status if specified
        if (!string.IsNullOrWhiteSpace(taskManagementQueryObject.Status))
        {
            queryable = queryable.Where(s => 
                s.Status == taskManagementQueryObject.Status);
        }
        
        // Filter by Priority if specified
        if (!string.IsNullOrWhiteSpace(taskManagementQueryObject.Priority))
        {
            queryable = queryable.Where(p => 
                p.Priority == taskManagementQueryObject.Priority);
        }
        
        // Filter by DueDate if specified
        if (taskManagementQueryObject.DueDate.HasValue)
        {
            queryable = queryable.Where(d => 
                d.DueDate.Date == taskManagementQueryObject.DueDate.Value.Date);
        }


        return await queryable.ToListAsync();
    }
}



/*
 *the DBcontext is just like the jpaRepo in spring boot that has several methods
 * the custom repository class can use. Methods like AddAsync, Save etc
 *
 */
