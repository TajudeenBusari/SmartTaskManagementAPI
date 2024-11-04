using System.Data.OleDb;
using FluentAssertions;
using JetBrains.Annotations;
using Moq;
using SmartTaskManagementAPI.Exceptions.modelNotFound;
using SmartTaskManagementAPI.TaskCategory.repository;
using SmartTaskManagementAPI.TaskCategory.service;
using SmartTaskManagementAPI.TaskManagement.repository;

namespace SmartTaskManagementAPITest.TaskCategory.service;
using SmartTaskManagementAPI.TaskCategory.model;
using SmartTaskManagementAPI.TaskManagement.model;


public class TaskCategoryServiceTest: IDisposable
{
    private readonly TaskCategoryService _categoryService;

    private readonly Mock<ITaskCategoryRepository> _mockCategoryRepository;

    private readonly Mock<ITaskManagementRepository> _mockTaskRepository;

    private readonly List<TaskCategory> _categoryList;
    
    // Setup method (runs before each test)
    public TaskCategoryServiceTest()
    {
        // Initialize repository and service with the context
        
        _mockCategoryRepository = new Mock<ITaskCategoryRepository>();
        
        _mockTaskRepository = new Mock<ITaskManagementRepository>();
        
        //Mock<ITaskManagementRepository> mockManagementRepository = new();
        //_categoryService = new TaskCategoryService(_mockCategoryRepository.Object, mockManagementRepository.Object);
        
        _categoryService = new TaskCategoryService(_mockCategoryRepository.Object, _mockTaskRepository.Object);
       
        
        _categoryList = new List<TaskCategory>()
        {
            new TaskCategory()
            {
                TaskCategoryId = 100L,
                Description = "Description1",
                Name = "Name1",
                Tasks = new List<TaskManagement>()
            },
            new TaskCategory()
            {
                TaskCategoryId = 200L,
                Description = "Description2",
                Name = "Name2",
                Tasks = new List<TaskManagement>()
            },
            new TaskCategory()
            {
                TaskCategoryId = 300L,
                Description = "Description3",
                Name = "Name3",
                Tasks = new List<TaskManagement>()
            }

        };
    }

    [Fact]
    public async Task TestGetAllTaskCategorySuccess()
    {
        //Arrange
        //List is gotten from set up method above
        
        //Mock
        _mockCategoryRepository
            .Setup(repo => repo.GetAllTaskCategory())
            .ReturnsAsync(_categoryList);

        //Act
        var result = await _categoryService.GetAllAsync();

        //Assert
        Assert.NotNull(result);
        Assert.Equal("100", result[0].TaskCategoryId.ToString());
        Assert.Equal("200", result[1].TaskCategoryId.ToString());
        Assert.Equal("Description1", result[0].Description);
        Assert.Equal("Description2", result[1].Description);
        
        _mockCategoryRepository
            .Verify(repo => repo.GetAllTaskCategory(), Times.Once);
    }

    
    [Fact]
    public async Task TestGetTaskCategoryByIdSuccess()
    {
        //Arrange
        
        //Mock
        _mockCategoryRepository
            .Setup(repo =>
                repo.GetByIdAsync(_categoryList[0].TaskCategoryId))
            .ReturnsAsync(_categoryList[0]);
        
        //Act
        var result = await _categoryService.GetByIdAsync(_categoryList[0].TaskCategoryId);
        
        //Assert
        Assert.NotNull(result);
        Assert.Equal("100", result.TaskCategoryId.ToString());
        Assert.Equal("Description1", result.Description);
        Assert.Equal("Name1", result.Name);
        _mockCategoryRepository
            .Verify(repo => repo.GetByIdAsync(_categoryList[0].TaskCategoryId), Times.Once);
    }

    [Fact]
    public async Task TestGetTaskCategoryByNonExistingIdFailure()
    {
        //Arrange
        var nonExistingId = 300L;
        
        //Mock
        _mockCategoryRepository
            .Setup(repo => repo.GetByIdAsync(nonExistingId))
            .ReturnsAsync((TaskCategory)null);

        //Act
        var exception = await Assert
            .ThrowsAsync<TaskCategoryNotFoundException>(
                () => _categoryService.GetByIdAsync(nonExistingId));
        
        //Assert
        Assert.Equal("Could not find TaskCategory with id " + nonExistingId, exception.Message);
        _mockCategoryRepository
            .Verify(repo => repo.GetByIdAsync(nonExistingId), Times.Once);
    }
    
    
    [Fact]
    public async Task TestAddTaskCategorySuccess()
    {
        //Arrange
        var taskCategory = new TaskCategory()
        {
            TaskCategoryId = 100L,
            Description = "Description1",
            Name = "Name1",
            Tasks = new List<TaskManagement>()
        };
        //Mock
        _mockCategoryRepository
            .Setup(repo =>
                repo.CreateAsync(taskCategory))
            .ReturnsAsync(taskCategory);

        //Act
        var result = await _categoryService.CreateAsync(taskCategory);

        //Assert
        Assert.NotNull(result);
        _mockCategoryRepository
            .Verify(repo => repo.CreateAsync(taskCategory), Times.Once);
    }
    
