using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartTaskManagementAPI.Exceptions.modelNotFound;
using SmartTaskManagementAPI.System;
using SmartTaskManagementAPI.TaskCategory.controller;
using SmartTaskManagementAPI.TaskCategory.mappers;
using SmartTaskManagementAPI.TaskCategory.model.dto;
using SmartTaskManagementAPI.TaskCategory.repository;
using SmartTaskManagementAPI.TaskCategory.service;
using SmartTaskManagementAPI.TaskManagement.repository;


namespace SmartTaskManagementAPITest.TaskCategory.controller;
using SmartTaskManagementAPI.TaskCategory.model;
using SmartTaskManagementAPI.TaskManagement.model;

[TestSubject(typeof(TaskCategoryController))]
public class TaskCategoryControllerTest
{
    private readonly Mock<ITaskCategoryRepository> _mockCategoryRepository;
    private readonly TaskCategoryService _taskCategoryService;
    private readonly Mock<ITaskManagementRepository> _mockTaskRepository;
    private readonly TaskCategoryController _controller;
    private readonly TaskCategoryMapper _mapper;
    private readonly List<TaskCategory> _categoryList;
    //private readonly List<TaskManagement> _taskList;

    // Setup method (runs before each test)
    public TaskCategoryControllerTest()
    {
        _mockCategoryRepository = new Mock<ITaskCategoryRepository>();
        _mockTaskRepository = new Mock<ITaskManagementRepository>();
        // Inject the mock repository into the service
        _taskCategoryService = new TaskCategoryService(_mockCategoryRepository.Object, _mockTaskRepository.Object);
        _mapper = new TaskCategoryMapper();
        // Pass the service into the controller
        _controller = new TaskCategoryController(_taskCategoryService);

        //_taskList = new List<TaskManagement>();
        var task1 = new TaskManagement();
        task1.TaskId = Guid.NewGuid();
        task1.Title = "title1";
        task1.Priority = "priority1";
        task1.Status = "status1";
        task1.Description = "description1";
        task1.DueDate = DateTime.Now;
        
        var task2 = new TaskManagement();
        task2.TaskId = Guid.NewGuid();
        task2.Title = "title2";
        task2.Priority = "priority2";
        task2.Status = "status2";
        task2.Description = "description2";
        task2.DueDate = DateTime.Now;
        
        var task3 = new TaskManagement();
        task3.TaskId = Guid.NewGuid();
        task3.Title = "title3";
        task3.Priority = "priority3";
        task3.Status = "status3";
        task3.Description = "description3";
        task3.DueDate = DateTime.Now;
        
        
        _categoryList = new List<TaskCategory>() //initialize as empty
        {
            new TaskCategory()
            {
                TaskCategoryId = 100L,
                Description = "Description1",
                Name = "Name1",
                Tasks = new List<TaskManagement>()
                {
                    task1, task2
                }

            },
            new TaskCategory()
            {
                TaskCategoryId = 200L,
                Description = "Description2",
                Name = "Name2",
                Tasks = new List<TaskManagement>()
                {
                    task3
                }

            },
            
            new TaskCategory()
            {
                TaskCategoryId = 300L,
                Description = "Description3",
                Name = "Name3",
                Tasks = new List<TaskManagement>()
                {
                    //no task
                }
            },
        };
    }
    
    
    [Fact]
    public async Task TestGetAllCategorySuccess()
    {
        //Arrange
        
        //Mock
        _mockCategoryRepository
            .Setup(repo
                => repo.GetAllTaskCategory())
            .ReturnsAsync(_categoryList);
        //Act
        var result = await _controller.GetAllCategories();
        
        //Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualResult = Assert.IsType<Result>(okResult.Value);
        Assert.True(actualResult.flag);
        Assert.Equal(200, actualResult.code);
        Assert.Equal("Find All Success", actualResult.message);
        Assert.IsType<List<TaskCategoryDto>>(actualResult.data);
        var actualResultData = actualResult.data as List<TaskCategoryDto>;
        Assert.Equal(_categoryList[0].TaskCategoryId.ToString(), actualResultData?[0].TaskCategoryId.ToString());
        Assert.Equal(_categoryList[1].TaskCategoryId.ToString(), actualResultData?[1].TaskCategoryId.ToString());
        Assert.Equal(_categoryList[0].Description, actualResultData?[0].Description);
        Assert.Equal(_categoryList[1].Description, actualResultData?[1].Description);
        Assert.Equal(_categoryList[0].Name, actualResultData?[0].Name);
        Assert.Equal(_categoryList[1].Name, actualResultData?[1].Name);
        Assert.Equal(_categoryList[0].Tasks.Count.ToString(), actualResultData?[0].NumberOfTaskManagements.ToString());
        Assert.Equal(_categoryList[1].Tasks.Count.ToString(), actualResultData?[1].NumberOfTaskManagements.ToString());
    }


