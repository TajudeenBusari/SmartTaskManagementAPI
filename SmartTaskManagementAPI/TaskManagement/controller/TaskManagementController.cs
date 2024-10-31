
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SmartTaskManagementAPI.Exceptions.modelNotFound;
using SmartTaskManagementAPI.TaskManagement.mappers;
using SmartTaskManagementAPI.TaskManagement.model.dto;
using SmartTaskManagementAPI.TaskManagement.model.Objects;
using SmartTaskManagementAPI.TaskManagement.service;

namespace SmartTaskManagementAPI.TaskManagement.controller;
using System;


[Route("api/v1/taskmanagement")]
[ApiController]
public class TaskManagementController: ControllerBase
{
    private readonly TaskManagementService _taskManagementService;
    private readonly TaskManagementMapper _managementMapper;

    public TaskManagementController(TaskManagementService taskManagementService)
    {
        _taskManagementService = taskManagementService;
        _managementMapper = new TaskManagementMapper();
    }

    [HttpGet]
    public async Task<ActionResult<Result>> GetAllTaskManagement()
    {
        
        var taskManagementList = await _taskManagementService.GetAllAsync();
        //convert to dto
        var taskManagementListDto =  _managementMapper
            .MapFromTaskMagtListToTaskMagtListDto(taskManagementList);
        return Ok (new Result(true, System.StatusCode.SUCCESS, "Find All Success", taskManagementListDto));
    }

    [HttpGet]
    [Route("{id:Guid}")]
    public async Task<ActionResult<Result>> GetTaskManagementByGuid([FromRoute] Guid id)
    {
        var returnedTaskManagement = await _taskManagementService.GetByIdAsync(id);

        if (returnedTaskManagement != null)
        {
            //Convert found TaskManagement to Dto
            var returnedTaskManagementDto = _managementMapper
                .MapFromTaskMagtToTaskMagtDto(returnedTaskManagement);
        
            return Ok(new Result(true, System.StatusCode.SUCCESS, "Find One Success", returnedTaskManagementDto));
        }
        else
        {
            return NotFound(new Result(false, System.StatusCode.NOT_FOUND, new TaskManagementNotFoundException(id).Message));
        }
        
    }

    [HttpPost]
    public async Task<ActionResult<Result>> AddTaskManagement([FromBody] CreateRequestTaskManagementDto createRequestTaskManagementDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new Result(false, System.StatusCode.BAD_REQUEST, "Invalid Data", ModelState));
        }
        //convert to TaskManager
        var taskManagement = _managementMapper
            .MapFromCreateRequestTaskManagementDtoToTaskManagement(createRequestTaskManagementDto);
        var savedTaskManagement = await _taskManagementService.CreateAsync(taskManagement);
        
        //convert back to TaskManagementDto
        var savedTaskManagementDto = _managementMapper.MapFromTaskMagtToTaskMagtDto(savedTaskManagement);
        
        return  Ok(new Result(true, System.StatusCode.SUCCESS, "Add One Success", savedTaskManagementDto));
    }

    [HttpPut]
    [Route("{id:Guid}")]
    public async Task<ActionResult<Result>> UpdateTaskManagement(
        [FromRoute] Guid id, 
        [FromBody] UpdateRequestTaskManagementDto updateRequestTaskManagementDto)
    {
        if (!ModelState.IsValid)
        {
            //for now, it is not returning this but the inbuilt exception
            return BadRequest(new Result(false, System.StatusCode.BAD_REQUEST, "Provided data is/are invalid."));
        }
        //convert to domain class
        var update = _managementMapper
            .MapFromUpdateRequestTaskManagementDtoToTaskManagement(updateRequestTaskManagementDto);
        var updated = await _taskManagementService.UpdateByIdAsync(id, update);
        
        //convert back to TaskManagementDto
        var updatedDto = _managementMapper.MapFromTaskMagtToTaskMagtDto(updated);
        
        return  Ok(new Result(true, System.StatusCode.SUCCESS, "Update Success", updatedDto));
    }

    [HttpDelete]
    [Route("{id:Guid}")]
    
    public async Task<ActionResult<Result>> DeleteTaskManagement([FromRoute] Guid id)
    {
        await _taskManagementService.DeleteByIdAsync(id);
        return Ok(new Result(true, System.StatusCode.SUCCESS, "Delete Success"));
    }
    
    //TODO:  Search tasks by keywords (e.g title, description etc).
    [HttpGet("search")]
    public async Task<ActionResult<Result>> GetAllTaskManagementByQueryObj([FromQuery] TaskManagementQueryObject queryObject)
    {
        var taskManagementList = await _taskManagementService.GetAllByQueryAsync(queryObject);
        if (taskManagementList == null || !taskManagementList.Any())
        {
            return NotFound(new Result(false, System.StatusCode.NOT_FOUND, "No TaskManagement matching the criteria."));
        }
        //else convert to dto
        var taskManagementListDto = _managementMapper.MapFromTaskMagtListToTaskMagtListDto(taskManagementList);
        
        return Ok (new Result(true, System.StatusCode.SUCCESS, "Find All Success", taskManagementListDto)) ;
    }
    
}