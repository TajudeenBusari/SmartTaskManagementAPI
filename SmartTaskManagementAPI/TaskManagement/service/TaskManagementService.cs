using System;
using SmartTaskManagementAPI.Data;
using SmartTaskManagementAPI.Exceptions.modelNotFound;
using SmartTaskManagementAPI.TaskCategory.repository;
using SmartTaskManagementAPI.TaskManagement.model.Objects;
using SmartTaskManagementAPI.TaskManagement.repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SmartTaskManagementAPI.TaskManagement.service;
using model;

public class TaskManagementService
{
    private readonly ITaskManagementRepository _taskManagementRepository;
   

    public TaskManagementService(ITaskManagementRepository taskManagementRepository)
    {
        _taskManagementRepository = taskManagementRepository;
       
    }
    
    //Get all
    public async Task<List<TaskManagement>> GetAllAsync()
    {
        return await _taskManagementRepository.GetAllAsync();
        
    }
    
    //Get by Id
    public async Task<TaskManagement> GetByIdAsync(Guid guid)
    {
        var foundTaskManagement = await _taskManagementRepository.GetByIdAsync(guid);
        if (foundTaskManagement == null)
        {
            throw new TaskManagementNotFoundException(guid);
        }

        return foundTaskManagement;
    }
    
    //Create a TaskManagement
    public async Task<TaskManagement> CreateAsync(TaskManagement taskManagement)
    {
        
        var newSavedTaskManagement = await _taskManagementRepository.CreateAsync(taskManagement);
        return newSavedTaskManagement;
    }
   
    //Update a TaskManagement by id
    public async Task<TaskManagement> UpdateByIdAsync(Guid id, TaskManagement taskManagement)
    {
        
        var foundTaskManagement = await _taskManagementRepository.GetByIdAsync(id);
        if (foundTaskManagement == null)
        {
            throw new TaskManagementNotFoundException(id);
        }

        await _taskManagementRepository.UpdateAsync(id, taskManagement);
        return foundTaskManagement;
    }
    
    //Delete a Task by id
    public async Task DeleteByIdAsync(Guid id)
    {
        var foundTaskManagement = await _taskManagementRepository.GetByIdAsync(id);
        if (foundTaskManagement == null)
        {
            throw new TaskManagementNotFoundException(id);
        }

        await _taskManagementRepository.DeleteAsync(id);
        
    }
    
    //find all tasks by query object
    public async Task<List<TaskManagement>?> GetAllByQueryAsync(TaskManagementQueryObject queryObject)
    {

        var foundTaskManagements = await _taskManagementRepository.GetAllByQueryAsync(queryObject);
        if (foundTaskManagements == null)
        {
            return null;
        }
        
        return foundTaskManagements;
    }
    
}