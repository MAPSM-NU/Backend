using Gym_App.Application.Services;
using Gym_App.Domain;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.DTOs.UserDTOs;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infastructure.Repositries;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace GymApp.Tests.AuthTests;

public class AuthTests : TestBase
{
    private readonly IUserServise _userServiceMock;
    private readonly Mock<ITokenHandler> _tokenHandlerMock;
    public AuthTests() : base("AuthTestDatabase")
    {
        _tokenHandlerMock = new Mock<ITokenHandler>();
        _userServiceMock = new UserService(_unitOfWork, _tokenHandlerMock.Object);
    }

    //Sign up tests
    [Fact]
    public async Task SignUpTest()
    {
        //Creates a role and a refresh token since user service needs them
        var role = CreateTestRole();
        var refreshToken = CreateTestRefreshToken();
        await _unitOfWork.SaveChangesAsync();
        var dto = new UserCreationDTO
        {
            Name = "Test",
            Email = "Test@gmail.com",
            UserType = "T",
            Password = "Test_2004"
        };

        //return the JWT token
        _tokenHandlerMock.Setup(t => t.CreateAccessToken(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()
        )).ReturnsAsync("TestToken");

        //Creates a refresh token and saves it into database
        _tokenHandlerMock.Setup(t => t.CreateRefreshToken(
            It.IsAny<Guid>()
        )).Callback(() =>
        {
            refreshToken.UserID = Guid.NewGuid();
        }).ReturnsAsync(refreshToken.RefreshToken);
        var result = await _userServiceMock.SignUpUser(dto);

        //Asserting the result
        Assert.NotNull(result);
        Assert.Equal("User created successfully", result.msg);
        Assert.Equal(2, result.Status);
        Assert.Equal("TestToken", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
    }
    [Fact]
    public async Task SignUpMissingInfoTest()
    {
        var dto = new UserCreationDTO
        {
            Name = "",
            Email = "Test@gmail.com",
            UserType = "T",
            Password = "Test_2004"
        };
        var Nameresult = await _userServiceMock.SignUpUser(dto);
        //Asserting the result
        Assert.NotNull(Nameresult);
        Assert.Equal("Missing Information", Nameresult.msg);
        var EmailResult = await _userServiceMock.SignUpUser(new UserCreationDTO
        {
            Name = "Test",
            Email = "",
            UserType = "T",
            Password = "Test_2004"
        });
        //Asserting the result
        Assert.NotNull(EmailResult);
        Assert.Equal("Missing Information", EmailResult.msg);
        Assert.Equal(0, EmailResult.Status);

        var PasswordResult = await _userServiceMock.SignUpUser(new UserCreationDTO
        {
            Name = "Test",
            Email = "Test@gmail.com",
            UserType = "T",
            Password = ""
        });
        //Asserting the result
        Assert.NotNull(PasswordResult);
        Assert.Equal("Missing Information", PasswordResult.msg);
        Assert.Equal(0, PasswordResult.Status);
    }
    [Fact]
    public async Task SignUpNameInUseExists()
    {
        var role = new Role
        {
            RoleName = "User",
            Id = Guid.NewGuid()
        };
        await _unitOfWork.Roles.Create(role);
        await _unitOfWork.Users.Create(new User
        {
            Name = "Test",
            Email = "Test@gmail.com",
            UserType = "T",
            Password = new PasswordHasher<User>().HashPassword(null, "Test_2004"),
            Role = role
        });
        await _unitOfWork.SaveChangesAsync();
        var dto = new UserCreationDTO
        {
            Name = "Test",
            Email = "Test@gmail.com",
            Password = "Test_2004",
            UserType = "T"
        };
        var result = await _userServiceMock.SignUpUser(dto);
        //Asserting the result
        Assert.NotNull(result);
        Assert.Equal("Name is already used", result.msg);
        Assert.Equal(0, result.Status);
    }
    [Fact]
    public async Task SignUpInvalidEmail()
    {
        var dto = new UserCreationDTO
        {
            Name = "InvalidEmailTest",
            Email = "InvalidEmail",
            Password = "Test_2004",
            UserType = "T"
        };
        var result = await _userServiceMock.SignUpUser(dto);
        //Asserting the result
        Assert.NotNull(result);
        Assert.Equal("Invalid Email", result.msg);
        Assert.Equal(0, result.Status);
    }
    [Fact]
    public async Task SignUpEmailInUseTest()
    {
        var user = CreateTestUser(CreateTestRole());
        await _unitOfWork.SaveChangesAsync();
        var dto = new UserCreationDTO
        {
            Name = "Test_2004",
            Email = user.Email,
            Password = "Test_2004",
            UserType = "T"
        };
        var result = await _userServiceMock.SignUpUser(dto);
        //Asserting the result
        Assert.NotNull(result);
        Assert.Equal("Email already in use", result.msg);
        Assert.Equal(0, result.Status);
    }
    [Fact]
    public async Task SignUpInvalidPasswordTest()
    {
        var dto = new UserCreationDTO
        {
            Name = "Tets_2004",
            Email = "Test@gmail.com",
            Password = "invalidpassword",
            UserType = "T"
        };
        var result = await _userServiceMock.SignUpUser(dto);
        //Asserting the result
        Assert.NotNull(result);
        Assert.Equal("Invalid Password", result.msg);
        Assert.Equal(0, result.Status);
    }

    //Sign in tests
    [Fact]
    public async Task SignInTest()
    {
        var role = new Role
        {
            RoleName = "Test Role",
            Id = Guid.NewGuid()
        };
        await _unitOfWork.Roles.Create(role);
        await _unitOfWork.Users.Create(new User
        {
            Name = "Test User",
            Email = "Test@gmail.com",
            UserType = "T",
            Password = new PasswordHasher<User>().HashPassword(null, "Test_2004"),
            Role = role
        });
        await _unitOfWork.SaveChangesAsync();
        var email = "Test@gmail.com";
        var password = "Test_2004";

        _tokenHandlerMock.Setup(t => t.CreateAccessToken(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()
        )).ReturnsAsync("TestToken");
        _tokenHandlerMock
            .Setup(t => t.RefreshingToken(It.IsAny<Guid>()))
            .ReturnsAsync("refresh-token");
        var result = await _userServiceMock.SigninUser(email, password);

        //Asserting the result
        Assert.NotNull(result);
        Assert.Equal(2, result.Status);
        Assert.Equal("TestToken", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
    }
    [Fact]
    public async Task SignInWrongPasswordTest()
    {
        var role = new Role
        {
            RoleName = "Test Role",
            Id = Guid.NewGuid()
        };
        await _unitOfWork.Roles.Create(role);
        await _unitOfWork.Users.Create(new User
        {
            Name = "Test User",
            Email = "Test@gmail.com",
            UserType = "T",
            Password = new PasswordHasher<User>().HashPassword(null, "Test_2004"),
            Role = role
        });
        await _unitOfWork.SaveChangesAsync();
        var email = "Test@gmail.com";
        var password = "WrongPassword";

        _tokenHandlerMock.Setup(t => t.CreateAccessToken(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()
        )).ReturnsAsync("TestToken");
        _tokenHandlerMock
            .Setup(t => t.RefreshingToken(It.IsAny<Guid>()))
            .ReturnsAsync("refresh-token");
        var result = await _userServiceMock.SigninUser(email, password);

        //Asserting the result
        Assert.NotNull(result);
        Assert.Equal("Invalid Password", result.msg);
        Assert.Equal(0, result.Status);
    }
    [Fact]
    public async Task UserNotFound()
    {
        var email = "Wrong Credentials";
        var password = "WrongPassword";

        _tokenHandlerMock.Setup(t => t.CreateAccessToken(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()
        )).ReturnsAsync("TestToken");
        _tokenHandlerMock
            .Setup(t => t.RefreshingToken(It.IsAny<Guid>()))
            .ReturnsAsync("refresh-token");
        var result = await _userServiceMock.SigninUser(email, password);

        //Asserting the result
        Assert.NotNull(result);
        Assert.Equal("User not found", result.msg);
        Assert.Equal(0, result.Status);
    }
    
    
}
