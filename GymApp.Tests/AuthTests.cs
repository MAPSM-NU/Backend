using Gym_App.Application.Services;
using Gym_App.Domain;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.DTOs.UserDTOs;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infastructure.Repositries;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace GymApp.Tests;

public class AuthTests
{
    private readonly DbBase _db;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserServise _userServiceMock;
    private readonly Mock<ITokenHandler> _tokenHandlerMock;
    public AuthTests()
    {
        var options = new DbContextOptionsBuilder<DbBase>()
            .UseInMemoryDatabase(databaseName: $"AuthTestDatabase-{Guid.NewGuid()}")
            .Options;
        _db = new DbBase(options);
        _unitOfWork = new UnitOfWork(_db);
        _tokenHandlerMock = new Mock<ITokenHandler>();
        _userServiceMock = new UserService(_unitOfWork, _tokenHandlerMock.Object);
    }
    [Fact]
    public async Task SignUpTest()
    {
        //Creates a role and a refresh token since user service needs them
        var role = new Role
        {
            RoleName = "User",
            Id = Guid.NewGuid()
        };
        var refreshToken = new RefreshTokens
        {
            RefreshToken = "refresh-token",
            Expires = DateTime.UtcNow.AddDays(7),
        };
        //saves into database
        await _unitOfWork.Roles.Create(role);
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
        )).Callback(async () =>
        {
            refreshToken.UserID = Guid.NewGuid();
            await _unitOfWork.Tokens.Create(refreshToken);
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
    [Fact]
    public async Task NameInUseExists()
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
    public async Task InvalidEmail()
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
}
