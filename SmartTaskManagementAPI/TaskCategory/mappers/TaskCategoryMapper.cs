using SmartTaskManagementAPI.TaskCategory.model.dto;

namespace SmartTaskManagementAPI.TaskCategory.mappers;
using model;
public class TaskCategoryMapper
{
    //TaskCategory to TaskCategoryDto
    public TaskCategoryDto MapFromCategoryToCategoryDto(TaskCategory category)
    {
        if (category == null)
        {
            return null;
        }

        return new TaskCategoryDto()
        {
            TaskCategoryId = category.TaskCategoryId,
            Description = category.Description,
            Name = category.Name,
            NumberOfTaskManagements = category.GetNumberOfTasksManagement()

        };
    }
    
    //TaskCategoryList to TaskCategoryListDto
    public List<TaskCategoryDto> MapFromCategoryListToCategoryDtoList(List<TaskCategory> categories)
    {
        if (categories == null)
        {
            return new List<TaskCategoryDto>(); //return empty list
        }

        return categories
            .Select(MapFromCategoryToCategoryDto)
            .ToList();
    }
    
    //ToDo To TaskCategory from CreateRequestTaskCategorytDto
    public TaskCategory MapFromCreateRequestCategoryDtoToTaskCategory(CreateRequestCategoryDto requestCategoryDto)
    {
        return new TaskCategory()
        {
            Description = requestCategoryDto.Description,
            Name = requestCategoryDto.Name

        };
    }
    
    //TODO TaskCategory from UpdateRequestTaskCategoryDto
    public TaskCategory MapFromUpdateRequestCategoryDtoToTaskCategory(UpdateRequestCategoryDto updateRequestCategoryDto)
    {
        return new TaskCategory()
        {
            Description = updateRequestCategoryDto.Description,
            Name = updateRequestCategoryDto.Name

        };
    }
}