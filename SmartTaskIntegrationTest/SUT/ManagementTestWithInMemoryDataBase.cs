using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SmartTaskIntegrationTest.Fixtures;
using SmartTaskIntegrationTest.Helper;
using SmartTaskManagementAPI.Data;
using SmartTaskManagementAPI.Exceptions.modelNotFound;
using SmartTaskManagementAPI.System;
using SmartTaskManagementAPI.TaskManagement.model.dto;
using Xunit.Abstractions;

namespace SmartTaskIntegrationTest.SUT;

public class ManagementTestWithInMemoryDataBase: IClassFixture<CustomInMemoryWebApplicationFactory<Program>>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly CustomInMemoryWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private string _token;

    public ManagementTestWithInMemoryDataBase(CustomInMemoryWebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    {
        //all these are run before each test case
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
            SeedingDataForInMemoryDataBase.InitializeTestDb(db);
        }
    }

    /// <summary>
    /// A method to authenticate and get a token
    /// </summary>
    private async Task AuthenticateAsync()
    {
        var loginRequest = new
        {
            Username = "Admin",
            Password = "Admin123!"
        };
        var content = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/v1/auth/login", content);
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var deserializedObject = JsonConvert.DeserializeObject<Result>(responseContent);
        _token = deserializedObject.data.ToString();
    }

    [Fact]
    public async Task TestGetAllTaskManagementSuccess()
    {
        //Arrange
        //Act
        var response = await _client.GetAsync(HttpHelper.Urls.GetAllTaskManagement);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonString = await response.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(jsonString);
        Assert.True(deserializeObject?.flag);
        Assert.Equal(200, deserializeObject?.code);
        Assert.Equal("Find All Success", deserializeObject?.message);
        var jsonArray = deserializeObject?.data;
        _testOutputHelper.WriteLine(jsonArray?.ToString());
        var taskManagementList = JsonConvert.DeserializeObject<List<TaskManagementDto>>(jsonArray.ToString());
        taskManagementList.Should().BeOfType<List<TaskManagementDto>>();
    }

    [Fact]
    public async Task TestGetTaskManagementByGuidSuccess()
    {
        //Arrange
        var existingTaskId = new Guid("c938a235-8a75-40d8-861f-b8e0f5498d6c");
        
        //Act
        var response = await _client.GetAsync(HttpHelper.Urls.GetTaskManagementById(existingTaskId));
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonString = await response.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(jsonString);
        Assert.True(deserializeObject?.flag);
        Assert.Equal(200, deserializeObject?.code);
        Assert.Equal("Find One Success", deserializeObject?.message);
        var responseData  = deserializeObject?.data;
        _testOutputHelper.WriteLine(responseData?.ToString());
    }

    [Fact]
    public async Task TestGetTaskManagementByNonExistingGuidFailure()
    {
        //Arrange
        var nonExistingGuid = new Guid("f9ead0ff-ee99-4016-ad6c-9d1b7bc4e2c2");
        //Act
        var response = await _client.GetAsync(HttpHelper.Urls.GetTaskManagementById(nonExistingGuid));
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var jsonString = await response.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(jsonString);
        Assert.False(deserializeObject?.flag);
        Assert.Equal(404, deserializeObject?.code);
        var exception = new TaskManagementNotFoundException(nonExistingGuid);
        Assert.Equal("Could not find TaskManagement with id " + nonExistingGuid, exception.Message);
        var responseDeserializeObjectData = deserializeObject?.data;
        Assert.Null(responseDeserializeObjectData);
        
    }

    [Fact]
    public async Task TestAddTaskManagementSuccess()
    {
        if (string.IsNullOrEmpty(_token))
        {
            await AuthenticateAsync();
        }
        // Set the Authorization header for the client
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        
        //if condition for data model is not fulfilled, it returned a bad request
        //Arrange
        var createRequest = new CreateRequestTaskManagementDto()
        {
            Title = "New Task1",
            Description = "New Description1",
            Priority = "low",
            Status = "Active",
            DueDate = DateTime.Now
        };
        
        //Act
        var response = await _client.PostAsJsonAsync(HttpHelper.Urls.AddTaskManagement, createRequest);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonString = await response.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(jsonString);
        Assert.True(deserializeObject?.flag);
        Assert.Equal(200, deserializeObject?.code);
        Assert.Equal("Add One Success", deserializeObject?.message);
        var responseData = deserializeObject?.data;
        _testOutputHelper.WriteLine(responseData?.ToString());
        var taskManagementDto = JsonConvert.DeserializeObject<TaskManagementDto>(responseData.ToString());
        Assert.NotNull(taskManagementDto?.TaskId.ToString());
        Assert.Equal(createRequest.Title, taskManagementDto.Title);
        Assert.Equal(createRequest.Description, taskManagementDto.Description);
        Assert.Equal(createRequest.Status, taskManagementDto.Status);
        Assert.Equal(createRequest.Priority, taskManagementDto.Priority);
    }

    [Fact]
    public async Task TestUpdateTaskManagementSuccess()
    {
        if (string.IsNullOrEmpty(_token))
        {
            await AuthenticateAsync();
        }
        // Set the Authorization header for the client
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        //if condition for data model is not fulfilled, it returned a bad request
        //Arrange
        var existingTaskIdTobeUpdated = new Guid("c938a235-8a75-40d8-861f-b8e0f5498d6c");
        
        /***
         * {
              "taskId": "c938a235-8a75-40d8-861f-b8e0f5498d6c",
              "title": "Title1",
              "description": "Description1",
              "dueDate": "2024-11-17T10:38:24.5936703",
              "status": "Status1",
              "priority": "Priority1", //update will only take at most 6 xters
              "taskCategoryId": 400
}
         */
        
        var updateRequest = new UpdateRequestTaskManagementDto()
        {
            Title = "Title1",
            Description = "Description1 updated", //updated
            Priority = "low", //updated
            DueDate = DateTime.Now,
            Status = "Status1"
        };

        //Act
        var response = await _client.PutAsJsonAsync(HttpHelper.Urls.UpdateTaskManagement(existingTaskIdTobeUpdated), updateRequest);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonString = await response.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(jsonString);
        Assert.True(deserializeObject?.flag);
        Assert.Equal(200, deserializeObject?.code);
        Assert.Equal("Update Success", deserializeObject?.message);
        var responseData = deserializeObject?.data;
        _testOutputHelper.WriteLine(responseData?.ToString());
        var taskManagementDto = JsonConvert.DeserializeObject<TaskManagementDto>(responseData.ToString());
        Assert.NotNull(taskManagementDto?.TaskId.ToString());
        Assert.Equal(updateRequest.Title, taskManagementDto.Title);
        Assert.Equal(updateRequest.Priority, taskManagementDto.Priority);
        Assert.Equal(updateRequest.Status, taskManagementDto.Status);

    }

    [Fact]
    public async Task TestUpdateTaskManagementWithInvalidDataFail()
    {
        if (string.IsNullOrEmpty(_token))
        {
            await AuthenticateAsync();
        }
        // Set the Authorization header for the client
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        //Arrange
        var existingTaskIdTobeUpdated = new Guid("c938a235-8a75-40d8-861f-b8e0f5498d6c");
        
        var updateRequest = new UpdateRequestTaskManagementDto()
        {
            Title = "Title1",
            Description = "Description1 updated", //updated
            Priority = "lowwwwwwwwww", //updated with invalid data
            DueDate = DateTime.Now,
            Status = "Status1"
        };
        
        //Act
        var response = await _client.PutAsJsonAsync(HttpHelper.Urls.UpdateTaskManagement(existingTaskIdTobeUpdated), updateRequest);
        
        //Assert
        //if condition for data model is not fulfilled, it returned a bad request
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task TestDeleteTaskManagementSuccess()
    {
        if (string.IsNullOrEmpty(_token))
        {
            await AuthenticateAsync();
        }
        // Set the Authorization header for the client
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        
        //Arrange
        var existingTaskIdTobeDeleted = new Guid("c938a235-8a75-40d8-861f-b8e0f5498d6c");
        
        //Act
        var response = await _client.DeleteAsync(HttpHelper.Urls.DeleteTaskManagement(existingTaskIdTobeDeleted));
        
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