    [Fact]
    public async Task TestUpdateTaskCategorySuccess()
    {
        //Arrange
        //old category
        const long taskCategoryId = 100L;
        var oldCategory = new TaskCategory()
        {
            TaskCategoryId = taskCategoryId,
            Description = "Description1",
            Name = "Name1",
        };
        
        //update
        var updated = new TaskCategory()
        {
            Description = oldCategory.Description,
            Name = "Name2",//updated

        };

        //first find and then update
        //Mock
        _mockCategoryRepository
            .Setup(repo =>
                repo.GetByIdAsync(taskCategoryId)).ReturnsAsync(oldCategory);

        _mockCategoryRepository
            .Setup(repo =>
                repo.UpdateAsync(taskCategoryId, It.IsAny<TaskCategory>()))
            .ReturnsAsync(updated);
        
        
        //Act
        var result = await _categoryService
            .UpdateByIdAsync(taskCategoryId, updated);
        
        //Assert
        Assert.NotNull(result);
        //Assert.Equal(updated.Name, result.Name);
        Assert.Equal(result.Description, result.Description);
        
        _mockCategoryRepository
            .Verify(repo =>
                repo.GetByIdAsync(oldCategory.TaskCategoryId), Times.Once);
        
        _mockCategoryRepository
            .Verify(repo =>
                repo.UpdateAsync(oldCategory.TaskCategoryId, It.IsAny<TaskCategory>()), Times.Once);
    }

    [Fact]
    public async Task TestUpdateTaskCategoryWithNonExistingIdFailure()
    {
        //Arrange
        const long nonExistingId = 600L;
        
        //Mock
        _mockCategoryRepository
            .Setup(repo => 
                repo.GetByIdAsync(nonExistingId)).ReturnsAsync((TaskCategory)null);
        //Act
        var exception = await Assert
            .ThrowsAsync<TaskCategoryNotFoundException>(() =>
            _categoryService.UpdateByIdAsync(nonExistingId, new TaskCategory()));
        //Assert
        Assert.Equal("Could not find TaskCategory with id " + nonExistingId, exception.Message);
    }
    
    [Fact]
    public async Task TestDeleteCategoryByIdSuccess()
    {
        //Arrange
        //old category
        var oldCategory = new TaskCategory()
        {
            TaskCategoryId = 100L,
            Description = "Description1",
            Name = "Name1",
            Tasks = new List<TaskManagement>()
            {
                new TaskManagement()
                {
                   TaskId = new Guid("52c5323f-ec72-4199-9906-ef3466b1f78d"),
                    Title = "Report",
                    Status = "Pending",
                    Priority = "High",
                    Description = "Description 1",
                    DueDate = DateTime.Now.AddDays(-1),
                    TaskCategoryId = 100L
                }
            }

        };

        //Mock
        _mockCategoryRepository
            .Setup(repo =>
                repo.GetByIdAsync(oldCategory.TaskCategoryId)).ReturnsAsync(oldCategory);
        _mockCategoryRepository.Setup(repo 
            => repo.DeleteAsync(oldCategory.TaskCategoryId)).Verifiable(); // Verifiable allows us to check if this method was called
        //Act
        await _categoryService.DeleteByIdAsync(oldCategory.TaskCategoryId);
        
        //Assert
        _mockCategoryRepository.Verify(repo =>
            repo.DeleteAsync(oldCategory.TaskCategoryId), Times.Once);

    }

    [Fact]
    public async Task TestDeleteCategoryByNonExistingIdFailure()
    {
        //Arrange
        //old task
        const long nonExistingId = 700L;


        //Mock
        //first find and then Delete
        //Mock
        _mockCategoryRepository
            .Setup(repo =>
                repo.GetByIdAsync(nonExistingId)).ReturnsAsync((TaskCategory)null);
        
        //Act
        var exception = await Assert
            .ThrowsAsync<TaskCategoryNotFoundException>(
                () => _categoryService.DeleteByIdAsync(nonExistingId));
        //Assert
        Assert.Equal("Could not find TaskCategory with id " + nonExistingId, exception.Message);
        
        _mockCategoryRepository
            .Verify(repo => 
                repo.GetByIdAsync(nonExistingId), Times.Once);
    }

    [Fact]
    public async Task TestAssignCategorySuccess()
    {
        //Arrange
        //the goal is now to assign the taskManagement that originally belong to category1 to category2
        
        var taskId = Guid.NewGuid(); //taskId to be assigned

        //original category holding the taskManagement
        var category1 = new TaskCategory()
        {
            TaskCategoryId = 100L,
            
        };
        
        //taskManagement to be assigned
       var  taskManagement = new TaskManagement()
        {
            TaskId = taskId,
            Title = "Report",
            Status = "Pending",
            Priority = "High",
            Description = "Description 1",
            DueDate = DateTime.Now.AddDays(-1),
            TaskCategoryId = 100L

        };
        category1.AddTaskManagement(taskManagement);

        //new category to which the task needs to be reassigned to
        var category2 = new TaskCategory()
        {
            TaskCategoryId = 200L,
            
        };
        

        //Mock
        //first find if they exist
        //I will focus only on the happy part
        _mockTaskRepository
            .Setup(repo =>
                repo.GetByIdAsync(taskManagement.TaskId))
            .ReturnsAsync(taskManagement);

        _mockCategoryRepository
            .Setup(repo => repo.GetByIdAsync(category2.TaskCategoryId))
            .ReturnsAsync(category2);
        
        
        _mockCategoryRepository
            .Setup(repo => 
                repo.AssignTaskManagementToCategoryAsync(category2.TaskCategoryId, taskManagement.TaskId))
            .Callback(() => taskManagement.TaskCategory = category2) // Update the TaskCategory property
            .Returns(Task.CompletedTask);

        //Act
        await  _categoryService
            .AssignTaskManagementAsync(200, taskManagement.TaskId);
        //Assert
        Assert.Equal(taskManagement.TaskCategory?.TaskCategoryId.ToString(), category2.TaskCategoryId.ToString());
        //category2.Tasks.Should().Contain(taskManagement);

    }


    public void Dispose()
    {
        //IF YOU ARE USING IN MEMORY DB
        // Clean up in-memory database by disposing context
        //_dbContext.Dispose();
    }





    
}