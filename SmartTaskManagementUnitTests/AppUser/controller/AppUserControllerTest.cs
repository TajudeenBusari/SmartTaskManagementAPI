using System.Net;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartTaskManagementAPI.AppUser.controller;
using SmartTaskManagementAPI.AppUser.mapper;
using SmartTaskManagementAPI.AppUser.models;
using SmartTaskManagementAPI.AppUser.models.dto;
using SmartTaskManagementAPI.AppUser.service.impl;
using SmartTaskManagementAPI.System;

namespace SmartTaskManagementAPITest.AppUser.controller;

public class AppUserControllerTest
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly UserController _userController;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly UserMapper _userMapper;
    private readonly List<ApplicationUser> _users;

    //set up method runs before each test
    public AppUserControllerTest()
    {
        // Mock IUserService
        _userServiceMock = new Mock<IUserService>();
        
        _userMapper = new UserMapper();
        
        // Mock UserManager<ApplicationUser>
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
        
        // Initialize the UserController
        _userController = new UserController(_userServiceMock.Object, _userManagerMock.Object);
        
        // Simulate Admin authentication for the test
        var adminUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "AdminUser"),
            new Claim(ClaimTypes.Role, "Admin")
        }, "mock"));
        
        // Simulate Normal user authentication for the test
         /*var normalUser = new ClaimsPrincipal(new ClaimsIdentity(new []
         {
             new Claim(ClaimTypes.Name, "NormalUser"),
             new Claim(ClaimTypes.Role, "User")
         }, "mock"));
        

         //ADMIN
        _userController.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext(){User = adminUser}
        };

        //USER
        _userController.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext(){User = normalUser}

        };*/
         
        var userHasher1 = new PasswordHasher<ApplicationUser>();
        _users = new List<ApplicationUser>();
        
        
        var user1 = new ApplicationUser();
        user1.Id = new Guid("d9a70870-4be3-46b3-bf7b-9fa7d000a19d").ToString();
        user1.Email = "test1@test.com";
        user1.UserName = "Admintest1";
        user1.FirstName = "test1";
        user1.LastName = "testt1";
        user1.PasswordHash = userHasher1.HashPassword(user1, "Admin123!");
        
        _users.Add(user1);
        
        var user2 = new ApplicationUser();
        user2.Id = new Guid("d0acf911-e0c4-4b6b-bab4-dc121676c884").ToString();
        user2.FirstName = "test2@test.com";
        user2.Email = "test2@test.com";
        user2.UserName = "Usertest1";
        user2.LastName = "test2";
        user2.PasswordHash = userHasher1.HashPassword(user2, "User123!");
        _users.Add(user2);
        
    }
    
    private void SetUserContext(string username, string role)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        }, "mock"));
        _userController.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
    }

    private void SetUserContextWithIdAndRole(string userId, string role)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role)
        }, "mock"));
        _userController.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
    }

    [Fact]
    public async Task TestRegisterUser_AdminRoleRegisterUser_Success()
    {
        //Arrange
        SetUserContext("AdminUser", "Admin");
        var registrationRequest = new RegistrationRequestDto();
        {
            registrationRequest.Email = "test1@test.com";
            registrationRequest.FirstName = "test1";
            registrationRequest.LastName = "testt1";
            registrationRequest.Password = "test123!";
            registrationRequest.Username = "username1";
            registrationRequest.Roles = new List<string>() { "User" }; //admin creating a normal user
        }
        
        //User to be returned by the service class
        var createdUser = new ApplicationUser();
        {
            createdUser.Id = new Guid("066a4270-6c0b-4997-9358-6c14842e4018").ToString();
            createdUser.Email =  registrationRequest.Email;
            createdUser.FirstName = registrationRequest.FirstName;
            createdUser.LastName = registrationRequest.LastName;
            createdUser.UserName = registrationRequest.Username;
        }
        var roles = new List<string>(){"User"};
        
        //mock
        _userServiceMock
            .Setup(service => service.CreateUserAsync(registrationRequest))
            .ReturnsAsync(createdUser);
        _userManagerMock.Setup(manager => manager.GetRolesAsync(createdUser))
            .ReturnsAsync(roles);
        //Act
        var result = await _userController.CreateUser(registrationRequest);
        
        //Assert
        
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var resultValue = Assert.IsType<Result>(okResult.Value);
        Assert.True(resultValue.flag);
        Assert.Equal(200, resultValue.code);
        Assert.Equal("A new user has been created", resultValue.message);
        Assert.IsType<UserDto>(resultValue.data);
        var resultUserDto = resultValue.data as UserDto;
        Assert.NotNull(resultUserDto);
        Assert.Equal(createdUser.Id, resultUserDto.Id);
        Assert.Equal(createdUser.Email, resultUserDto.Email);
        Assert.Equal(createdUser.FirstName, resultUserDto.FirstName);
        Assert.Equal(createdUser.LastName, resultUserDto.LastName);
        Assert.Equal(createdUser.UserName, resultUserDto.Username);
        
        //verify mocks
        _userServiceMock.Verify(service => service.CreateUserAsync(registrationRequest), Times.Once);
        _userManagerMock.Verify(manager => manager.GetRolesAsync(createdUser), Times.Once);
        
    }

    [Fact]
    public async Task TestRegisterUser_UserRoleRegisterUser_Failure()
    {
        //Arrange
        SetUserContext("NormalUser", "User");
        var registrationRequest = new RegistrationRequestDto();
        {
            registrationRequest.Email = "test1@test.com";
            registrationRequest.FirstName = "test1";
            registrationRequest.LastName = "testt1";
            registrationRequest.Password = "test123!";
            registrationRequest.Username = "username1";
            registrationRequest.Roles = new List<string>() { "User" };
        }
        
        //User to be returned by the service class
        var createdUser = new ApplicationUser();
        {
            createdUser.Id = new Guid("066a4270-6c0b-4997-9358-6c14842e4018").ToString();
            createdUser.Email =  registrationRequest.Email;
            createdUser.FirstName = registrationRequest.FirstName;
            createdUser.LastName = registrationRequest.LastName;
            createdUser.UserName = registrationRequest.Username;
        }
       
        
        //mock
        //the service class is never called, so this lines of code is not really necessary
        _userServiceMock
            .Setup(service => service.CreateUserAsync(It.IsAny<RegistrationRequestDto>()))
            .ThrowsAsync(new Exception("User creation failed: error"));
        
        //Act
        var result = _userController.CreateUser(registrationRequest).Result;
        
        //Assert
        var unAuthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        unAuthorizedResult.Should().NotBeNull();
        var resultValue = Assert.IsType<Result>(unAuthorizedResult.Value);
        Assert.False(resultValue.flag);
        Assert.Equal(401, resultValue.code);
        Assert.Equal("User creation failed.", resultValue.message);
        _userServiceMock.Verify(service => service.CreateUserAsync(It.IsAny<RegistrationRequestDto>()), Times.Never);
        
    }

    [Fact]
    public async Task TestGetAllUsersWithAdminRole_Success()
    {
        SetUserContext("AdminUser", "Admin");
        
        //Arrange
        _userServiceMock
            .Setup(service => service.GetAllUsersAsync()).ReturnsAsync(_users);
        
        _userManagerMock
            .Setup(manager => 
                manager.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(new List<string>() { "Admin" });
        
        //Act
        var result = await _userController.GetAllUsers();
        
        //Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var resultValue = Assert.IsType<Result>(okResult.Value);
        Assert.True(resultValue.flag);
        Assert.Equal(200, resultValue.code);
        Assert.Equal("Find All Success", resultValue.message);
        Assert.IsType<List<UserDto>>(resultValue.data);
        var resultUserDto = resultValue.data as List<UserDto>;
        Assert.NotNull(resultUserDto);
        Assert.Equal(_users.Count, resultUserDto.Count);
        Assert.Equal(_users[0].Id, resultUserDto[0].Id);
        Assert.Equal(_users[0].Email, resultUserDto[0].Email);
        Assert.Equal(_users[0].FirstName, resultUserDto[0].FirstName);
        Assert.Equal(_users[0].LastName, resultUserDto[0].LastName);
        
    }

    [Fact]
    public async Task TestGetAllUsersWithUserRole_Failure()
    {
        //Arrange
        SetUserContext("NormalUser", "User");
        //Act
        var result = await  _userController.GetAllUsers();
        
        //Assert
        var unAuthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        var resultValue = Assert.IsType<Result>(unAuthorizedResult.Value);
        Assert.False(resultValue.flag);
        Assert.Equal(401, resultValue.code);
        Assert.Equal("You are not authorized to access this resource.", resultValue.message);
        _userServiceMock.Verify(service => service.GetAllUsersAsync(), Times.Never);
    }

    [Fact]
    public async Task TestGetAllUsersWithAdminRole_NoUserFoundReturnsEmptyList()
    {
        //Arrange
        SetUserContext("AdminUser", "Admin");
        _userServiceMock.Setup(service => service.GetAllUsersAsync()).ReturnsAsync(new List<ApplicationUser>());
        
        //Act
        var result = await _userController.GetAllUsers();
        
        //Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var resultValue = Assert.IsType<Result>(okResult.Value);
        Assert.True(resultValue.flag);
        Assert.Equal(200, resultValue.code);
        Assert.Empty((List<UserDto>)resultValue.data);
        _userServiceMock.Verify(service => service.GetAllUsersAsync(), Times.Once);
    }

    [Fact]
    public async Task TestGetNormalUserByIdWithAdminRole_Success()
    {
        //Arrange
        SetUserContext("AdminUser", "Admin");
        var adminUserId= _users[0].Id;
        var normalUserId = _users[1].Id;
        
        //mock
        _userServiceMock
            .Setup(s => s.GetUserAsync(normalUserId)).ReturnsAsync(_users[1]);
        _userManagerMock
            .Setup(m => 
                m.GetRolesAsync(_users[1])).ReturnsAsync(new List<string>() { "User" });
        
        //Act
        var result = await _userController.GetUserById(normalUserId);
        
        //Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var resultValue = Assert.IsType<Result>(okResult.Value);
        Assert.NotNull(resultValue);
        Assert.True(resultValue.flag);
        Assert.Equal(200, resultValue.code);
        Assert.Equal("Find One Success", resultValue.message);
        Assert.NotNull(resultValue.data);
        
        _userServiceMock.Verify(service => service.GetUserAsync(normalUserId), Times.Once);
        _userManagerMock.Verify(manager => manager.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Once);
        
    }

    [Fact]
    public async Task TestGetNormalUserByIdWithUserRole_Success()
    {
        /*NB: WE ARE USING SetUserContextWithIdAndRole() IN THIS METHOD BECAUSE IN THE
         *CONTROLLER LOGIC TO GET USER DETAILS, ONLY ADMIN CAN GET
         * ALL USERS DETAILS OR A NORMAL USER CAN GET THEIR OWN DETAILS, SO THE
         * LOGIC WILL CHECK IF USER IS ADMIN OR CHECK THE ID OF ANY OTHER USER
         * TRYING TO ACCESS THEIR OWN DETAILS OR ANY OTHER DETAILS.IF USER IS NOT AN ADMIN AND
         * TRYING TO CHECK DETAILS OTHER THAN THEIR OWN, A FORBIDDEN WILL BE RETURNED
         */
        
        var normalUserId = _users[1].Id;
        SetUserContextWithIdAndRole(normalUserId, "User");
        
        //mock
        _userServiceMock
            .Setup(s => s.GetUserAsync(normalUserId)).ReturnsAsync(_users[1]);
        _userManagerMock
            .Setup(m => 
                m.GetRolesAsync(_users[1])).ReturnsAsync(new List<string>() { "User" });
        
        //Act
        var result = await _userController.GetUserById(normalUserId);
        
        //Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var resultValue = Assert.IsType<Result>(okResult.Value);
        Assert.NotNull(resultValue);
        Assert.True(resultValue.flag);
        Assert.Equal(200, resultValue.code);
        Assert.Equal("Find One Success", resultValue.message);
        Assert.NotNull(resultValue.data);
        
        _userServiceMock.Verify(service => service.GetUserAsync(normalUserId), Times.Once);
        _userManagerMock.Verify(manager => manager.GetRolesAsync(_users[1]), Times.Once);
    }

    [Fact]
    public async Task TestGetNormalUserById_UserAccessOtherDetails_Forbidden()
    {
        //Arrange
        var normalUser = _users[1];
        var adminUser = _users[0];
        SetUserContextWithIdAndRole(normalUser.Id, "User");
        
        //Act
        var result = await _userController.GetUserById(adminUser.Id);
        
        //Assert
        var forbiddenResult = Assert.IsType<ForbidResult>(result.Result);
        _userServiceMock.Verify(service => service.GetUserAsync(It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(manager => manager.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task TestUpdateNormalUserByAdminUserRole_Success()
    {
        //any operation with Admin, the SetUserContext("AdminUser", "Admin") can be used
        //Or SetUserContextWithIdAndRole(userId, role)
        SetUserContext("AdminUser", "Admin");
        var adminUserId = _users[0].Id;
        var normalUserId = _users[1].Id;
        

        var userHasher1 = new PasswordHasher<ApplicationUser>();
        var oldUser = new ApplicationUser()
        {
            Id = normalUserId,
            Email = "test2@test.com",
            FirstName = "test2@test.com",
            LastName = "normalUser",
            UserName = "Usertest1",
            PasswordHash = userHasher1.HashPassword(new ApplicationUser(), "User123!")

        };
        
        var updateRequest = new UpdateRequestDto()
        {
            Username = "normalUsername", //updated
            FirstName = "normalUser", //updated
            LastName = "normalUser", //updated
            Email = "normalUser@normalUser.com", //updated

        };
        
        //Mock
        _userServiceMock
            .Setup(s => 
                s.UpdateUserByIdAsync(normalUserId, updateRequest))
            .ReturnsAsync(_users[1]);
        _userManagerMock
            .Setup(manager => 
                manager.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string>() { "User" });
        //Act
        var result = await _userController.UpdateUser(normalUserId, updateRequest);
        
        //Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var resultValue = Assert.IsType<Result>(okResult.Value);
        Assert.NotNull(resultValue);
        Assert.True(resultValue.flag);
        Assert.Equal(200, resultValue.code);
        Assert.Equal("Update Success", resultValue.message);
    }

    [Fact]
    public async Task TestUpdateNormalUserByUserRole_UserAccessOtherDetails_Forbidden()
    {
        //Arrange
        //Normal user trying to update Admin details
        var normalUser = _users[1];
        var adminUser = _users[0];
        SetUserContextWithIdAndRole(normalUser.Id, "User");
        var result = await _userController.UpdateUser(adminUser.Id, new UpdateRequestDto());
        //Assert
        var forbiddenResult = Assert.IsType<ForbidResult>(result.Result);
        _userServiceMock.Verify(service => service.GetUserAsync(It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(manager => manager.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
        
    }

    [Fact]
    public async Task TestUpdateNormalUserByUserRole_UserAccessOwnDetails_Success()
    {
        var normalUser = _users[1];
        SetUserContextWithIdAndRole(normalUser.Id, "User");
        var userHasher1 = new PasswordHasher<ApplicationUser>();
        var oldUser = new ApplicationUser()
        {
            Id = normalUser.Id,
            Email = "test2@test.com",
            FirstName = "test2@test.com",
            LastName = "normalUser",
            UserName = "Usertest1",
            PasswordHash = userHasher1.HashPassword(new ApplicationUser(), "User123!")

        };
        
        var updateRequest = new UpdateRequestDto()
        {
            Username = "normalUsername", //updated
            FirstName = "normalUser", //updated
            LastName = "normalUser", //updated
            Email = "normalUser@normalUser.com", //updated

        };
        
        //mock
        _userServiceMock
            .Setup(s => 
                s.UpdateUserByIdAsync(normalUser.Id, updateRequest))
            .ReturnsAsync(_users[1]);
        _userManagerMock
            .Setup(manager => 
                manager.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string>() { "User" });
        //Act
        var result = await _userController.UpdateUser(normalUser.Id, updateRequest);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var resultValue = Assert.IsType<Result>(okResult.Value);
        Assert.NotNull(resultValue);
        Assert.True(resultValue.flag);
        Assert.Equal(200, resultValue.code);
        Assert.Equal("Update Success", resultValue.message);
    }

    [Fact]
    public async Task TestDeleteNormalUserByAdminUserRole_Success()
    {
        //Arrange
        SetUserContext("AdminUser", "Admin");
        var normalUserId = _users[1].Id;
        
        //Mock
        _userServiceMock
            .Setup(s => 
                s.DeleteUserAsync(normalUserId)).Verifiable();
        //Act
        var result = await _userController.DeleteUser(normalUserId);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var resultValue = Assert.IsType<Result>(okResult.Value);
        Assert.NotNull(resultValue);
        Assert.True(resultValue.flag);
        Assert.Equal(200, resultValue.code);
        Assert.Equal("Delete Success", resultValue.message);
        _userServiceMock.Verify(s => s.DeleteUserAsync(normalUserId), Times.Once);
        
        //I am skipping all other delete methods like normal user deleting another user details
        
    }
    
}