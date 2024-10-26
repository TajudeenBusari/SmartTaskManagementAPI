
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using SmartTaskManagementAPI.Exceptions.modelNotFound;
using SmartTaskManagementAPI.System;
using SmartTaskManagementAPI.TaskCategory.model;
using SmartTaskManagementAPI.TaskManagement.controller;
using SmartTaskManagementAPI.TaskManagement.mappers;
using SmartTaskManagementAPI.TaskManagement.model.dto;
using SmartTaskManagementAPI.TaskManagement.model.Objects;
using SmartTaskManagementAPI.TaskManagement.repository;
using SmartTaskManagementAPI.TaskManagement.service;

namespace SmartTaskManagementAPITest.TaskManagement.controller;
using SmartTaskManagementAPI.TaskManagement.model;


public class TaskManagementControllerTest
{
    private readonly Mock<ITaskManagementRepository> _mockRepository;
    //private readonly Mock<TaskManagementService> _mockService;
    private readonly TaskManagementService _taskManagementService;
    private readonly TaskManagementController _controller;
    private readonly TaskManagementMapper _mapper;
    
    // Constructor to set up the mocks and controller
    public TaskManagementControllerTest()
    {
        // Mock the repository that the service depends on
        _mockRepository = new Mock<ITaskManagementRepository>();
        
        // Inject the mock repository into the service
        _taskManagementService = new TaskManagementService(_mockRepository.Object);
        _mapper = new TaskManagementMapper();
        
        // Pass the service into the controller
        _controller = new TaskManagementController(_taskManagementService);
        
    }

    [Fact]
    public async Task TestGetAllTaskManagementSuccess()
    {
        // Arrange: Create a list of mock tasks
        var tasksManagementList = new List<TaskManagement>()
        {
            new TaskManagement()
            {
                TaskId = new Guid("212bd5f9-d108-4dc4-9ef4-03fbe237ad74"),
                Title = "title1", 
                Status = "Open",
                Description = "description1",
                DueDate = DateTime.Now,
                Priority = "priority1",
                TaskCategory = new TaskCategory()
                {
                    Name = ""
                },
                TaskCategoryId = 100L
            },
            new TaskManagement()
            {
                TaskId = new Guid("3fc52bc0-70d2-4301-89c1-f6cbd271cae8"),
                Title = "title2", 
                Status = "Close",
                Description = "description2",
                DueDate = DateTime.Now,
                Priority = "priority2",
                TaskCategory = new TaskCategory()
                {
                    Name = ""
                },
                TaskCategoryId = 200L
            }
        };
        
        // Mock the REPO to return these tasks
        /***
         * Not in the controller class test, we are mocking the repo and using
         * in the setup instead of the service class bcos the service takes the repo as argument
         */
        _mockRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(tasksManagementList);
        
        // Act: Call the controller method
        var result = await _controller.GetAllTaskManagement();
        
        
        // Assert
        Assert.NotNull(result);
        //Assert.IsType<Result>(result);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualResult = Assert.IsType<Result>(okResult.Value);
        
        Assert.True(actualResult.flag);
        Assert.Equal(200, actualResult.code);
        Assert.Equal("Find All Success", actualResult.message);
        Assert.IsType<List<TaskManagementDto>>(actualResult.data);
        var returnedTasks = actualResult.data as List<TaskManagementDto>;
        Assert.Equal("212bd5f9-d108-4dc4-9ef4-03fbe237ad74", returnedTasks?[0].TaskId.ToString());
        Assert.Equal("title1", returnedTasks?[0].Title);
        Assert.Equal("Open", returnedTasks?[0].Status);
        Assert.Equal("description1", returnedTasks?[0].Description);
        Assert.Equal((DateTime.Now).ToString(), returnedTasks?[0].DueDate.ToString());
        Assert.Equal("priority1", returnedTasks?[0].Priority);
        Assert.Equal(100.ToString(), returnedTasks?[0].TaskCategoryId.ToString());
        Assert.Equal(2, returnedTasks?.Count); // Verify the number of tasks
        


    }

