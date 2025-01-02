using System.Collections;
using System.Security.Claims;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MockQueryable;
using Moq;
using SmartTaskManagementAPI.AppUser.models;
using SmartTaskManagementAPI.AppUser.models.dto;
using SmartTaskManagementAPI.AppUser.service;
using SmartTaskManagementAPI.Exceptions;
using SmartTaskManagementAPI.Exceptions.modelNotFound;

namespace SmartTaskManagementAPITest.AppUser.service;

[TestSubject(typeof(UserService))]
public class UserServiceTest
{
    private readonly UserService userService;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
    private readonly List<ApplicationUser> _users;

    public UserServiceTest()
    {
        _mockUserManager = MockUserManager();
        _mockRoleManager = MockRoleManager();
        userService = new UserService(_mockUserManager.Object, _mockRoleManager.Object);
        
        _users = new List<ApplicationUser>();
        var userHasher1 = new PasswordHasher<ApplicationUser>();
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
    
    private static Mock<UserManager<ApplicationUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
    }

    private static Mock<RoleManager<IdentityRole>> MockRoleManager()
    {
        var store = new Mock<IRoleStore<IdentityRole>>();
        return new Mock<RoleManager<IdentityRole>>(store.Object, null, null, null, null);
    }
    
    
    
    [Fact]
    public async Task TestCreateUserWithoutRolesSuccess()
    {
        //Arrange
        var registrationDto = new RegistrationRequestDto()
        {
            Username = "username",
            Password = "Password123!",
            Email = "test@test.com",
            FirstName = "test",
            LastName = "test",
            Roles = null

        };
        var expectedUser = new ApplicationUser()
        {
            UserName = registrationDto.Username,
            Email = registrationDto.Email,
            FirstName = registrationDto.FirstName,
            LastName = registrationDto.LastName,

        };
        
        //Mock
        _mockUserManager
            .Setup(x => 
                x.CreateAsync(It.IsAny<ApplicationUser>(), registrationDto.Password))
            .ReturnsAsync(IdentityResult.Success);

        //Act
        var createdUser = await userService.CreateUserAsync(registrationDto);
        
        //Assert
        Assert.NotNull(createdUser);
        Assert.Equal(expectedUser.UserName, createdUser.UserName);
        Assert.Equal(expectedUser.Email, createdUser.Email);
        _mockUserManager.Verify(m => 
            m.CreateAsync(It.Is<ApplicationUser>(u => 
                u.UserName == registrationDto.Username), registrationDto.Password), Times.Once);

    }

    [Fact]
    public async Task TestCreateUserWithRoleSuccess()
    {
        //Arrange
        var registrationDto = new RegistrationRequestDto()
        {
            Username = "username",
            Password = "Password123!",
            Email = "test@test.com",
            FirstName = "test",
            LastName = "test",
            Roles = new List<string> { "Admin", "User" }

        };
        
        //Mock
        _mockUserManager
            .Setup(x => 
                x.CreateAsync(It.IsAny<ApplicationUser>(), registrationDto.Password))
            .ReturnsAsync(IdentityResult.Success);
        
        _mockRoleManager
            .Setup(r => 
                r.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        
        _mockRoleManager
            .Setup(r => r.CreateAsync(It.IsAny<IdentityRole>()))
            .ReturnsAsync(IdentityResult.Success);
        
        _mockUserManager
            .Setup(m => 
                m.AddToRolesAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
           
        
        //Act
        var createdUser = await userService.CreateUserAsync(registrationDto);
        
        //Assert
        Assert.NotNull(createdUser);
        Assert.Equal(registrationDto.Username, createdUser.UserName);
        Assert.Equal(registrationDto.Email, createdUser.Email);
        
        _mockUserManager
            .Verify(m => 
            m.CreateAsync(It.Is<ApplicationUser>(u => 
                u.UserName == registrationDto.Username), registrationDto.Password), Times.Once);
        
        _mockRoleManager
            .Verify(r => 
                r.CreateAsync(It.Is<IdentityRole>(x => 
                    registrationDto.Roles.Contains(x.Name))), Times.Exactly(2));
        
        _mockUserManager
            .Verify(r => 
                r.AddToRolesAsync(It.Is<ApplicationUser>(u => 
                    u.UserName == registrationDto.Username), registrationDto.Roles), Times.Exactly(1));
        
        //_mockRoleManager.Verify(rm => rm.CreateAsync(It.Is<IdentityRole>(r => registrationDto.Roles.Contains(r.Name))), Times.Exactly(2));
    }

    [Fact]
    public async Task TestCreateUserShouldThrowExceptionWhenUserCreationFails()
    {
        // Arrange
        var registrationDto = new RegistrationRequestDto
        {
            Username = "testuser",
            Email = "testuser@example.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User",
            Roles = null
        };
        
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registrationDto.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError(){Description="User creation failed"}));
        //Act and Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => userService.CreateUserAsync(registrationDto));
        Assert.Equal("User creation failed: User creation failed", exception.Message);
    }

