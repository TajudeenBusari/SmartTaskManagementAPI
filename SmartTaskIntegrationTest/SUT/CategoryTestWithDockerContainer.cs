using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Newtonsoft.Json;
using SmartTaskIntegrationTest.Fixtures;
using SmartTaskIntegrationTest.Helper;
using SmartTaskManagementAPI.System;
using SmartTaskManagementAPI.TaskCategory.model;
using SmartTaskManagementAPI.TaskCategory.model.dto;
using SmartTaskManagementAPI.TaskManagement.model.dto;
using Xunit.Abstractions;

namespace SmartTaskIntegrationTest.SUT;

public class CategoryTestWithDockerContainer: IClassFixture<CustomDockerWebApplicationFactory>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly CustomDockerWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private string _token;

    public CategoryTestWithDockerContainer(CustomDockerWebApplicationFactory factory, ITestOutputHelper testOutputHelper)
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
    public async Task TestGetAllCategories()
    {
        //Arrange
        //Act
        var response = await _client.GetAsync(HttpHelper.Urls.GetAllCategories);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonString = await response.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(jsonString);
        Assert.True(deserializeObject?.flag);
        Assert.Equal(200, deserializeObject?.code);
        Assert.Equal("Find All Success", deserializeObject?.message);
        var jsonArray = deserializeObject?.data;
        _testOutputHelper.WriteLine(jsonArray?.ToString());
        var taskManagementList = JsonConvert.DeserializeObject<List<TaskCategoryDto>>(jsonArray.ToString());
        taskManagementList.Should().BeOfType<List<TaskCategoryDto>>();

    }

    [Fact, TestPriority(2)]
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

    [Fact, TestPriority(3)]
    public async Task TestUpdateTaskCategorySuccess()
    {
        if (string.IsNullOrEmpty(_token))
        {
            await AuthenticateAsync();
        }
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        //Arrange
        const int taskCategoryId = 100;
        var updateRequest = new UpdateRequestCategoryDto()
        {
            Name = "Category1",
            Description = "Description1 updated" //updated
        };
        
        //Act
        var response = await _client
            .PutAsJsonAsync(HttpHelper.Urls.UpdateCategoryById(taskCategoryId), updateRequest);

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

    [Fact, TestPriority(4)]
    public async Task TestAddCategorySuccess()
    {
        if (string.IsNullOrEmpty(_token))
        {
            await AuthenticateAsync();
        }
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        
        //Arrange
        var createRequest = new CreateRequestCategoryDto()
        {
            Name = "test1",
            Description = "some test1 description"
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
        Assert.Equal("test1", taskCategoryDto.Name);
        Assert.Equal("some test1 description", taskCategoryDto.Description);
    }

    [Fact, TestPriority(5)]
    public async Task TestAssignCategorySuccess()
    {
        if (string.IsNullOrEmpty(_token))
        {
            await AuthenticateAsync();
        }
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
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
        
        //Assign task
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
    
    
    [Fact, TestPriority(6)]
    public async Task TestDeleteCategorySuccess()
    {
        if (string.IsNullOrEmpty(_token))
        {
            await AuthenticateAsync();
        }
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        
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