using Gym_App.Application.Authorization;
using Gym_App.Application.Hubs;
using Gym_App.Application.Services;
using Gym_App.Domain;
using Gym_App.Domain;
using Gym_App.Infastructure.DTOs.UserDTOs;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infrastructure.Interfaces.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using System.Runtime.CompilerServices;

namespace GymApp.Tests.AuthTests;

public class AuthTests : TestBase
{
    private readonly IUserServise _userServiceMock;
    private readonly Mock<ITokenHandler> _tokenHandlerMock;
    private readonly Mock<IFileService> _fileService;
    private readonly Mock<ICachedAuthorizationService> _authorizationServiceMock;
    private readonly Mock<IEmailSender> _EmailSender;
    private readonly Mock<ILogger<UserService>> _loggerMock;    
    public AuthTests() : base("AuthTestDatabase")
    {
        _tokenHandlerMock = new Mock<ITokenHandler>();
        _fileService = new Mock<IFileService>();
        _loggerMock = new Mock<ILogger<UserService>>();
        _authorizationServiceMock = new Mock<ICachedAuthorizationService>();
        _EmailSender = new Mock<IEmailSender>();
        _userServiceMock = new UserService(_unitOfWork, _tokenHandlerMock.Object,_fileService.Object,_loggerMock.Object
            ,_authorizationServiceMock.Object,_EmailSender.Object);
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
        _fileService.Setup(f => f.UploadFileAsync(//so the upload methodology doesnt return null
            It.IsAny<IFormFile>(),//For other sad paths tests, it wont even reach this state so there is no use to
            It.IsAny<string[]>()//copy paste it there
        )).ReturnsAsync(new Gym_App.Infastructure.Transfer_Classes.SettersResponse { msg = "success",status = 2});

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
        await _unitOfWork.Users.Create(new Gym_App.Domain.User
        {
            Name = "Test",
            Email = "Test@gmail.com",
            UserType = "T",
            Password = new PasswordHasher<Gym_App.Domain.User>().HashPassword(null, "Test_2004"),
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
        await _unitOfWork.Users.Create(new Gym_App.Domain.User
        {
            Name = "Test User",
            Email = "Test@gmail.com",
            UserType = "T",
            Password = new PasswordHasher<Gym_App.Domain.User>().HashPassword(null, "Test_2004"),
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
        await _unitOfWork.Users.Create(new Gym_App.Domain.User
        {
            Name = "Test User",
            Email = "Test@gmail.com",
            UserType = "T",
            Password = new PasswordHasher<Gym_App.Domain.User>().HashPassword(null, "Test_2004"),
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
    //Forget Password Tests
    [Fact]
    public async Task ForgetPasswordSuccessfully()
    {
        var user = CreateTestUser(CreateTestRole());
        await _unitOfWork.SaveChangesAsync();
        var passResult = await _userServiceMock.ForgotPassword(user.Email);
        Assert.NotNull(passResult);
        Assert.Equal("Password reset OTP sent successfully", passResult.msg);
        Assert.Equal(2, passResult.status);
    }
    [Fact]
    public async Task ForgetPasswordButUserNotFound()
    {
        var passResult = await _userServiceMock.ForgotPassword("nonexistent@example.com");
        Assert.NotNull(passResult);
        Assert.Equal("User not found", passResult.msg);
        Assert.Equal(0, passResult.status);
    }
    [Fact]
    public async Task ForgetPasswordEmailFailed()
    {
        var user = CreateTestUser(CreateTestRole());
        await _unitOfWork.SaveChangesAsync();
        _EmailSender.Setup(e => e.SendPasswordResetEmail(
            It.IsAny<string>(),
            It.IsAny<string>()
        )).ThrowsAsync(new Exception("Email sending failed"));
        var passResult = await _userServiceMock.ForgotPassword(user.Email);
        Assert.NotNull(passResult);
        Assert.Equal("An error occurred while processing the request.", passResult.msg);
        Assert.Equal(0, passResult.status);
    }
    //Resetting Passsword
    [Fact]
    public async Task ResetPasswordSuccessfully()
    {
        var user = CreateTestUser(CreateTestRole());
        await _unitOfWork.SaveChangesAsync();
        var passResult = await _userServiceMock.ForgotPassword(user.Email);
        Assert.Equal(2, passResult.status);
        var token = await _unitOfWork.PasswordResetToken.GetTokenByUserEmail(user.Email);
        var resetResult = await _userServiceMock.ResetPassword(user.Email, token.OTP, "NewPassword_2004");
        Assert.NotNull(resetResult);
        Assert.Equal("Password reset successfully", resetResult.msg);
        Assert.Equal(2, resetResult.status);
    }
    [Fact]
    public async Task ResetPasswordEmailNotFound()
    {
        var resetResult = await _userServiceMock.ResetPassword("nonexistent@example.com", "invalid-otp", "NewPassword_2004");
        Assert.NotNull(resetResult);
        Assert.Equal("User not found", resetResult.msg);
        Assert.Equal(0, resetResult.status);
    }
    [Fact]
    public async Task ResetPasswordInvalidOTP()
    {
        var user = CreateTestUser(CreateTestRole());
        await _unitOfWork.SaveChangesAsync();
        var passResult = await _userServiceMock.ForgotPassword(user.Email);
        Assert.Equal(2, passResult.status);
        var resetResult = await _userServiceMock.ResetPassword(user.Email, "invalid-otp", "NewPassword_2004");
        Assert.NotNull(resetResult);
        Assert.Equal("OTP verification failed: Invalid OTP", resetResult.msg);
        Assert.Equal(0, resetResult.status);
    }
    [Fact]
    public async Task ResetPasswordUsedOTP()
    {
        var user = CreateTestUser(CreateTestRole());
        await _unitOfWork.SaveChangesAsync();
        var passResult = await _userServiceMock.ForgotPassword(user.Email);
        Assert.Equal(2, passResult.status);
        var token = await _unitOfWork.PasswordResetToken.GetTokenByUserEmail(user.Email);
        // Simulate using the OTP
        token.isUsed = true;
        await _unitOfWork.PasswordResetToken.Update(token);
        await _unitOfWork.SaveChangesAsync();
        var resetResult = await _userServiceMock.ResetPassword(user.Email, token.OTP, "NewPassword_2004");
        Assert.NotNull(resetResult);
        Assert.Equal("OTP verification failed: OTP has already been used", resetResult.msg);
        Assert.Equal(0, resetResult.status);
    }
    [Fact]
    public async Task ResetPasswordInvalidPassword()
    {
        var user = CreateTestUser(CreateTestRole());
        await _unitOfWork.SaveChangesAsync();
        var passResult = await _userServiceMock.ForgotPassword(user.Email);
        Assert.Equal(2, passResult.status);
        var token = await _unitOfWork.PasswordResetToken.GetTokenByUserEmail(user.Email);
        var resetResult = await _userServiceMock.ResetPassword(user.Email, token.OTP, "test_2004");
        Assert.NotNull(resetResult);
        Assert.Equal("Invalid new password", resetResult.msg);
        Assert.Equal(0, resetResult.status);
    }
    [Fact]
    public async Task ResetPasswordSamePassword()
    {
        var user = CreateTestUser(CreateTestRole());
        await _unitOfWork.SaveChangesAsync();
        var passResult = await _userServiceMock.ForgotPassword(user.Email);
        Assert.Equal(2, passResult.status);
        var token = await _unitOfWork.PasswordResetToken.GetTokenByUserEmail(user.Email);
        var resetResult = await _userServiceMock.ResetPassword(user.Email, token.OTP, "Test_2004");
        Assert.NotNull(resetResult);
        Assert.Equal("New password cannot be the same as the old password", resetResult.msg);
        Assert.Equal(0, resetResult.status);
    }
}