    [Fact]
    public async Task TestGetManagementByGuidSuccess()
    {
        //Arrange
        var taskManagement = new TaskManagement();
        taskManagement.TaskId = new Guid("212bd5f9-d108-4dc4-9ef4-03fbe237ad74");
        taskManagement.Title = "title1";
        taskManagement.Status = "Open";
        taskManagement.Description = "description1";
        taskManagement.DueDate = DateTime.Now;
        taskManagement.Priority = "priority1";
        taskManagement.TaskCategory = new TaskCategory()
        {
            Name = ""
        };
        taskManagement.TaskCategoryId = 100L;
        
        //Mock
        _mockRepository.Setup(repo => 
                repo.GetByIdAsync(taskManagement.TaskId))
            .ReturnsAsync(taskManagement);
        
        //Act
        var result = await _controller.GetTaskManagementByGuid(taskManagement.TaskId);

        //Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualResult = Assert.IsType<Result>(okResult.Value);
        Assert.True(actualResult.flag);
        Assert.Equal(200, actualResult.code);
        Assert.Equal("Find One Success", actualResult.message);
        Assert.IsType<TaskManagementDto>(actualResult.data);
        var returnedTasks = actualResult.data as TaskManagementDto;
        
        Assert.Equal("212bd5f9-d108-4dc4-9ef4-03fbe237ad74", returnedTasks?.TaskId.ToString());
        Assert.Equal("title1", returnedTasks?.Title);
        Assert.Equal("Open", returnedTasks?.Status);
        Assert.Equal("description1", returnedTasks?.Description);



    }

    [Fact]
    public async Task TestGetManagementByGuidNotFound()
    {
        //Arrange
        var nonExistingGuid = new Guid("8f99b8e4-9452-428f-8e1a-55339a827c24");
        
        //Mock
        _mockRepository.Setup(repo => 
                repo.GetByIdAsync(nonExistingGuid))
            .ReturnsAsync((TaskManagement)null);
        
        //Act
        var exception = await Assert
            .ThrowsAsync<TaskManagementNotFoundException>(() => _controller.GetTaskManagementByGuid(nonExistingGuid));
        
        //Assert
        Assert.Equal("Could not find TaskManagement with id " + nonExistingGuid, exception.Message);
        _mockRepository
            .Verify(repo => repo.GetByIdAsync(nonExistingGuid), Times.Once);
    }

    [Fact]
    public async Task TestCreateTaskManagementWithValidDataSuccess()
    {
        //Arrange
        
        //createRequestDto
        var createRequestTaskManagementDto = new CreateRequestTaskManagementDto();
        createRequestTaskManagementDto.Title = "title1";
        createRequestTaskManagementDto.Description = "Description1";
        createRequestTaskManagementDto.DueDate = DateTime.Now;
        createRequestTaskManagementDto.Status = "Open";
        createRequestTaskManagementDto.Priority = "low";
        
        //domain model that will be returned by the service class
        var taskManagementDomainModel = new TaskManagement();
        taskManagementDomainModel.TaskId = new Guid("32A2F5D4-4862-41B9-B7C7-7F9A3EA3DAE4");
        createRequestTaskManagementDto.Title = "title1";
        createRequestTaskManagementDto.Status = "Open";
        createRequestTaskManagementDto.Description = "description1";
        createRequestTaskManagementDto.DueDate = DateTime.Now;
        createRequestTaskManagementDto.Priority = "priority1";
        taskManagementDomainModel.TaskCategoryId = 100L;

        //taskManagementDto
        var taskManagementDto = new TaskManagementDto();
        taskManagementDto.TaskId = taskManagementDomainModel.TaskId;
        taskManagementDto.Title = taskManagementDomainModel.Title;
        taskManagementDto.Status = taskManagementDomainModel.Status;
        taskManagementDto.Description = taskManagementDomainModel.Description;
        taskManagementDto.DueDate = taskManagementDomainModel.DueDate;
        taskManagementDto.Priority = taskManagementDomainModel.Priority;
        taskManagementDto.TaskCategoryId = taskManagementDomainModel.TaskCategoryId;
        
        
        

        taskManagementDomainModel= _mapper.MapFromCreateRequestTaskManagementDtoToTaskManagement(createRequestTaskManagementDto);
        
        //Mock
         _mockRepository
             .Setup(repo => 
                 repo.CreateAsync(It.IsAny<TaskManagement>()))
             .ReturnsAsync(taskManagementDomainModel);
         
         //not really necessary
         taskManagementDto = _mapper.MapFromTaskMagtToTaskMagtDto(taskManagementDomainModel);
             
         

        //Act
        var result = await _controller.AddTaskManagement(createRequestTaskManagementDto);
        

        //Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualResult = Assert.IsType<Result>(okResult.Value);
        Assert.True(actualResult.flag);
        Assert.Equal(200, actualResult.code);
        Assert.Equal("Add One Success", actualResult.message);
        Assert.IsType<TaskManagementDto>(actualResult.data);
        var returnedTasks = actualResult.data as TaskManagementDto;
        Assert.NotNull(returnedTasks?.TaskId.ToString());
        Assert.Equal("title1", returnedTasks.Title);
        
    }

