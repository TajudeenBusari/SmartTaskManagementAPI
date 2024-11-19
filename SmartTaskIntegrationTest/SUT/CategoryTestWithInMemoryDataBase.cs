using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SmartTaskIntegrationTest.Fixtures;
using SmartTaskIntegrationTest.Helper;
using SmartTaskManagementAPI.Data;
using SmartTaskManagementAPI.System;
using SmartTaskManagementAPI.TaskCategory.model;
using SmartTaskManagementAPI.TaskCategory.model.dto;
using SmartTaskManagementAPI.TaskManagement.model.dto;
using Xunit.Abstractions;

namespace SmartTaskIntegrationTest.SUT;

public class CategoryTestWithInMemoryDataBase: IClassFixture<CustomInMemoryWebApplicationFactory<Program>>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly CustomInMemoryWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    public CategoryTestWithInMemoryDataBase(CustomInMemoryWebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _factory = factory;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions()
        {
            AllowAutoRedirect = false
        });

        using (var scope = _factory.Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            //db.Database.Migrate(); //no need to use this to avoid creating double table
            SeedingDataForInMemoryDataBase.InitializeTestDb(db);
        }
    }
    
    
    
    [Fact]
    public async Task TestGetAllCategoriesSuccess()
    {
        var response = await _client.GetAsync(HttpHelper.Urls.GetAllCategories);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonString = await response.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(jsonString);
        Assert.True(deserializeObject?.flag);
        Assert.Equal(200, deserializeObject?.code);
        Assert.Equal("Find All Success", deserializeObject?.message);
        var jsonArray = deserializeObject?.data;
        _testOutputHelper.WriteLine(jsonArray?.ToString()); //all including the ones in the ApplicationDbContext class will be returned
        var taskManagementList = JsonConvert.DeserializeObject<List<TaskCategoryDto>>(jsonArray.ToString());
        taskManagementList.Should().BeOfType<List<TaskCategoryDto>>();
        
    }

    [Fact]
    public async Task AddCategorySuccess()
    {
        //Arrange
        var createRequest = new CreateRequestCategoryDto()
        {
            Name = "TestCategory",
            Description = "TestCategoryDescription",
        };
        
        //Act
        var response = await _client.PostAsJsonAsync(HttpHelper.Urls.AddCategory, createRequest);
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonString = await response.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(jsonString);
        Assert.True(deserializeObject?.flag);
        Assert.Equal(200, deserializeObject?.code);
        Assert.Equal("Add One Success", deserializeObject?.message);
        var responseData = deserializeObject?.data;
        _testOutputHelper.WriteLine(responseData?.ToString());
        var taskCategoryDto = JsonConvert.DeserializeObject<TaskCategoryDto>(responseData.ToString());
        Assert.NotNull(taskCategoryDto?.TaskCategoryId.ToString());
        Assert.Equal("TestCategory", taskCategoryDto.Name);
        Assert.Equal("TestCategoryDescription", taskCategoryDto.Description);
    }

    [Fact]
    public async Task UpdateCategorySuccess()
    {
        //Arrange
        const int taskCategoryId = 100;
        var updateRequest = new UpdateRequestCategoryDto()
        {
            Name = "Category1",
            Description = "Description1 updated",
        };
        
        //Act
        var response = await _client.PutAsJsonAsync(HttpHelper.Urls.UpdateCategoryById(taskCategoryId), updateRequest);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonString = await response.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(jsonString);
        Assert.True(deserializeObject?.flag);
        Assert.Equal(200, deserializeObject?.code);
        Assert.Equal("Update Success", deserializeObject?.message);
        var responseData = deserializeObject?.data;
        _testOutputHelper.WriteLine(responseData?.ToString());
        var categoryDto = JsonConvert.DeserializeObject<TaskCategoryDto>(responseData.ToString());
        Assert.Equal("Description1 updated", categoryDto?.Description);
    }

    [Fact]
    public async Task GetCategoryByIdSuccess()
    {
        //Arrange
        const long categoryId = 200L;
        
        //Act
        var response = await _client.GetAsync(HttpHelper.Urls.GetCategoryById(categoryId));
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonString = await response.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(jsonString);
        Assert.True(deserializeObject?.flag);
        Assert.Equal(200, deserializeObject?.code);
        Assert.Equal("Find One Success", deserializeObject?.message);
        _testOutputHelper.WriteLine(deserializeObject?.data.ToString());
    }

    [Fact]
    public async Task TestAssignCategorySuccess()
    {
        //Arrange
        var taskToBeAssigned = new CreateRequestTaskManagementDto()
        {
            Title = "title1",
            Description = "Description1",
            DueDate = DateTime.Now,
            Status = "Open",
            Priority = "low"
        };
        
        //Create task to be assigned
        var taskResponse = await _client.PostAsJsonAsync(HttpHelper.Urls.AddTaskManagement, taskToBeAssigned);
        taskResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonString = await taskResponse.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(jsonString);
        Assert.True(deserializeObject?.flag);
        Assert.Equal(200, deserializeObject?.code);
        Assert.Equal("Add One Success", deserializeObject?.message);
        var responseData = deserializeObject?.data;
        _testOutputHelper.WriteLine("Task to be assigned is : " + responseData);//printing task management to be assigned
        var taskManagementDto = JsonConvert.DeserializeObject<TaskManagementDto>(responseData.ToString());
        var taskId = taskManagementDto?.TaskId;
        _testOutputHelper.WriteLine(taskId.ToString());
        
        //Assign
        //category to assign the created task to
        var existingCategory = new TaskCategory()
        {
            TaskCategoryId= 100,
            Name = "Category1",
            Description = "Description1"
        };
        //Act
        //assert that taskId is not null in the request
        
        var categoryUrl = HttpHelper.Urls.AssignTaskToCategory(existingCategory.TaskCategoryId, new Guid(taskId.ToString()!));
        var response = await _client
            .PutAsync(categoryUrl, null);
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonReadAsStringAsyncString = await response.Content.ReadAsStringAsync();
        var deserializedObj = JsonConvert.DeserializeObject<Result>(jsonReadAsStringAsyncString);
        Assert.True(deserializedObj?.flag);
        Assert.Equal(200, deserializedObj?.code);
        Assert.Equal("Assignment Success", deserializedObj?.message);
        var resData = deserializedObj?.data;
        Assert.NotNull(resData);
        Assert.Equal("100", resData.ToString());
    }

    [Fact]
    public async Task TestDeleteCategorySuccess()
    {
        //Arrange
        var existingCategoryId = 100L;
        
        //Act
        var response = await _client.DeleteAsync(HttpHelper.Urls.DeleteCategory(existingCategoryId));

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonString = await response.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(jsonString);
        Assert.True(deserializeObject?.flag);
        Assert.Equal(200, deserializeObject?.code);
        Assert.Equal("Delete Success", deserializeObject?.message);
        var responseData = deserializeObject?.data;
        Assert.Null(responseData);
    }
   
}