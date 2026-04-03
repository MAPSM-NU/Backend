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

namespace GymApp.Tests;

public class UserTests
{
    private readonly DbBase _db;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserServise _userServiceMock;
    private readonly Mock<ITokenHandler> _tokenHandlerMock;
    public UserTests()
    {
        var options = new DbContextOptionsBuilder<DbBase>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        _db = new DbBase(options);
        _unitOfWork = new UnitOfWork(_db);
        _tokenHandlerMock = new Mock<ITokenHandler>();
        _userServiceMock = new UserService(_unitOfWork, _tokenHandlerMock.Object);
    }
    [Fact]
    public async Task UserUpdateTest()
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            RoleName = "User"
        };
        await _unitOfWork.Roles.Create(role);
        await _unitOfWork.SaveChangesAsync();
        var userId = Guid.NewGuid();
        var user = new User
        {
            Role = role,
            Name = "Test",
            Email = "Test@gmail.com",
            Password = new PasswordHasher<User>().HashPassword(null, "Test_2004"),
            State = "TestState",
            City = "TestCity",
            Country = "TestCountry",
            DOB = DateTime.Now,
            Bio = "TestBio",
            CreatedAt = DateTime.Now,
            HeightCm = 0,
            WeightKg = 0,
            Id = userId,
            isEmailConfirmed = false,
            PhoneNumber = "testNum",
            UserType = "Trainee",
            UpdatedAt = DateTime.Now
        };
        await _unitOfWork.Users.Create(user);
        await _unitOfWork.SaveChangesAsync();
        var dto = new UserUpdateDTO
        {
            Id = userId,
            Name = "UpdatedTest",
            State = "UpdatedTestState",
            City = "UpdatedTestCity",
            Country = "UpdatedTestCountry",
            Bio = "UpdatedTestBio",
            HeightCm = 1,
            WeightKg = 1,
            PhoneNumber = "UpdatedtestNum",
        };
        var result = await _userServiceMock.UpdateUser(dto);
        Assert.Equal(2, result.status);

        var userFromDatabase = await _unitOfWork.Users.GetById(userId);
        Assert.NotNull(userFromDatabase);
        Assert.Equal("UpdatedTest", userFromDatabase.Name);
        Assert.Equal("UpdatedTestState", userFromDatabase.State);
        Assert.Equal("UpdatedTestCity", userFromDatabase.City);
        Assert.Equal("UpdatedTestCountry", userFromDatabase.Country);
        Assert.Equal("UpdatedTestBio", userFromDatabase.Bio);
        Assert.Equal(1, userFromDatabase.HeightCm);
        Assert.Equal(1, userFromDatabase.WeightKg);
        Assert.Equal("UpdatedtestNum", userFromDatabase.PhoneNumber);
    }
}