    [Fact]
    public async Task TestUpdateTaskManagementWithValidDataSuccess()
    {
        //Arrange
        var oldTaskManagement = new TaskManagement();
        oldTaskManagement.TaskId = Guid.NewGuid();
        oldTaskManagement.Title = "Report";
        oldTaskManagement.Status = "Pending";
        oldTaskManagement.Priority = "High";
        oldTaskManagement.Description = "Description 1";
        oldTaskManagement.DueDate = DateTime.Now.AddDays(-1);
        oldTaskManagement.TaskCategoryId = 100L;
        
        var updateRequestDto = new UpdateRequestTaskManagementDto();
        updateRequestDto.Title = "Report";
        updateRequestDto.Description = "Description1";
        updateRequestDto.Status = "Status1"; //upadted
        updateRequestDto.Priority = "High";
        updateRequestDto.DueDate = DateTime.Now; //upadted

        //Will be returned by the service class
        var updated = new TaskManagement();
        updated.TaskId = oldTaskManagement.TaskId;
        updated.Title = updateRequestDto.Title;
        updated.Status = updateRequestDto.Status;
        updated.Description = updateRequestDto.Description;
        updated.DueDate = updateRequestDto.DueDate;
        updated.Priority = updateRequestDto.Priority;
        updated.TaskCategoryId = 100L;

        oldTaskManagement = _mapper.MapFromUpdateRequestTaskManagementDtoToTaskManagement(updateRequestDto);
        
        //first find and then update
        //Mock
        _mockRepository
            .Setup(repo =>
                repo.GetByIdAsync(oldTaskManagement.TaskId)).ReturnsAsync(oldTaskManagement);
        
        

        //Mock
        _mockRepository.Setup(repo =>
                repo.UpdateAsync(oldTaskManagement.TaskId, It.IsAny<TaskManagement>()))
            .ReturnsAsync(updated);

        //Act
        var result = await _controller.UpdateTaskManagement(oldTaskManagement.TaskId, updateRequestDto);

        //Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualResult = Assert.IsType<Result>(okResult.Value);
        Assert.True(actualResult.flag);
        Assert.Equal(200, actualResult.code);
        Assert.Equal("Update Success", actualResult.message);
        Assert.IsType<TaskManagementDto>(actualResult.data);
        var returnedTasks = actualResult.data as TaskManagementDto;
        Assert.Equal("Report", returnedTasks?.Title); 
        Assert.Equal("Status1", returnedTasks?.Status);//updated
        Assert.Equal(updateRequestDto.DueDate.ToString(), returnedTasks?.DueDate.ToString());//updated
        
        
        
    }

    [Fact]
    public async Task TestUpdateTaskManagementWithNonExistingId()
    {
        //Arrange
        var nonExistingId = Guid.NewGuid();
        
        //createRequestDto
        var updateRequestTaskManagementDto = new UpdateRequestTaskManagementDto();
        updateRequestTaskManagementDto.Title = "title1";
        updateRequestTaskManagementDto.Description = "Description1";
        updateRequestTaskManagementDto.DueDate = DateTime.Now;
        updateRequestTaskManagementDto.Status = "Open";
        updateRequestTaskManagementDto.Priority = "low";

       TaskManagement NonExistingTaskManagement =  _mapper
           .MapFromUpdateRequestTaskManagementDtoToTaskManagement(updateRequestTaskManagementDto);

        //Mock
        
        _mockRepository
            .Setup(repo =>
                repo.GetByIdAsync(nonExistingId)).ReturnsAsync((TaskManagement)null);
        //Act
        var exception = await Assert
            .ThrowsAsync<TaskManagementNotFoundException>(() => _controller.GetTaskManagementByGuid(nonExistingId));

        //Assert
        //Assert
        Assert.Equal("Could not find TaskManagement with id " + nonExistingId, exception.Message);
        _mockRepository
            .Verify(repo => repo.GetByIdAsync(nonExistingId), Times.Once);
    }

