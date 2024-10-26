

using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Moq;
using SmartTaskManagementAPI.Data;
using SmartTaskManagementAPI.Exceptions.modelNotFound;
using SmartTaskManagementAPI.TaskCategory.model;
using SmartTaskManagementAPI.TaskManagement.model.Objects;
using SmartTaskManagementAPI.TaskManagement.repository;
using SmartTaskManagementAPI.TaskManagement.service;


namespace SmartTaskManagementAPITest.TaskManagement.service;
using SmartTaskManagementAPI.TaskManagement.model;

public class TaskManagementServiceTest: IDisposable
{
    private readonly TaskManagementService _taskManagementService; //service under test
    private readonly Mock<ITaskManagementRepository> _mocktaskManagementRepository;
    
    

    // Setup method (runs before each test)
    public TaskManagementServiceTest()
    {
        /*var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;*/
        
        
        // Initialize repository and service with the context
        _mocktaskManagementRepository = new Mock<ITaskManagementRepository>();
        _taskManagementService = new TaskManagementService(_mocktaskManagementRepository.Object);


    }


    // Teardown method (runs after each test)
    public void Dispose()
    {
        // Clean up in-memory database by disposing context
        //_dbContext.Dispose();
        
    }
    
    [Fact]
    public async Task TestGetAllTaskManagementSuccess()
    {
        //Arrange
        var tasksManagementList = new List<TaskManagement>
        {
            new TaskManagement
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
            new TaskManagement
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
        
        // Mock the repository to return the tasks list
        _mocktaskManagementRepository
            .Setup(repo => repo.GetAllAsync()).ReturnsAsync(tasksManagementList);
       
        
        //Act
        var result = await _taskManagementService.GetAllAsync();
        
        //Assert
        Assert.NotNull(result);
        Assert.Equal("212bd5f9-d108-4dc4-9ef4-03fbe237ad74", result[0].TaskId.ToString());
        Assert.Equal("3fc52bc0-70d2-4301-89c1-f6cbd271cae8", result[1].TaskId.ToString());
        Assert.Equal("title1", result[0].Title);
        Assert.Equal("title2", result[1].Title);
        _mocktaskManagementRepository
            .Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task TestGetTaskManagementByQuerySuccess()
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
            //Arrange
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
        
        // Mock the repository to return the tasks list
        _mocktaskManagementRepository
            .Setup(repo => repo.GetAllByQueryAsync(queryObject))
            .ReturnsAsync(tasksManagementList);
        //Act
        var result = await _taskManagementService.GetAllByQueryAsync(queryObject);
        
        //Assert
        Assert.NotNull(result);
        Assert.Equal("212bd5f9-d108-4dc4-9ef4-03fbe237ad74", result[0].TaskId.ToString());
        Assert.Equal("3fc52bc0-70d2-4301-89c1-f6cbd271cae8", result[1].TaskId.ToString());
        Assert.Equal(tasksManagementList.Count, result.Count);
        Assert.All(result, task => Assert.Contains("Sample", task.Title));
        Assert.All(result, task => Assert.Equal("High", task.Priority));
        
        // Verify the repository was called once with the queryObject
        _mocktaskManagementRepository
            .Verify(repo => repo.GetAllByQueryAsync(queryObject), Times.Once);

    }

    [Fact]
    public async Task TestGetTaskManagementByQueryNotFound()
    {
        //Arrange
        var queryObj = new TaskManagementQueryObject()
        {
            Title = "nonExistingTask"
        };
        
        //Mock
        _mocktaskManagementRepository
            .Setup(repo => 
                repo.GetAllByQueryAsync(queryObj))
            .ReturnsAsync((List<TaskManagement>?)null);
        
        //Act
        var result = await _taskManagementService.GetAllByQueryAsync(queryObj);
        
        //Assert
        Assert.Null(result);
        _mocktaskManagementRepository.Verify(repo => repo.GetAllByQueryAsync(queryObj));

    }
    [Fact]
    public async Task TestGetTaskManagementByIdSuccess()
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
        _mocktaskManagementRepository
            .Setup(repo => 
                repo.GetByIdAsync(taskManagement.TaskId))
            .ReturnsAsync(taskManagement);
        
        //Act
        var result = await _taskManagementService.GetByIdAsync(taskManagement.TaskId);
        
        //Assert
        Assert.NotNull(result);
        Assert.Equal("212bd5f9-d108-4dc4-9ef4-03fbe237ad74", result.TaskId.ToString());
        _mocktaskManagementRepository
            .Verify(repo => repo.GetByIdAsync(taskManagement.TaskId), Times.Once);

    }
    

    [Fact]
    public async Task TestGetTaskManagementByIdNotFoundFailure()
    {
        //Arrange
        var nonExistingTaskId = new Guid("a21f806e-89e8-4a43-aef9-95d421ef0664");
        //Mock
        _mocktaskManagementRepository
            .Setup(repo => 
                repo.GetByIdAsync(nonExistingTaskId))
            .ReturnsAsync((TaskManagement)null);

        //Act
        var exception = await Assert
            .ThrowsAsync<TaskManagementNotFoundException>(
                () => _taskManagementService.GetByIdAsync(nonExistingTaskId));
        //Assert
        Assert.Equal("Could not find TaskManagement with id " + nonExistingTaskId, exception.Message);
        _mocktaskManagementRepository
            .Verify(repo => repo.GetByIdAsync(nonExistingTaskId), Times.Once);

    }