    [Fact]
    public async Task TestAddCategorySuccess()
    {
        //Arrange
        //create Request Dto
        var requestDto = new CreateRequestCategoryDto()
        {
            Description = "Description1",
            Name = "Name1",
        };
        
        //model that will be returned by the service class
        var domainModel = new TaskCategory()
        {
            TaskCategoryId = 200L,
            Description = "Description1",
            Name = "Name1",
            Tasks = new List<TaskManagement>()
            {
                
            }
        };
        
        //task category dto
        var categoryDto = new TaskCategoryDto()
        {
            TaskCategoryId = 200L,
            Name = "Name1",
            Description = "Description1",
            NumberOfTaskManagements = 0
        };

        domainModel = _mapper.MapFromCreateRequestCategoryDtoToTaskCategory(requestDto);
        _mockCategoryRepository
            .Setup(repo
                => repo.CreateAsync(It.IsAny<TaskCategory>()))
            .ReturnsAsync(domainModel);
        
        //Act
        var result = await _controller.AddCategory(requestDto);
        
        //Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualResult = Assert.IsType<Result>(okResult.Value);
        Assert.True(actualResult.flag);
        Assert.Equal(200, actualResult.code);
        Assert.Equal("Add One Success", actualResult.message);
        Assert.IsType<TaskCategoryDto>(actualResult.data);
        var returnedTasks = actualResult.data as TaskCategoryDto;
        Assert.NotNull(returnedTasks?.TaskCategoryId.ToString());
        Assert.Equal(requestDto.Name, returnedTasks.Name);
        Assert.Equal(requestDto.Description, returnedTasks.Description);
        
    }


    [Fact]
    public async Task TestGetCategoryByIdSuccess()
    {
        //Arrange
        
        //Mock
        _mockCategoryRepository
            .Setup(repo =>
                repo.GetByIdAsync(_categoryList[0].TaskCategoryId))
            .ReturnsAsync(_categoryList[0]);
        
        //Act
        var result = await _controller.GetCategoryById(_categoryList[0].TaskCategoryId);
        
        //Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualResult = Assert.IsType<Result>(okResult.Value);
        Assert.True(actualResult.flag);
        Assert.Equal(200, actualResult.code);
        Assert.Equal("Find One Success", actualResult.message);
        Assert.IsType<TaskCategoryDto>(actualResult.data);
        var returnedTasks = actualResult.data as TaskCategoryDto;
        Assert.Equal(_categoryList[0].TaskCategoryId, returnedTasks?.TaskCategoryId);
        Assert.Equal(_categoryList[0].Name, returnedTasks?.Name);
        Assert.Equal(_categoryList[0].Description, returnedTasks?.Description);
    }
    
    [Fact]
    public async Task TestGetCategoryByNonExistingIdFailure()
    {
        //Arrange
        const long nonExistingId = 900L;
        
        //Mock
        _mockCategoryRepository
            .Setup(repo =>
                repo.GetByIdAsync(nonExistingId))
            .ReturnsAsync((TaskCategory)null);
        
        //Act
        var exception = await Assert.ThrowsAsync<TaskCategoryNotFoundException>(() =>
            _controller.GetCategoryById(nonExistingId));
        
        //Assert
        Assert.Equal("Could not find TaskCategory with id " + nonExistingId, exception.Message);
        _mockCategoryRepository
            .Verify(repo => repo.GetByIdAsync(nonExistingId), Times.Once);
    }