    [Fact]
    public async Task TestDeleteTaskManagementWithValidId()
    {
        //Arrange
        //old data to be deleted
        var oldTaskManagement = new TaskManagement();
        oldTaskManagement.TaskId = Guid.NewGuid();
        oldTaskManagement.Title = "Report";
        oldTaskManagement.Status = "Pending";
        oldTaskManagement.Priority = "High";
        oldTaskManagement.Description = "Description 1";
        oldTaskManagement.DueDate = DateTime.Now.AddDays(-1);
        oldTaskManagement.TaskCategoryId = 100L;
        
        //Mock
        _mockRepository
            .Setup(repo =>
                repo.GetByIdAsync(oldTaskManagement.TaskId)).ReturnsAsync(oldTaskManagement);
        _mockRepository
            .Setup(repo =>
                repo.DeleteAsync(oldTaskManagement.TaskId)).Verifiable();
        //Act
        var result = await _controller.DeleteTaskManagement(oldTaskManagement.TaskId);
        
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
    
    [Fact]
    public async Task TestDeleteTaskManagementWithNonExistingId()
    {
        //Arrange
        //old data to be deleted
        var nonExistingId = Guid.NewGuid();
        
        //Mock
        _mockRepository
            .Setup(repo =>
                repo.GetByIdAsync(nonExistingId)).ReturnsAsync((TaskManagement)null);
        
        //Act
        var exception = await Assert
            .ThrowsAsync<TaskManagementNotFoundException>(() => _controller.DeleteTaskManagement(nonExistingId));
        
        //Assert
        Assert.Equal("Could not find TaskManagement with id " + nonExistingId, exception.Message);
        _mockRepository
            .Verify(repo => repo.GetByIdAsync(nonExistingId), Times.Once);
    }

    [Fact]
    public async Task TestGetAllTaskManagementByQueryObjSuccess()
    {
        //Arrange
        var queryObject = new TaskManagementQueryObject()
        {
            Title = "Sample",
            DueDate = DateTime.Now,
            Priority = "High",
            Status = "Open"
        };
        var tasksManagementList = new List<TaskManagement>
        {
            new TaskManagement
            {
                TaskId = new Guid("212bd5f9-d108-4dc4-9ef4-03fbe237ad74"),
                Title = "Sample Task1", 
                Status = "Open",
                Description = "description1",
                DueDate = DateTime.Now,
                Priority = "High",
                TaskCategory = new TaskCategory()
                {
                    Name = ""
                },
                TaskCategoryId = 100L
            },
            new TaskManagement
            {
                TaskId = new Guid("3fc52bc0-70d2-4301-89c1-f6cbd271cae8"),
                Title = "Sample Task2", 
                Status = "Close",
                Description = "description1 and description1",
                DueDate = DateTime.Now,
                Priority = "High",
                TaskCategory = new TaskCategory()
                {
                    Name = ""
                },
                TaskCategoryId = 200L
            }
        };
        
        //Mock
        _mockRepository
            .Setup(repo =>
                repo.GetAllByQueryAsync(queryObject))
            .ReturnsAsync(tasksManagementList);
        
        //Act
        var result = await _controller.GetAllTaskManagementByQueryObj(queryObject);
        
        //Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualResult = Assert.IsType<Result>(okResult.Value);
        Assert.True(actualResult.flag);
        Assert.Equal(200, actualResult.code);
        Assert.Equal("Find All Success", actualResult.message);
        Assert.IsType<List<TaskManagementDto>>(actualResult.data);
        if (actualResult.data is List<TaskManagementDto> returnedTasks)
        {
            Assert.All(returnedTasks, task => Assert.Contains("Sample", task.Title));
            Assert.Equal("212bd5f9-d108-4dc4-9ef4-03fbe237ad74", returnedTasks[0].TaskId.ToString());
            Assert.Equal("Sample Task1", returnedTasks?[0].Title);
            //Assert more as you wish
        }
    }
    
    [Fact]
    public async Task TestGetNonExistingTaskManagementByQueryObjSuccess()
    {
        //Arrange
        var queryObj = new TaskManagementQueryObject()
        {
            Title = "nonExistingTask"
        };
        
        //Mock
        
        _mockRepository.Setup(repo => 
                repo.GetAllByQueryAsync(queryObj))
            .ReturnsAsync((List<TaskManagement>)null);
        
        //Act
        var result = await _controller.GetAllTaskManagementByQueryObj(queryObj);
        
        //Assert
        Assert.NotNull(result);
        var okNotFoundObjectResultResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var actualResult = Assert.IsType<Result>(okNotFoundObjectResultResult.Value);
        
        Assert.False(actualResult.flag);
        Assert.Equal(404, actualResult.code);
        Assert.Equal("No TaskManagement matching the criteria.", actualResult.message);
        
    }
}