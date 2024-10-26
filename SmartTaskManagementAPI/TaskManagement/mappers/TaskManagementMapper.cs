using SmartTaskManagementAPI.TaskManagement.model.dto;

namespace SmartTaskManagementAPI.TaskManagement.mappers;
using model;

public class TaskManagementMapper
{
    //TaskManagementMapper to TaskManagementMapperDto
    public  TaskManagementDto MapFromTaskMagtToTaskMagtDto(TaskManagement taskManagement)
    {
        if (taskManagement == null)
        {
            return null;
        }
        return new TaskManagementDto()
        {
            TaskId = taskManagement.TaskId,
            Title = taskManagement.Title,
            Description = taskManagement.Description,
            DueDate = taskManagement.DueDate,
            Status = taskManagement.Status,
            Priority = taskManagement.Priority,
            TaskCategoryId = taskManagement.TaskCategoryId
        };
    }
    
    //TaskManagementMapper to TaskManagementMapperDto
    public  List<TaskManagementDto> MapFromTaskMagtListToTaskMagtListDto(List<TaskManagement> taskManagementList)
    {
        if (taskManagementList == null)
        {
            return new List<TaskManagementDto>(); //return empty list
        }

        return taskManagementList
            .Select(MapFromTaskMagtToTaskMagtDto)
            .ToList();

    }
    
    //To TaskManagement from CreateRequestTaskManagementDto
    public TaskManagement MapFromCreateRequestTaskManagementDtoToTaskManagement(CreateRequestTaskManagementDto createRequestTaskManagementDto)
    {
        if (createRequestTaskManagementDto == null)
        {
            return null;
        }

        return new TaskManagement()
        {
            Title = createRequestTaskManagementDto.Title,
            Description = createRequestTaskManagementDto.Description,
            DueDate = createRequestTaskManagementDto.DueDate,
            Status = createRequestTaskManagementDto.Status,
            Priority = createRequestTaskManagementDto.Priority,
            TaskCategoryId = createRequestTaskManagementDto.TaskCategoryId
        };
        
        /***
         * NB: Once the client provide this data, the object that will be returned back to
         * client is the TaskManagementDto, that is, we will map back to the TaskManagementDto
         * that has categoryId in addition to the data in CreateRequestTaskManagementDto in the controller
         */
    }

    public TaskManagement MapFromUpdateRequestTaskManagementDtoToTaskManagement(UpdateRequestTaskManagementDto updateRequestTaskManagementDto)
    {
        if (updateRequestTaskManagementDto == null)
        {
            return null;
        }

        return new TaskManagement()
        {
            Title = updateRequestTaskManagementDto.Title,
            Description = updateRequestTaskManagementDto.Description,
            DueDate = updateRequestTaskManagementDto.DueDate,
            Status = updateRequestTaskManagementDto.Status, 
            Priority = updateRequestTaskManagementDto.Priority
            
        };
    }
    
    

    public static CreateRequestTaskManagementDto MapFromTaskManagementToCreateTaskManagementDto(
        TaskManagement taskManagement)
    {
        if (taskManagement == null)
        {
            return null;
        }

        return new CreateRequestTaskManagementDto()
        {
            Title = taskManagement.Title,
            Description = taskManagement.Description,
            DueDate = taskManagement.DueDate,
            Status = taskManagement.Status,
            Priority = taskManagement.Priority,
            TaskCategoryId = taskManagement.TaskCategoryId
            
        };
    }
}