    [Fact]
    public async Task TestCreateTaskManagementWithValidDataSuccess()
    {
        //Arrange
        var taskManagement = new TaskManagement();
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
        _mocktaskManagementRepository
            .Setup(repo =>
                repo.CreateAsync(taskManagement)).ReturnsAsync(taskManagement);
        
        //Act
        var result = await _taskManagementService.CreateAsync(taskManagement);
        
        //Assert
        Assert.NotNull(result);
        Assert.Equal(taskManagement.TaskId.ToString(), result.TaskId.ToString());
        Assert.Equal(taskManagement.Title, result.Title);
        Assert.Equal(taskManagement.Status, result.Status);
        Assert.Equal(taskManagement.Description, result.Description);
        Assert.Equal(taskManagement.DueDate.ToString(), result.DueDate.ToString());
        Assert.Equal(taskManagement.Priority, result.Priority);
        
        _mocktaskManagementRepository
            .Verify(repo => repo.CreateAsync(taskManagement), Times.Once);
    }

    [Fact]
    public async Task TestUpdateTaskManagementWithValidDataSuccess()
    {
        //Arrange
        //old task
        var oldTaskManagement = new TaskManagement();
        oldTaskManagement.TaskId = new Guid("52c5323f-ec72-4199-9906-ef3466b1f78d");
        oldTaskManagement.Title = "Report";
        oldTaskManagement.Status = "Pending";
        oldTaskManagement.Priority = "High";
        oldTaskManagement.Description = "Description 1";
        oldTaskManagement.DueDate = DateTime.Now.AddDays(-1);

        var updateTaskManagement = new TaskManagement();
        oldTaskManagement.TaskId = new Guid("52c5323f-ec72-4199-9906-ef3466b1f78d");
        updateTaskManagement.Title = "Audit";
        updateTaskManagement.Status = "Completed";
        updateTaskManagement.Priority = "High";
        updateTaskManagement.Description = "Description 1";
        updateTaskManagement.DueDate = DateTime.Now.AddDays(7);
        
        //first find and then update
        //Mock
        _mocktaskManagementRepository
            .Setup(repo =>
                repo.GetByIdAsync(oldTaskManagement.TaskId)).ReturnsAsync(oldTaskManagement);
       
        
        
        _mocktaskManagementRepository
            .Setup(repo =>
                repo.UpdateAsync(oldTaskManagement.TaskId, It.IsAny<TaskManagement>()))
            .ReturnsAsync(updateTaskManagement);
        
        //Act
        var result = await _taskManagementService
            .UpdateByIdAsync(oldTaskManagement.TaskId, updateTaskManagement);
        
        //Assert
        Assert.NotNull(result);
        
        _mocktaskManagementRepository
            .Verify(repo =>
                repo.GetByIdAsync(oldTaskManagement.TaskId), Times.Once);
        
        _mocktaskManagementRepository
            .Verify(repo =>
                repo.UpdateAsync(oldTaskManagement.TaskId, It.IsAny<TaskManagement>()), Times.Once);

    }

    [Fact]
    public async Task TestUpdateTaskManagementWithNonExistingId()
    {
        //Arrange
        var nonExistingId = Guid.NewGuid();
        
        //Mock
        _mocktaskManagementRepository
            .Setup(repo => repo.GetByIdAsync(nonExistingId))
            .ReturnsAsync((TaskManagement)null);
        //Act
        var exception = await Assert.ThrowsAsync<TaskManagementNotFoundException>(
            () => _taskManagementService.UpdateByIdAsync(nonExistingId, new TaskManagement()));
        //Assert
        Assert.Equal("Could not find TaskManagement with id " + nonExistingId, exception.Message);
    }

    [Fact]
    public async Task TestDeleteTaskManagementWithExistingIdSuccess()
    {
        //Arrange
        //old task
        var oldTaskManagement = new TaskManagement();
        oldTaskManagement.TaskId = new Guid("52c5323f-ec72-4199-9906-ef3466b1f78d");
        oldTaskManagement.Title = "Report";
        oldTaskManagement.Status = "Pending";
        oldTaskManagement.Priority = "High";
        oldTaskManagement.Description = "Description 1";
        oldTaskManagement.DueDate = DateTime.Now.AddDays(-1);
        oldTaskManagement.TaskCategoryId = 100L;
        
        //Mock
        //first find and then Delete
        //Mock
        _mocktaskManagementRepository
            .Setup(repo =>
                repo.GetByIdAsync(oldTaskManagement.TaskId)).ReturnsAsync(oldTaskManagement);
        
        
        _mocktaskManagementRepository
            .Setup(repo =>
                repo.DeleteAsync(oldTaskManagement.TaskId)).Verifiable(); // Verifiable allows us to check if this method was called
        
        //Act
        await _taskManagementService.DeleteByIdAsync(oldTaskManagement.TaskId);
        
        //Assert
        _mocktaskManagementRepository
            .Verify(repo => 
                repo.DeleteAsync(oldTaskManagement.TaskId), Times.Once);
    }
    
    [Fact]
    public async Task TestDeleteTaskManagementWithNonExistingIdFailure()
    {
        //Arrange
        //old task
        var nonExistingId = Guid.NewGuid();
       
        
        //Mock
        //first find and then Delete
        //Mock
        _mocktaskManagementRepository
            .Setup(repo =>
                repo.GetByIdAsync(nonExistingId)).ReturnsAsync((TaskManagement)null);
        
        
        //Act
        var exception = await Assert
            .ThrowsAsync<TaskManagementNotFoundException>(
                () => _taskManagementService.DeleteByIdAsync(nonExistingId));
        
        //Assert
        Assert.Equal("Could not find TaskManagement with id " + nonExistingId, exception.Message);
        _mocktaskManagementRepository
            .Verify(repo => 
                repo.GetByIdAsync(nonExistingId), Times.Once);
        
        
    }

    
}