    [Fact]
    public async Task TestCreateUserShouldThrowExceptionWhenRoleCreationFails()
    {
        // Arrange
        var registrationDto = new RegistrationRequestDto
        {
            Username = "testuser",
            Email = "testuser@example.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User",
            Roles = new List<string> { "Admin" }
        };
        
        //mock
        _mockUserManager.Setup(x => 
            x.CreateAsync(It.IsAny<ApplicationUser>(), registrationDto.Password))
            .ReturnsAsync(IdentityResult.Success);
        
        _mockRoleManager.Setup(rm => rm.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        
        _mockRoleManager.Setup(rm => rm.CreateAsync(It.IsAny<IdentityRole>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role creation failed" }));
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => userService.CreateUserAsync(registrationDto));
        Assert.Equal("Role creation failed: Role creation failed", exception.Message);

    }

    [Fact]
    public async Task TestGetUsersSuccess()
    {
        //Arrange
        
        //Mock
        // Convert the list to an IQueryable mock that supports async operations
        //add this package: Install-Package MockQueryable.Moq for mockQueryable

        var mockUsers = _users.AsQueryable().BuildMock();
        
        _mockUserManager
            .Setup(x => x.Users)
            .Returns(mockUsers);
        
        //Act
        var result = await userService.GetAllUsersAsync();
        //Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task TestGetUserByIdSuccess()
    {
        //Arrange
        var userHasher1 = new PasswordHasher<ApplicationUser>();
        var existingUser = new ApplicationUser()
        {
           Id = new Guid("d0acf911-e0c4-4b6b-bab4-dc121676c884").ToString(),
           Email = "test1@test.com",
           UserName = "Admintest1",
           FirstName = "test1",
           LastName = "testt1",
           PasswordHash = userHasher1.HashPassword(new ApplicationUser(), "Admin123!" )
        };
        
        //Mock
        _mockUserManager
            .Setup(m => m.FindByIdAsync(existingUser.Id)).ReturnsAsync(existingUser);

        //Act
        var result = await userService.GetUserAsync(existingUser.Id);
        
        //Assert
        Assert.NotNull(result);
        Assert.Equal(existingUser.Id, result.Id);
        Assert.Equal(existingUser.UserName, result.UserName);
        Assert.Equal(existingUser.Email, result.Email);
        Assert.Equal(existingUser.FirstName, result.FirstName);
        Assert.Equal(existingUser.LastName, result.LastName);
        Assert.Equal(existingUser.PasswordHash, result.PasswordHash);
        
        _mockUserManager.Verify(c => c.FindByIdAsync(existingUser.Id), Times.Once);
    }

    [Fact]
    public async Task TestGetUserByIdShouldThrowExceptionWhenUserDoesNotExist()
    {
        //Arrange
        var nonExistingUserId = Guid.NewGuid().ToString();
        //mock
        _mockUserManager
            .Setup(m => 
                m.FindByIdAsync(nonExistingUserId))
            .ReturnsAsync((ApplicationUser)null); //Simulate user not found
        
        //Act and Assert
        var exception = await Assert.ThrowsAsync<UserNotFoundException>(() => userService.GetUserAsync(nonExistingUserId));
        Assert.Equal("Could not find User with id " + nonExistingUserId, exception.Message);
        
    }

    [Fact]
    public async Task TestUpdateUserSuccess()
    {
        //Arrange
        var userHasher = new PasswordHasher<ApplicationUser>();
        var userTobeReturnedByUserManager = new ApplicationUser();
        {
            userTobeReturnedByUserManager.Id = new Guid().ToString();
            userTobeReturnedByUserManager.Email = "testuser@example.com";
            userTobeReturnedByUserManager.UserName = "testuser";
            userTobeReturnedByUserManager.FirstName = "Test";
            userTobeReturnedByUserManager.LastName = "User";
            userTobeReturnedByUserManager.PasswordHash = userHasher.HashPassword(userTobeReturnedByUserManager, "Admin123!");
        }
        var existingUserId =  userTobeReturnedByUserManager.Id;
        var updatedUser = new UpdateRequestDto();
        updatedUser.Username = "testuserupdated";
        updatedUser.Email = "testuser@example.com";
        updatedUser.FirstName = "Test";
        updatedUser.LastName = "User";
        
        
        //first find and then update
        //Mock
        _mockUserManager
            .Setup(x => x.FindByIdAsync(existingUserId))
            .ReturnsAsync(userTobeReturnedByUserManager);
        _mockUserManager
            .Setup(u => 
                u.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
        
        //Act
        var result = await userService.UpdateUserByIdAsync(existingUserId, updatedUser);
        //Assert
        Assert.NotNull(result);
        Assert.Equal(updatedUser.Username, result.UserName);
        Assert.Equal(updatedUser.Email, result.Email);
        Assert.Equal(updatedUser.FirstName, result.FirstName);
        Assert.Equal(updatedUser.LastName, result.LastName);
        _mockUserManager.Verify(u => u.FindByIdAsync(existingUserId), Times.Once);
        _mockUserManager.Verify(u => u.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Once);

    }

    [Fact]
    public async Task TestUpdateUserShouldThrowUserNotFoundExceptionWhenUserDoesNotExist()
    {
        var nonExistingUserId = Guid.NewGuid().ToString();
        var updateDto = new UpdateRequestDto()
        {
            Username = "newUser",
            FirstName = "NewFirstName",
            LastName = "NewLastName",
            Email = "new@test.com"
        };
        
        _mockUserManager.Setup(x => x.FindByIdAsync(nonExistingUserId))
            .ReturnsAsync((ApplicationUser)null);
        var exception = await Assert
            .ThrowsAsync<UserNotFoundException>(() => 
            userService.UpdateUserByIdAsync(nonExistingUserId, updateDto));
        _mockUserManager.Verify(u => u.FindByIdAsync(nonExistingUserId), Times.Once);
        _mockUserManager.Verify(u => u.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task TestUpdateUserShouldThrowExceptionWhenUserUpdateIsNotSuccessful()
    {
        // Arrange
        var existingUserId = new Guid().ToString();
        var existingUser = new ApplicationUser
        {
            Id = existingUserId,
            UserName = "oldUser",
            FirstName = "OldFirstName",
            LastName = "OldLastName",
            Email = "old@test.com"
        };
        var updateDto = new UpdateRequestDto
        {
            Username = "newUser",
            FirstName = "NewFirstName",
            LastName = "NewLastName",
            Email = "new@test.com"
        };
        
        _mockUserManager
            .Setup(m => m.FindByIdAsync(existingUserId))
            .ReturnsAsync(existingUser);
        
        _mockUserManager
            .Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Update failed" }));
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            userService.UpdateUserByIdAsync(existingUserId, updateDto));
        Assert.Equal("Update failed", exception.Message);
        
        _mockUserManager.Verify(m => m.FindByIdAsync(existingUserId), Times.Once);
        _mockUserManager.Verify(u => u.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }

    [Fact]
    public async Task TestDeleteUserSuccess()
    {
        //Arrange
        var existingUserId = Guid.NewGuid().ToString();
        var userHasher1 = new PasswordHasher<ApplicationUser>();
        var existingUser = new ApplicationUser()
        {
            Id = existingUserId,
            UserName = "oldUser",
            FirstName = "OldFirstName",
            LastName = "OldLastName",
            Email = "old@test.com",
            PasswordHash = userHasher1.HashPassword(new ApplicationUser(), "Admin123!" )
            
        };
        
        //Mock
        _mockUserManager
            .Setup(m => 
                m.FindByIdAsync(existingUserId))
            .ReturnsAsync(existingUser);
        _mockUserManager
            .Setup(x => 
                x.DeleteAsync(It.IsAny<ApplicationUser>())).Verifiable();  // Verifiable allows us to check if this method was called
        
        //Act
        await userService.DeleteUserAsync(existingUserId);
        
        //Assert
        _mockUserManager.Verify(u => u.FindByIdAsync(existingUserId), Times.Once);
        _mockUserManager.Verify(u => u.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }

    [Fact]
    public async Task TestChangePasswordSuccess()
    {
        //Arrange
        var existingUserId = Guid.NewGuid().ToString();
        var userHasher1 = new PasswordHasher<ApplicationUser>();
        var existingUser = new ApplicationUser()
        {
            Id = existingUserId,
            UserName = "oldUser",
            FirstName = "OldFirstName",
            LastName = "OldLastName",
            Email = "someemail1@email.com",
            PasswordHash = userHasher1.HashPassword(new ApplicationUser(), "Admin123!" )
        };
        
        //Mock
        _mockUserManager
            .Setup(m => 
                m.FindByIdAsync(existingUserId))
            .ReturnsAsync(existingUser);
        _mockUserManager
            .Setup(m => 
                m.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        
        //Act
        await userService.ChangePasswordAsync(existingUserId, "Admin123!", "NewPassword123!", "NewPassword123!");
        
        //Assert
        var passwordVerificationResult = userHasher1.VerifyHashedPassword(existingUser, existingUser.PasswordHash, "NewPassword123!");
        
        Assert.Equal(PasswordVerificationResult.Success, passwordVerificationResult);
        _mockUserManager.Verify(u => u.FindByIdAsync(existingUserId), Times.Once);
        _mockUserManager.Verify(u => u.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }

    [Fact]
    public async Task TestChangePasswordOldPasswordIsInCorrect()
    {
        //Arrange
        var existingUserId = Guid.NewGuid().ToString();
        var userHasher1 = new PasswordHasher<ApplicationUser>();
        var existingUser = new ApplicationUser()
        {
            Id = existingUserId,
            UserName = "oldUser",
            FirstName = "OldFirstName",
            LastName = "OldLastName",
            Email = "someemail1@email.com",
            PasswordHash = userHasher1.HashPassword(new ApplicationUser(), "CorrectOldPassword123!" )
        };
        
        //Mock
        _mockUserManager
            .Setup(m => 
                m.FindByIdAsync(existingUserId))
            .ReturnsAsync(existingUser);
        
        var incorrectOldPassword = "IncorrectOldPassword123!";
        var newPassword = "NewPassword123!";
        var confirmPassword = "NewPassword123!";
        
        //Act and Assert
        var exception = await Assert.ThrowsAsync<PasswordChangeIllegalArgument>(() => 
            userService.ChangePasswordAsync(existingUserId, incorrectOldPassword, newPassword, confirmPassword));
       Assert.Equal("Old password is incorrect.", exception.Message); 
       _mockUserManager.Verify(m => m.FindByIdAsync(existingUserId), Times.Once);
         _mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task TestChangePasswordNewPasswordIsSameAsOldPassword()
    {
        //Arrange
        var existingUserId = Guid.NewGuid().ToString();
        var userHasher1 = new PasswordHasher<ApplicationUser>();
        var existingUser = new ApplicationUser()
        {
            Id = existingUserId,
            UserName = "oldUser",
            FirstName = "OldFirstName",
            LastName = "OldLastName",
            Email = "someemail2@email.com",
            PasswordHash = userHasher1.HashPassword(new ApplicationUser(), "CorrectOldPassword123!")
        };
        
        //Mock
        _mockUserManager
            .Setup(m => 
                m.FindByIdAsync(existingUserId))
            .ReturnsAsync(existingUser);
        
        var correctOldPassword = "CorrectOldPassword123!";
        var newPassword = "CorrectOldPassword123!";
        var confirmPassword = "CorrectOldPassword123!";
        
       
        
        //Act and Assert
        var exception = await Assert.ThrowsAsync<PasswordChangeIllegalArgument>(() => 
            userService.ChangePasswordAsync(existingUserId, correctOldPassword, newPassword, confirmPassword));
        Assert.Equal("New password cannot be the same as the old password.", exception.Message);
        
    }

    [Fact]
    public async Task TestChangePasswordNewPasswordIsNotSameAsConfirmPassword()
    {
        //Arrange
        var existingUserId = Guid.NewGuid().ToString();
        var userHasher1 = new PasswordHasher<ApplicationUser>();
        var existingUser = new ApplicationUser()
        {
            Id = existingUserId,
            UserName = "oldUser",
            FirstName = "OldFirstName",
            LastName = "OldLastName",
            Email = "someemail4@email.com",
            PasswordHash = userHasher1.HashPassword(new ApplicationUser(), "CorrectOldPassword123!")
            
        };
        
        //MOCK
        _mockUserManager
            .Setup(m => 
                m.FindByIdAsync(existingUserId))
            .ReturnsAsync(existingUser);
        
        var correctOldPassword = "CorrectOldPassword123!";
        var newPassword = "NewPassword123!";
        var confirmPassword = "NewPassword12!"; //different from newPassword
        
        //Act and Assert
        var exception = await Assert.ThrowsAsync<PasswordChangeIllegalArgument>(() => 
            userService.ChangePasswordAsync(existingUserId, correctOldPassword, newPassword, confirmPassword));
        Assert.Equal("New password and confirm new password do not match.", exception.Message);
        _mockUserManager.Verify(m => m.FindByIdAsync(existingUserId), Times.Once);
        _mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
        
    }

    [Fact]
    public async Task TestChangePasswordNewPasswordDoesNotConformToPolicy()
    {
        //Arrange
        var existingUserId = Guid.NewGuid().ToString();
        var userHasher1 = new PasswordHasher<ApplicationUser>();
        var existingUser = new ApplicationUser()
        {
            Id = existingUserId,
            UserName = "oldUser",
            FirstName = "OldFirstName",
            LastName = "OldLastName",
            Email =  "someemail5@email.com",
            PasswordHash = userHasher1.HashPassword(new ApplicationUser(), "CorrectOldPassword123!")
        };
        
        //Mock
        _mockUserManager
            .Setup(m => 
                m.FindByIdAsync(existingUserId))
            .ReturnsAsync(existingUser);
        var correctOldPassword = "CorrectOldPassword123!";
        var newPassword = "newpassword"; //does not conform to policy
        var confirmPassword = "newpassword"; //does not conform to policy
        
        //Act and Assert
        var exception = await Assert.ThrowsAsync<PasswordChangeIllegalArgument>(() => 
            userService.ChangePasswordAsync(existingUserId, correctOldPassword, newPassword, confirmPassword));
        Assert.Equal("New password must contain at least one lowercase letter, one uppercase letter, one digit, one special character, and be between 8 and 15 characters long.", exception.Message);
        _mockUserManager.Verify(m => m.FindByIdAsync(existingUserId), Times.Once);
        _mockUserManager.Verify(m => m.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

}