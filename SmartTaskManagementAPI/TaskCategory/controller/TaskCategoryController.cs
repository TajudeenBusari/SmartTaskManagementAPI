using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartTaskManagementAPI.System;
using SmartTaskManagementAPI.TaskCategory.mappers;
using SmartTaskManagementAPI.TaskCategory.model.dto;
using SmartTaskManagementAPI.TaskCategory.service;

namespace SmartTaskManagementAPI.TaskCategory.controller;
using System;

[Route("api/v1/taskcategory")]
[ApiController]
public class TaskCategoryController: ControllerBase
{
    private readonly TaskCategoryService _taskCategoryService;
    private readonly TaskCategoryMapper _taskCatoryMapper;
    

    public TaskCategoryController(TaskCategoryService taskCategoryService)
    {
        _taskCategoryService = taskCategoryService;
        _taskCatoryMapper = new TaskCategoryMapper();
    }

    [HttpGet]
    public async Task<ActionResult<Result>> GetAllCategories()
    {
        var categories = await _taskCategoryService.GetAllAsync();
        //convert to Dto
        var categoryDtos = _taskCatoryMapper.MapFromCategoryListToCategoryDtoList(categories);
        
        return Ok(new Result(true, System.StatusCode.SUCCESS, "Find All Success", categoryDtos));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Result>> AddCategory([FromBody] CreateRequestCategoryDto createRequestCategoryDto)
    {
        if (!ModelState.IsValid)
        {
            return new Result(false, System.StatusCode.BAD_REQUEST, "Invalid Data", ModelState);
        }
        
        //convert to category domain
        var taskCategory = _taskCatoryMapper
            .MapFromCreateRequestCategoryDtoToTaskCategory(createRequestCategoryDto);
        var savedCategory = await _taskCategoryService.CreateAsync(taskCategory);
        //convert back to categoryDto
        var savedCategoryDto = _taskCatoryMapper.MapFromCategoryToCategoryDto(savedCategory);
        return Ok(new Result(true, System.StatusCode.SUCCESS, "Add One Success", savedCategoryDto));
    }

    [HttpGet]
    [Route("{id:long}")]
    public async Task<ActionResult<Result>> GetCategoryById([FromRoute] long id)
    {
        var returnedCategory = await _taskCategoryService.GetByIdAsync(id);
        //convert to Dto
        var returnedCategoryDto = _taskCatoryMapper.MapFromCategoryToCategoryDto(returnedCategory);
        return Ok(new Result(true, System.StatusCode.SUCCESS, "Find One Success", returnedCategoryDto));
    }

    [HttpPut]
    [Route("{id:long}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Result>> UpdateCategoryById([FromRoute]long id, UpdateRequestCategoryDto updateRequestCategoryDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }
        //convert to domain class
        var update=_taskCatoryMapper
            .MapFromUpdateRequestCategoryDtoToTaskCategory(updateRequestCategoryDto);
        var updated =await _taskCategoryService.UpdateByIdAsync(id, update);
        //convert back to Dto
        var updatedDto = _taskCatoryMapper.MapFromCategoryToCategoryDto(updated);
        return Ok(new Result(true, System.StatusCode.SUCCESS, "Update Success", updatedDto));
    }

    [HttpDelete]
    [Route("{id:long}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Result>> DeleteCategory([FromRoute] long id)
    {
        await _taskCategoryService.DeleteByIdAsync(id);
        
        return Ok(new Result(true, System.StatusCode.SUCCESS, "Delete Success"));
    }

    [HttpPut("{categoryId:long}/tasks/{taskId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Result>> AssignTaskToCategory([FromRoute] long categoryId, [FromRoute] Guid taskId)
    {
        await _taskCategoryService.AssignTaskManagementAsync(categoryId, taskId);
        return Ok(new Result(true, System.StatusCode.SUCCESS, "Assignment Success", categoryId));
    }
    
}