    [Fact]
    public async Task TestUpdateCategorySuccess()
    {
        //Arrange
        var oldCategory = new TaskCategory()
        {
            TaskCategoryId = 300L,
            Description = "Description3",
            Name = "Name3",
            Tasks = new List<TaskManagement>()
            {
                //no task
            }

        };
        
        //update
        var updateRequestDto = new UpdateRequestCategoryDto()
        {
           Name = "Name4", //update
           Description = "Description3"
           
        };
        
        //will be returned by the service class
        var updated = new TaskCategory()
        {
            TaskCategoryId = oldCategory.TaskCategoryId,
            Description = oldCategory.Description,
            Name = updateRequestDto.Name,
            Tasks = new List<TaskManagement>()
            {
            //no task
            }
        };

        oldCategory = _mapper.MapFromUpdateRequestCategoryDtoToTaskCategory(updateRequestDto);

        //Mock
        //first find and then update
        _mockCategoryRepository
            .Setup(repo =>
                repo.GetByIdAsync(oldCategory.TaskCategoryId))
            .ReturnsAsync(oldCategory);

        _mockCategoryRepository
            .Setup(repo =>
                repo.UpdateAsync(oldCategory.TaskCategoryId, It.IsAny<TaskCategory>()))
            .ReturnsAsync(updated);

        //Act
        var result = await _controller.UpdateCategoryById(oldCategory.TaskCategoryId, updateRequestDto);
        
        //Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualResult = Assert.IsType<Result>(okResult.Value);
        Assert.True(actualResult.flag);
        Assert.Equal(200, actualResult.code);
        Assert.Equal("Update Success", actualResult.message);
        Assert.IsType<TaskCategoryDto>(actualResult.data);
        var returnedTasks = actualResult.data as TaskCategoryDto;
        Assert.Equal(updated.Name, returnedTasks?.Name); //only name updated

    }
    
    [Fact]
    public async Task TestUpdateCategoryWithNonExistingIdFailure()
    {
        //Arrange
        const long nonExistingId = 1000L;
        
        //update request
        var updateRequestDto = new UpdateRequestCategoryDto()
        {
            Name = "Name4", //update
            Description = "Description3"
           
        };
        var nonExistingCategory = _mapper.MapFromUpdateRequestCategoryDtoToTaskCategory(updateRequestDto);
        
        //Mock
        _mockCategoryRepository
            .Setup(repo =>
                repo.GetByIdAsync(nonExistingId))
            .ReturnsAsync((TaskCategory)null);
        
        //Act
        var exception = await Assert
            .ThrowsAsync<TaskCategoryNotFoundException>(() =>
                _controller.GetCategoryById(nonExistingId));
        
        //Assert
        Assert.Equal("Could not find TaskCategory with id " + nonExistingId, exception.Message);
        

    }

    [Fact]
    public async Task TestDeleteCategorySuccess()
    {
        //Arrange
        //old data to be deleted
        var oldCategory = new TaskCategory()
        {
            TaskCategoryId = 300L,
            Description = "Description3",
            Name = "Name3",
            Tasks = new List<TaskManagement>()
            {
                //no task
            }
        };

        //Mock
        _mockCategoryRepository
            .Setup(repo =>
                repo.GetByIdAsync(oldCategory.TaskCategoryId))
            .ReturnsAsync(oldCategory);
        
        _mockCategoryRepository
            .Setup(repo => 
                repo.DeleteAsync(oldCategory.TaskCategoryId)).Verifiable();
        
        //Act
        var result = await _controller.DeleteCategory(oldCategory.TaskCategoryId);
        
        //Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualResult = Assert.IsType<Result>(okResult.Value);
        Assert.True(actualResult.flag);
        Assert.Equal(200, actualResult.code);
        Assert.Equal("Delete Success", actualResult.message);
        var returnedTasks = actualResult.data;
        Assert.Null(returnedTasks);
    }
    
    //i am skipping the test for nonExistingId
}