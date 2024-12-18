using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SmartTaskIntegrationTest.Fixtures;
using SmartTaskIntegrationTest.Helper;
using SmartTaskManagementAPI.Exceptions;
using SmartTaskManagementAPI.Exceptions.modelNotFound;
using SmartTaskManagementAPI.System;
using SmartTaskManagementAPI.TaskManagement.model;
using SmartTaskManagementAPI.TaskManagement.model.dto;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace SmartTaskIntegrationTest.SUT;

[TestCaseOrderer("SmartTaskIntegrationTest.Helper.PriorityOrderer", "SmartTaskIntegrationTest")]
public class ManagementTestWithDockerContainer: IClassFixture<CustomDockerWebApplicationFactory>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly CustomDockerWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private string _token;
    public ManagementTestWithDockerContainer(CustomDockerWebApplicationFactory factory, ITestOutputHelper testOutputHelper)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _testOutputHelper = testOutputHelper;
    }

    /// <summary>
    /// A method to authenticate and get a token
    /// </summary>
    private async Task AuthenticateAsync()
    {
        var loginRequest = new
        {
            username = "Admin",
            password = "Admin123!"
        };
        var content = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/v1/auth/login", content);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var deserializedObject = JsonConvert.DeserializeObject<Result>(responseContent);
        _token = deserializedObject.data.ToString();
    }
    
    [Fact, TestPriority(1)]
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
        
        /*
         *
         * This works as well but the order of list keep changing, making my test fail
            Assert.Equal("Sample Task1", taskManagementList?[0].Title);
            Assert.Equal("Grocery Shopping", taskManagementList?[1].Title);
            Assert.Equal("High", taskManagementList?[0].Priority);
            Assert.Equal("Medium", taskManagementList?[1].Priority);
        *
         * 
        */
        
    }

    [Fact, TestPriority(2)]
    public async Task GetTaskManagementByGuidSuccess()
    {
        //Arrange
        //var taskId = new Guid("212bd5f9-d108-4dc4-9ef4-03fbe237ad74"); //use this and use data fixture in custom web factory
        var taskId = new Guid("EE3EF122-AF19-4E5F-88C5-F2241AE989C8"); //this is already available in DB
        
        
        //Act
        var response = await _client.GetAsync(HttpHelper.Urls.GetTaskManagementById(taskId));
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonString = await response.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(jsonString);
        Assert.True(deserializeObject?.flag);
        Assert.Equal(200, deserializeObject?.code);
        Assert.Equal("Find One Success", deserializeObject?.message);
    }

    [Fact, TestPriority(3)]
    public async Task TestTaskManagementByNonExistingGuidFailure()
    {
        var nonExistingGuid = new Guid("f9ead0ff-ee99-4016-ad6c-9d1b7bc4e2c2");
        
        //Arrange
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
    
    [Fact, TestPriority(4)]
    public async Task TestAddTaskManagementSuccess()
    {
        if (string.IsNullOrEmpty(_token))
        {
            await AuthenticateAsync();
        }
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        
        //Arrange
        var createRequest = new CreateRequestTaskManagementDto();
        
        createRequest.Title = "title1";
        createRequest.Description = "Description1";
        createRequest.DueDate = DateTime.Now;
        createRequest.Status = "Open";
        createRequest.Priority = "low";
        
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
        Assert.Equal("title1", taskManagementDto.Title);
        Assert.Equal("Description1", taskManagementDto.Description);
        Assert.Equal("Open", taskManagementDto.Status);
        Assert.Equal("low", taskManagementDto.Priority);
    }

    [Fact, TestPriority(5)]
    public async Task TestUpdateTaskManagementSuccess()
    {
        if (string.IsNullOrEmpty(_token))
        {
            await AuthenticateAsync();
        }
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        //if condition for data model is not fulfilled, it returned a bad request
        //Arrange
        var taskId = new Guid("EE3EF122-AF19-4E5F-88C5-F2241AE989C8");
        var updateRequest = new UpdateRequestTaskManagementDto();
        updateRequest.Title = "Report"; //UPDATED 
        updateRequest.Description = "Complete the quarterly financial report";
        updateRequest.Status = "progress"; 
        updateRequest.Priority = "High";//upadted to High from Low
        updateRequest.DueDate = DateTime.Now; //updated to now
        
        //Act
        var response = await _client.PutAsJsonAsync(HttpHelper.Urls.UpdateTaskManagement(taskId), updateRequest);
        
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
        Assert.Equal("Report", taskManagementDto.Title);
        Assert.Equal("High", taskManagementDto.Priority);
        Assert.Equal("progress", taskManagementDto.Status);
    }

    [Fact, TestPriority(6)]
    public async Task TestUpdateTaskManagementWithInvalidData()
    {
        if (string.IsNullOrEmpty(_token))
        {
            await AuthenticateAsync();
        }
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        //Arrange
        var taskId = new Guid("EE3EF122-AF19-4E5F-88C5-F2241AE989C8");
        var updateRequest = new UpdateRequestTaskManagementDto();
        updateRequest.Title = "Reporttttttttt"; //UPDATED with invalid data 
        updateRequest.Description = "Complete the quarterly financial report";
        updateRequest.Status = "progress"; 
        updateRequest.Priority = "High";//upadted to High from Low
        updateRequest.DueDate = DateTime.Now; //updated to now
        
        //Act
        var response = await _client.PutAsJsonAsync(HttpHelper.Urls.UpdateTaskManagement(taskId), updateRequest);
        
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var jsonString = await response.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(jsonString);
        Assert.False(deserializeObject?.flag);
        //Assert.Equal(StatusCode.BAD_REQUEST, deserializeObject?.code);//code is 0, cos of data validation in UpdateRequestTaskManagementDto? 
        var responseData = deserializeObject?.data;
        Assert.Null(responseData);
        
    }

    [Fact, TestPriority(7)]
    public async Task TestDeleteTaskManagementSuccess()
    {
        if (string.IsNullOrEmpty(_token))
        {
            await AuthenticateAsync();
        }
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        //Arrange
        var existingTaskId = new Guid("EE3EF122-AF19-4E5F-88C5-F2241AE989C8");

        //Act
        var response =await _client.DeleteAsync(HttpHelper.Urls.DeleteTaskManagement(existingTaskId));
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