using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SmartTaskIntegrationTest.Fixtures;
using SmartTaskIntegrationTest.Helper;
using SmartTaskManagementAPI.AppUser.models.dto;
using SmartTaskManagementAPI.Data;
using SmartTaskManagementAPI.System;
using Xunit.Abstractions;

namespace SmartTaskIntegrationTest.SUT;

public class AppUserTestWithInMemoryDatabase: IClassFixture<CustomInMemoryWebApplicationFactory<Program>>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly CustomInMemoryWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private string _tokenAdmin;
    private string _tokenUser;

    public AppUserTestWithInMemoryDatabase(CustomInMemoryWebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
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
            var database = scopedServices.GetRequiredService<ApplicationDbContext>();
            database.Database.EnsureDeleted();
            database.Database.EnsureCreated();
        }
    }

    /// <summary>
    /// methods to authenticate and get a token
    /// </summary>
    private async Task AuthenticateAsyncAsAdmin()
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
        var deserializeObject = JsonConvert.DeserializeObject<Result>(responseContent);
        _tokenAdmin = deserializeObject.data.ToString();
    }
    
    private async Task AuthenticateAsyncAsUser()
    {
        var loginRequest = new
        {
            username = "User",
            password = "User123!"
        };
        var content = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/v1/auth/login", content);
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(responseContent);
        _tokenUser = deserializeObject.data.ToString();
    }

    [Fact]
    public async Task TestGetAllUsersWithAdminSuccess()
    {
        //Arrange
        //Authenticate Admin
        if (string.IsNullOrEmpty(_tokenAdmin))
        {
            await AuthenticateAsyncAsAdmin();
        }
        
        // Set the Authorization header for the client
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenAdmin);
        
        //Act
        var response = await _client.GetAsync(HttpHelper.Urls.GetAllUsers);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonString = await response.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(jsonString);
        Assert.True(deserializeObject?.flag);
        Assert.Equal(200, deserializeObject?.code);
        Assert.Equal("Find All Success", deserializeObject?.message);
        var jsonArray = deserializeObject?.data;
        _testOutputHelper.WriteLine(jsonArray?.ToString());
        var users = JsonConvert.DeserializeObject<List<UserDto>>(jsonArray.ToString());
        users.Should().BeOfType<List<UserDto>>();
        
    }

    /// <summary>
    /// This method returns forbidden because normal user
    /// cannot get all users
    /// </summary>
    [Fact]
    public async Task TestGetAllUsersWithNormalUserReturnsForbidden()
    {
        //Authenticate Normal User
        if (string.IsNullOrEmpty(_tokenUser))
        {
            await AuthenticateAsyncAsUser();
        }
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenUser);
        
        var response = await _client.GetAsync(HttpHelper.Urls.GetAllUsers);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task TestCreateUserWithAdminSuccess()
    {
        //Arrange
        //Authenticate Admin
        if (string.IsNullOrEmpty(_tokenAdmin))
        {
            await AuthenticateAsyncAsAdmin();
        }
        
        // Set the Authorization header for the client
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenAdmin);

        var createRequest = new RegistrationRequestDto()
        {
            Username = "Benny",
            Password = "Benn123!",
            FirstName = "Ben",
            LastName = "Joe",
            Email = "ben@ben.com",
            //Roles = ["Admin", "User"],
            Roles = new List<string>{ "Admin", "User"}
        };
        
        //Act
        var response = await _client.PostAsJsonAsync(HttpHelper.Urls.AddUser, createRequest);
         //Assert
         response.StatusCode.Should().Be(HttpStatusCode.OK);
         var jsonString = await response.Content.ReadAsStringAsync();
         var deserializeObject = JsonConvert.DeserializeObject<Result>(jsonString);
         Assert.True(deserializeObject?.flag); 
         Assert.Equal("A new user has been created", deserializeObject?.message);
    }

    /// <summary>
    /// Normal user cannot create a new user
    /// </summary>
    [Fact]
    public async Task TestCreateUserWithNormalUserReturnsForbidden()
    {
        //Arrange
        if (string.IsNullOrEmpty(_tokenUser))
        {
            await AuthenticateAsyncAsUser();
        }
        // Set the Authorization header for the client
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenUser);
        
        var createRequest = new RegistrationRequestDto()
        {
            Username = "Benny",
            Password = "Benn123!",
            FirstName = "Ben",
            LastName = "Joe",
            Email = "ben@ben.com",
            //Roles = ["Admin", "User"],
            Roles = new List<string>{ "Admin", "User"}
        };
        
        //Act
        var response = await _client.PostAsJsonAsync(HttpHelper.Urls.AddUser, createRequest);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// This indicated that only admin can update any user details
    /// </summary>
    [Fact]
    public async Task TestUpdateUserWithAdminRoleSuccess()
    {
        //Arrange
        if (string.IsNullOrEmpty(_tokenAdmin))
        {
            await AuthenticateAsyncAsAdmin();
        }
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenAdmin);
        
        //create user to be updated //1st stage
        var createRequest = new RegistrationRequestDto()
        {
            Username = "teejaybaba",
            Password = "Teejaybaba123!",
            FirstName = "Taju",
            LastName = "Arigbabuwo",
            Email = "taju@arigbabu.com",
            Roles = new List<string> { "User" } //normal user role
        };
        //Act
        var response = await _client.PostAsJsonAsync(HttpHelper.Urls.AddUser, createRequest);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonString = await response.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(jsonString);
        Assert.True(deserializeObject?.flag);
        Assert.Equal("A new user has been created", deserializeObject?.message);
        var responseData = deserializeObject?.data.ToString();
        var deserializeDataObject = JsonConvert.DeserializeObject<UserDto>(responseData);
        var userId = deserializeDataObject?.Id;
        _testOutputHelper.WriteLine(userId);
        
        //update user with userId //2nd stage
        var updateRequest = new UpdateRequestDto()
        {
            Username = "teejaybaba",
            FirstName = "Tajuupdate", //updated
            LastName = "Arigbabuwo",
            Email = "taju@arigbabu.com"
        };
        
        //Act
        var updateResponse = await _client.PutAsJsonAsync(HttpHelper.Urls.UpdateUser(userId), updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonStringContent = await updateResponse.Content.ReadAsStringAsync();
        var deserializeStringObject = JsonConvert.DeserializeObject<Result>(jsonStringContent);
        Assert.True(deserializeStringObject?.flag);
        Assert.Equal(200, deserializeStringObject?.code);
        Assert.Equal("Update Success", deserializeStringObject?.message);
        var updateResponseData = deserializeStringObject?.data.ToString();
        var updatedUserDto = JsonConvert.DeserializeObject<UserDto>(updateResponseData);
        Assert.True(updatedUserDto?.Id == userId);
        Assert.Equal(updateRequest.FirstName, updatedUserDto.FirstName);
        Assert.Equal(updateRequest.LastName, updatedUserDto.LastName);
        Assert.Equal(updateRequest.Email, updatedUserDto.Email);
        Assert.Equal(updateRequest.Username, updatedUserDto.Username);

    }

    /// <summary>
    /// username = User, password = "User123!" trying
    /// to update the user below should return forbidden
    /// </summary>
    [Fact]
    public async Task TestUpdateUserWithNormalUserUpdatingAnotherUserDetailsReturnsForbidden()
    {
        //Arrange
        
        //CREATE A NEW USER
        if (string.IsNullOrEmpty(_tokenAdmin))
        {
            await AuthenticateAsyncAsAdmin();
        }

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenAdmin);
        
        //create user to be updated //1st stage
        var createRequest = new RegistrationRequestDto()
        {
            Username = "teejaybaba",
            Password = "Teejaybaba123!",
            FirstName = "Taju",
            LastName = "Arigbabuwo",
            Email = "taju@arigbabu.com",
            Roles = new List<string> { "User" } //normal user role
        };
        
        //Act
        var response = await _client.PostAsJsonAsync(HttpHelper.Urls.AddUser, createRequest);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonString = await response.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(jsonString);
        Assert.True(deserializeObject?.flag);
        Assert.Equal("A new user has been created", deserializeObject?.message);
        var responseData = deserializeObject?.data.ToString();
        var deserializeDataObject = JsonConvert.DeserializeObject<UserDto>(responseData);
        var userId = deserializeDataObject?.Id;
        _testOutputHelper.WriteLine(userId);
        
        
        //TO UPDATE THE USER CREATED
        if (string.IsNullOrEmpty(_tokenUser)) //username = User, password = "User123!"
        {
            await AuthenticateAsyncAsUser();
        }
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenUser);
        
        //update user with userId //2nd stage
        var updateRequest = new UpdateRequestDto()
        {
            Username = "teejaybaba",
            FirstName = "Tajuupdate", //updated
            LastName = "Arigbabuwo",
            Email = "taju@arigbabu.com"
        };
        
        //Act
        var updateResponse = await _client.PutAsJsonAsync(HttpHelper.Urls.UpdateUser(userId), updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        
    }

    /// <summary>
    /// We can also create the user we want to delete as done
    /// with the Update test but here we will use the existing user 
    /// </summary>
    [Fact]
    public async Task TestDeleteUserWithAdminUserDeletingUserReturnsSuccess()
    {
        //Arrange
        if (string.IsNullOrEmpty(_tokenAdmin))
        {
            await AuthenticateAsyncAsAdmin();
        }
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenAdmin);
        var existingUserId = new Guid("61616161-6161-6161-6161-616161616161").ToString();
        
        //Act
        var response = await _client.DeleteAsync(HttpHelper.Urls.DeleteUser(existingUserId));
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonString = await response.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(jsonString);
        Assert.True(deserializeObject?.flag);
        Assert.Equal(200, deserializeObject?.code);
        Assert.Equal("Delete Success", deserializeObject?.message);
        Assert.Null(deserializeObject?.data);
    }

    [Fact]
    public async Task TestDeleteUserWithUserDeletingOwnDetailsReturnsSuccess()
    {
        //Arrange
        if (string.IsNullOrEmpty( _tokenUser))
        {
            await AuthenticateAsyncAsUser();
        }
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenUser);
        
        var existingUserId = new Guid("61616161-6161-6161-6161-616161616161").ToString();
        
        //Act
        var response = await _client.DeleteAsync(HttpHelper.Urls.DeleteUser(existingUserId));
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonString = await response.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(jsonString);
        Assert.True(deserializeObject?.flag);
        Assert.Equal(200, deserializeObject?.code);
        Assert.Equal("Delete Success", deserializeObject?.message);
        Assert.Null(deserializeObject?.data);
    }

    [Fact]
    public async Task TestDeleteUserWithUserRoleDeletingAnotherUserReturnsFailure()
    {
        //Arrange
        if (string.IsNullOrEmpty(_tokenAdmin))
        {
            await AuthenticateAsyncAsAdmin();
        }
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenAdmin);
        
            //-1 CREATE USER TO BE DELETED
            var createRequest = new RegistrationRequestDto()
            {
                Username = "teejaybaba",
                Password = "Teejaybaba123!",
                FirstName = "Taju",
                LastName = "Arigbabuwo",
                Email = "taju@arigbabu.com",
                Roles = new List<string> { "User" } //normal user role
            };

        //Act
        var response = await _client.PostAsJsonAsync(HttpHelper.Urls.AddUser, createRequest);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonString = await response.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(jsonString);
        Assert.True(deserializeObject?.flag);
        Assert.Equal(200, deserializeObject?.code);
        //extract id from data
        var responseData = deserializeObject?.data.ToString();
        var deserializeDataObject = JsonConvert.DeserializeObject<UserDto>(responseData);
        var userId = deserializeDataObject?.Id;
        _testOutputHelper.WriteLine(userId);
        
            //2- NORMAL USER TRY TO DELETE
            if (string.IsNullOrEmpty(_tokenUser))
            {
                await AuthenticateAsyncAsUser();
            }
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenUser);
        //Act
        var result = await _client.DeleteAsync(HttpHelper.Urls.DeleteUser(userId));
        
        //Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        
    }
}