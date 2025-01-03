using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Newtonsoft.Json;
using SmartTaskIntegrationTest.Fixtures;
using SmartTaskIntegrationTest.Helper;
using SmartTaskManagementAPI.AppUser.models.dto;
using SmartTaskManagementAPI.System;
using SmartTaskManagementAPI.TaskCategory.model.dto;
using Xunit.Abstractions;

namespace SmartTaskIntegrationTest.SUT;

public class AppUserTestWithDockerContainer: IClassFixture<CustomDockerWebApplicationFactory>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly CustomDockerWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private string _tokenAdmin;
    private string _tokenUser;

    public AppUserTestWithDockerContainer(CustomDockerWebApplicationFactory factory, ITestOutputHelper testOutputHelper)
    {
        _factory = factory;
        _testOutputHelper = testOutputHelper;
        _client = _factory.CreateClient();
    }

    /// <summary>
    /// Methods of authentication for Admin and normal user
    /// </summary>
    private async Task AuthenticateAsyncAsAdmin()
         {
             var loginRequest = new
             {
                 userName = "Admin", password = "Admin123!"
             };
              var content = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");
              var response = await _client.PostAsync("/api/v1/auth/login", content);
              response.EnsureSuccessStatusCode();
              //extract token from data
              var responseString = await response.Content.ReadAsStringAsync();
              var deserializeObject = JsonConvert.DeserializeObject<Result>(responseString);
              _tokenAdmin = deserializeObject.data.ToString();
              
         }
    
    private async Task AuthenticateAsyncAsNormalUser()
    {
        var loginRequest = new
        {
            userName = "User", password = "User123!"
        };
        var content = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/v1/auth/login", content);
        response.EnsureSuccessStatusCode();
        //extract token from data
        var responseString = await response.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(responseString);
        _tokenUser = deserializeObject.data.ToString();
         
    }

    [Fact]
    public async Task TestGetAllUsersWithAdminRoleSuccess()
    {
        //Arrange
        if (string.IsNullOrEmpty(_tokenAdmin))
        {
            await AuthenticateAsyncAsAdmin();
        }
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenAdmin);
        
        //Act
        var response  = await _client.GetAsync(HttpHelper.Urls.GetAllUsers);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseString = await response.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(responseString);
        Assert.True(deserializeObject?.flag);
        Assert.Equal(200, deserializeObject?.code);
        Assert.Equal("Find All Success", deserializeObject?.message);
        var jsonArray = deserializeObject?.data;
        _testOutputHelper.WriteLine(jsonArray?.ToString());
        var users = JsonConvert.DeserializeObject<List<UserDto>>(jsonArray.ToString());
        users.Should().BeOfType<List<UserDto>>();

    }

    [Fact]
    public async Task TestCreateNewUserWithAdminRoleSuccess()
    {
        //Arrange
        if (string.IsNullOrEmpty(_tokenAdmin))
        {
            await AuthenticateAsyncAsAdmin();
        }
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

    [Fact]
    public async Task TestCreateNewUserWithNormalRoleFailure()
    {
        //Arrange
        if (string.IsNullOrEmpty(_tokenUser))
        {
            await AuthenticateAsyncAsNormalUser();
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
            Roles = new List<string>{ "Admin", "User"}
        };
        
        //Act
        var response = await _client.PostAsJsonAsync(HttpHelper.Urls.AddUser, createRequest);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Admin can get any user
    /// </summary>
    [Fact]
    public async Task TestGetUserByIdWithAdminRoleSuccess()
    {
        //Arrange
        if (string.IsNullOrEmpty(_tokenAdmin))
        {
            await AuthenticateAsyncAsAdmin();
        }
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenAdmin);
        
        var existingUserId = new Guid("61616161-6161-6161-6161-616161616161").ToString(); //User Id
        
        //Act
        var response = await _client.GetAsync(HttpHelper.Urls.GetUserById(existingUserId));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonString = await response.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(jsonString);
        Assert.True(deserializeObject?.flag);
        Assert.Equal(200, deserializeObject?.code);
        Assert.Equal("Find One Success", deserializeObject?.message);
        var deserializeDataObject = deserializeObject?.data;
        _testOutputHelper.WriteLine(deserializeDataObject?.ToString());
        var userDetails = JsonConvert.DeserializeObject<UserDto>(deserializeDataObject.ToString());
        Assert.Equal(existingUserId, userDetails?.Id);
        Assert.Equal("John", userDetails?.FirstName);
        Assert.Equal("Doe", userDetails?.LastName);
        Assert.Equal("user@example.com", userDetails?.Email);
    }

    /// <summary>
    /// Normal cannot get other user details e.g. that of Admin
    /// </summary>
    [Fact]
    public async Task TestGetUserByIdWithNormalRoleFailure()
    {
        //Arrange
        if (string.IsNullOrEmpty(_tokenUser))
        {
            await AuthenticateAsyncAsNormalUser();
        }
        // Set the Authorization header for the client
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenUser);
        
        var existingUserId = new Guid("d5968f05-ba4e-4b33-a529-c3b3c281909f").ToString(); //Admin Id
        
        //Act
        var response = await _client.GetAsync(HttpHelper.Urls.GetUserById(existingUserId));
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        
    }

    [Fact]
    public async Task TestUpdateUserWithAdminRoleSuccess()
    {
        //Arrange
        if (string.IsNullOrEmpty(_tokenAdmin))
        {
            await AuthenticateAsyncAsAdmin();
        }
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenAdmin);
        
        //create user to  be updated or use the existing one
        var createRequest = new RegistrationRequestDto()
        {
            Username = "kenny",
            Password = "Kenny123!", //Password must start with capital letter
            FirstName = "kennyG",
            LastName = "kennyJ",
            Email = "kenny@kenny.com",
            Roles = new List<string>{"User"}
            
        };
        
        var createResponse = await _client.PostAsJsonAsync(HttpHelper.Urls.AddUser, createRequest);
        
        //Assert
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var deserializedObjectResult = JsonConvert.DeserializeObject<Result>(createResponseContent);
        Assert.True(deserializedObjectResult?.flag);
        Assert.Equal(200, deserializedObjectResult?.code);
        Assert.Equal("A new user has been created", deserializedObjectResult?.message);
        var responseData = deserializedObjectResult?.data;
        _testOutputHelper.WriteLine(responseData?.ToString());
        var userDetails = JsonConvert.DeserializeObject<UserDto>(responseData.ToString());
        var userId = userDetails?.Id;

        //update user
        var existingUserId = userId;
        var updateRequest = new UpdateRequestDto()
        {
            Username = "kenny",
            FirstName = "TaiwoG", //formally kennyG
            LastName = "kennyJ",
            Email = "kenny@kenny.com",
        };
        
        //Act
        var response = await _client.PutAsJsonAsync(HttpHelper.Urls.UpdateUser(existingUserId), updateRequest);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonString = await response.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(jsonString);
        Assert.True(deserializeObject?.flag);
        Assert.Equal(200, deserializeObject?.code);
        Assert.Equal("Update Success", deserializeObject?.message);
        var deserializeDataObject = deserializeObject?.data;
        _testOutputHelper.WriteLine(deserializeDataObject?.ToString());
        var updatedUserDetails = JsonConvert.DeserializeObject<UserDto>(deserializeDataObject.ToString());
        Assert.Equal(existingUserId, updatedUserDetails?.Id);
        Assert.Equal("TaiwoG", updatedUserDetails?.FirstName);
    }

    [Fact]
    public async Task TestUpdateUserWithNormalRoleFailure()
    {
        if (string.IsNullOrEmpty(_tokenUser))
        {
            await AuthenticateAsyncAsNormalUser();
        }
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenUser);
        
        
        var existingUserId = new Guid("d5968f05-ba4e-4b33-a529-c3b3c281909f").ToString(); //Another user, Admin
        var updateRequest = new UpdateRequestDto()
        {
            Username = "admin",
            FirstName = "Admin1updated", //formally Admin1
            LastName = "Admin1",
            Email = "admin@example.com",
        };
        
        //Act
        var response = await _client.PutAsJsonAsync(HttpHelper.Urls.UpdateUser(existingUserId), updateRequest);
        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task TestDeleteUserWithAdminRoleSuccess()
    {
        //Arrange
        if (string.IsNullOrEmpty(_tokenAdmin))
        {
            await AuthenticateAsyncAsAdmin();
        }
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenAdmin);
        
        //Delete an existing data or create a new one and delete
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
        var deserializeDataObject = deserializeObject?.data;
        Assert.Null(deserializeDataObject);
    }

    [Fact]
    public async Task TestPasswordChangeSuccess()
    {
        //Arrange
        if (string.IsNullOrEmpty(_tokenAdmin))
        {
            await AuthenticateAsyncAsAdmin();
        }

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenAdmin);

        //create user whose password will be changed
        var createRequest = new RegistrationRequestDto()
        {
            Username = "Tenny",
            Password = "Tenny123!", //Password must start with capital letter
            FirstName = "TennyG",
            LastName = "TennyJ",
            Email = "Tenny@Tenny.com",
            Roles = new List<string>{"User"}
        };
        
        var createResponse = await _client.PostAsJsonAsync(HttpHelper.Urls.AddUser, createRequest);
        
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var deserializedObjectResult = JsonConvert.DeserializeObject<Result>(createResponseContent);
        Assert.True(deserializedObjectResult?.flag);
        Assert.Equal(200, deserializedObjectResult?.code);
        Assert.Equal("A new user has been created", deserializedObjectResult?.message);
        var responseData = deserializedObjectResult?.data;
        _testOutputHelper.WriteLine(responseData?.ToString());
        var userDetails = JsonConvert.DeserializeObject<UserDto>(responseData.ToString());
        var userId = userDetails?.Id;
        
        //Login as the user to change password
        var tennyLoginRequest = new
        {
            userName = "Tenny", password = "Tenny123!"
        };
        
        var content = new StringContent(JsonConvert.SerializeObject(tennyLoginRequest), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/v1/auth/login", content);
        response.EnsureSuccessStatusCode();
        //extract token from data
        var responseString = await response.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(responseString);
        var tennyToken = deserializeObject.data.ToString();

        
        //Act
        //change password
        var changePasswordRequest = new Dictionary<string, string>()
        {
            {"currentPassword", "Tenny123!"},
            {"newPassword", "Tenny1234!"},
            {"confirmPassword", "Tenny1234!"}
        };
        
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tennyToken);
        var changePasswordResponse = await _client.PatchAsJsonAsync(HttpHelper.Urls.ChangePassword(userId), changePasswordRequest);
        
        //Assert
        changePasswordResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonString = await changePasswordResponse.Content.ReadAsStringAsync();
        var deObject = JsonConvert.DeserializeObject<Result>(jsonString);
        Assert.True(deObject?.flag);
        Assert.Equal(200, deObject?.code);
        Assert.Equal("Password Change Success", deObject?.message);
    }

    [Fact]
    public async Task TestPasswordChangeWithWrongOldPassword()
    {
        //Arrange
        if (string.IsNullOrEmpty(_tokenAdmin))
        {
            await AuthenticateAsyncAsAdmin();
        }

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenAdmin);

        //create user whose password will be changed
        var createRequest = new RegistrationRequestDto()
        {
            Username = "Sedo",
            Password = "Sedo123!", 
            FirstName = "Sedi",
            LastName = "Rebecca",
            Email = "Sedi@Rebecca.com",
            Roles = new List<string> { "User" }
        };
        
        var createResponse = await _client.PostAsJsonAsync(HttpHelper.Urls.AddUser, createRequest);
        
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var deserializedObjectResult = JsonConvert.DeserializeObject<Result>(createResponseContent);
        Assert.True(deserializedObjectResult?.flag);
        Assert.Equal(200, deserializedObjectResult?.code);
        Assert.Equal("A new user has been created", deserializedObjectResult?.message);
        var responseData = deserializedObjectResult?.data;
        _testOutputHelper.WriteLine(responseData?.ToString());
        var userDetails = JsonConvert.DeserializeObject<UserDto>(responseData.ToString());
        var userId = userDetails?.Id;
        
        //Login as the user to change password
        var sedoLoginRequest = new
        {
            userName = "Sedo", password = "Sedo123!"

        };
        var content = new StringContent(JsonConvert.SerializeObject(sedoLoginRequest), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/v1/auth/login", content);
        response.EnsureSuccessStatusCode();
        //extract token from data
        var responseString = await response.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(responseString);
        var sedoToken = deserializeObject.data.ToString();
        
        //Act
        //change password
        var changePasswordRequest = new Dictionary<string, string>()
        {
            {"currentPassword", "Sedo1234!"}, //wrong old password
            {"newPassword", "Sedo12345!"},
            {"confirmPassword", "Sedo12345!"}
        };
        
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sedoToken);
        var changePasswordResponse = await _client.PatchAsJsonAsync(HttpHelper.Urls.ChangePassword(userId), changePasswordRequest);
        
        //Assert
        changePasswordResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var jsonString = await changePasswordResponse.Content.ReadAsStringAsync();
        var deObject = JsonConvert.DeserializeObject<Result>(jsonString);
        Assert.False(deObject?.flag);
        Assert.Equal(400, deObject?.code);
        Assert.Equal("Old password is incorrect.", deObject?.message);
        var respData = deObject?.data;
        Assert.Null(respData);
    }

    [Fact]
    public async Task TestPasswordChangeWithNewPasswordAndConfirmPasswordNotMatching()
    {
        
        //Arrange
        if (string.IsNullOrEmpty(_tokenAdmin))
        {
            await AuthenticateAsyncAsAdmin();
        }

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenAdmin);

        //create user whose password will be changed
        var createRequest = new RegistrationRequestDto()
        {
            Username = "nathaniel",
            Password = "Nath123!", 
            FirstName = "Nath",
            LastName = "Aniel",
            Email = "nath@aniel.com",
            Roles = new List<string> { "User" }
        };
        
        var createResponse = await _client.PostAsJsonAsync(HttpHelper.Urls.AddUser, createRequest);
        
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var deserializedObjectResult = JsonConvert.DeserializeObject<Result>(createResponseContent);
        Assert.True(deserializedObjectResult?.flag);
        Assert.Equal(200, deserializedObjectResult?.code);
        Assert.Equal("A new user has been created", deserializedObjectResult?.message);
        var responseData = deserializedObjectResult?.data;
        _testOutputHelper.WriteLine(responseData?.ToString());
        var userDetails = JsonConvert.DeserializeObject<UserDto>(responseData.ToString());
        var userId = userDetails?.Id;
        
        //Login as the user to change password
        var nathLoginRequest = new
        {
            userName = "nathaniel", password = "Nath123!"

        };
        var content = new StringContent(JsonConvert.SerializeObject(nathLoginRequest), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/v1/auth/login", content);
        response.EnsureSuccessStatusCode();
        //extract token from data
        var responseString = await response.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(responseString);
        var nathToken = deserializeObject.data.ToString();
        
        //Act
        //change password
        var changePasswordRequest = new Dictionary<string, string>()
        {
            {"currentPassword", "Nath123!"},
            {"newPassword", "Nath12345!"},
            {"confirmPassword", "Nath123456!"} //confirm password does not match new password
        };
        
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", nathToken);
        var changePasswordResponse = await _client.PatchAsJsonAsync(HttpHelper.Urls.ChangePassword(userId), changePasswordRequest);
        
        //Assert
        changePasswordResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var jsonString = await changePasswordResponse.Content.ReadAsStringAsync();
        var deObject = JsonConvert.DeserializeObject<Result>(jsonString);
        Assert.False(deObject?.flag);
        Assert.Equal(400, deObject?.code);
        Assert.Equal("New password and confirm password do not match.", deObject?.message);
    }

    [Fact]
    public async Task TestPasswordChangeWithNewPasswordSameAsOldPassword()
    {
        //Arrange
        if (string.IsNullOrEmpty(_tokenAdmin))
        {
            await AuthenticateAsyncAsAdmin();
        }

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenAdmin);

        //create user whose password will be changed
        var createRequest = new RegistrationRequestDto()
        {
            Username = "kinggy",
            Password = "King123!",
            FirstName = "King",
            LastName = "Daniel",
            Email = "King@Daniel.com",
            Roles = new List<string> { "User" }
        };
        
        var createResponse = await _client.PostAsJsonAsync(HttpHelper.Urls.AddUser, createRequest);
        
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var deserializedObjectResult = JsonConvert.DeserializeObject<Result>(createResponseContent);
        Assert.True(deserializedObjectResult?.flag);
        Assert.Equal(200, deserializedObjectResult?.code);
        Assert.Equal("A new user has been created", deserializedObjectResult?.message);
        var responseData = deserializedObjectResult?.data;
        _testOutputHelper.WriteLine(responseData?.ToString());
        var userDetails = JsonConvert.DeserializeObject<UserDto>(responseData.ToString());
        var userId = userDetails?.Id;
        
        //Login as the user to change password
        var kingLoginRequest = new
        {
            userName = "kinggy", password = "King123!"

        };
        var content = new StringContent(JsonConvert.SerializeObject(kingLoginRequest), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/v1/auth/login", content);
        response.EnsureSuccessStatusCode();
        //extract token from data
        var responseString = await response.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(responseString);
        var kingToken = deserializeObject.data.ToString();
        
        //Act
        //change password
        var changePasswordRequest = new Dictionary<string, string>()
        {
            {"currentPassword", "King123!"},
            {"newPassword", "King123!"}, //new password is same as old password
            {"confirmPassword", "King123!"}
          
        };
        
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", kingToken);
        var changePasswordResponse = await _client.PatchAsJsonAsync(HttpHelper.Urls.ChangePassword(userId), changePasswordRequest);
        
        //Assert
        changePasswordResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var jsonString = await changePasswordResponse.Content.ReadAsStringAsync();
        var deObject = JsonConvert.DeserializeObject<Result>(jsonString);
        Assert.False(deObject?.flag);
        Assert.Equal(400, deObject?.code);
        Assert.Equal("New password cannot be the same as the old password.", deObject?.message);
    }

    [Fact]
    public async Task TestChangePasswordNewPasswordDoesNotMeetPolicy()
    {
        
        //Arrange
        if (string.IsNullOrEmpty(_tokenAdmin))
        {
            await AuthenticateAsyncAsAdmin();
        }

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenAdmin);

        //create user whose password will be changed
        var createRequest = new RegistrationRequestDto()
        {
            Username = "Fredrick",
            Password = "Fred123!",
            FirstName = "Fred",
            LastName = "Dan",
            Email = "Fred@Dan.com",
            Roles = new List<string> { "User" }
        };
        
        var createResponse = await _client.PostAsJsonAsync(HttpHelper.Urls.AddUser, createRequest);
        
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var deserializedObjectResult = JsonConvert.DeserializeObject<Result>(createResponseContent);
        Assert.True(deserializedObjectResult?.flag);
        Assert.Equal(200, deserializedObjectResult?.code);
        Assert.Equal("A new user has been created", deserializedObjectResult?.message);
        var responseData = deserializedObjectResult?.data;
        _testOutputHelper.WriteLine(responseData?.ToString());
        var userDetails = JsonConvert.DeserializeObject<UserDto>(responseData.ToString());
        var userId = userDetails?.Id;
        
        //Login as the user to change password
        var kingLoginRequest = new
        {
            userName = "Fredrick", password = "Fred123!"

        };
        var content = new StringContent(JsonConvert.SerializeObject(kingLoginRequest), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/v1/auth/login", content);
        response.EnsureSuccessStatusCode();
        //extract token from data
        var responseString = await response.Content.ReadAsStringAsync();
        var deserializeObject = JsonConvert.DeserializeObject<Result>(responseString);
        var fredToken = deserializeObject.data.ToString();
        
        //Act
        //change password
        var changePasswordRequest = new Dictionary<string, string>()
        {
            {"currentPassword", "Fred123!"},
            {"newPassword", "Fred123"}, //new password does not meet policy
            {"confirmPassword", "Fred123"}
        };
        
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", fredToken);
        var changePasswordResponse = await _client.PatchAsJsonAsync(HttpHelper.Urls.ChangePassword(userId), changePasswordRequest);
        
        //Assert
        changePasswordResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var jsonString = await changePasswordResponse.Content.ReadAsStringAsync();
        var deObject = JsonConvert.DeserializeObject<Result>(jsonString);
        Assert.False(deObject?.flag);
        Assert.Equal(400, deObject?.code);
        Assert.Equal("New password must contain at least one lowercase letter, one uppercase letter, one digit, one special character, and be between 8 and 15 characters long.", deObject?.message);
    }


}