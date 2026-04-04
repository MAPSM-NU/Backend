using Gym_App.Application.Services;
using Gym_App.Domain;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.DTOs.UserDTOs;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infastructure.Repositries;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
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
            .UseInMemoryDatabase(databaseName: $"UserTestDatabase-{Guid.NewGuid()}")
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
    [Fact]
    public async Task UserDeleteTest()
    {
        var role = new Role
        {
            RoleName = "User",
            Id = Guid.NewGuid()
        };
        var userId = Guid.NewGuid();
        await _unitOfWork.Roles.Create(role);
        await _unitOfWork.Users.Create(new User
        {
            Name = "Test User",
            Email = "Test@gmail.com",
            UserType = "T",
            Password = new PasswordHasher<User>().HashPassword(null, "Test_2004"),
            Role = role,
            Id = userId
        });
        await _unitOfWork.SaveChangesAsync();
        var result = await _userServiceMock.DeleteUser(userId);
        Assert.Equal(2, result.status);
        var user = await _userServiceMock.GetUserByID(userId);
        Assert.Null(user.Value);
    }
    [Fact]
    public async Task UserGetByIdTest()
    {
        var role = new Role
        {
            RoleName = "User",
            Id = Guid.NewGuid()
        };
        var userId = Guid.NewGuid();
        await _unitOfWork.Roles.Create(role);
        await _unitOfWork.Users.Create(new User
        {
            Name = "Test User",
            Email = "Test@gmail.com",
            UserType = "T",
            Password = new PasswordHasher<User>().HashPassword(null, "Test_2004"),
            Role = role,
            Id = userId
        });
        await _unitOfWork.SaveChangesAsync();
        var result = await _userServiceMock.GetUserByID(userId);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.status);
    }
    [Fact]
    public async Task UserGetAllTest()
    {
        //if (_db.Users.Any())
        //    _db.Users.RemoveRange(_db.Users);
        var role = new Role
        {
            RoleName = "User",
            Id = Guid.NewGuid()
        };
        await _unitOfWork.Roles.Create(role);
        for (int i = 0; i < 5; i++)
        {
            await _unitOfWork.Users.Create(new User
            {
                Name = $"Test User {i}",
                Email = $"Test{i}@gmail.com",
                UserType = "T",
                Password = new PasswordHasher<User>().HashPassword(null, $"Test{i}_2004"),
                Role = role,
                Id = Guid.NewGuid()
            });
        }
        await _unitOfWork.SaveChangesAsync();
        var result = await _userServiceMock.GetAllUsers(1,5);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.status);
        Assert.Equal(5, result.Data.TotalCount);
    }
    [Fact]
    public async Task UserWithSearchTest()
    {
        var role = new Role
        {
            RoleName = "User",
            Id = Guid.NewGuid()
        };
        await _unitOfWork.Roles.Create(role);
        for (int i = 0; i < 5; i++)
        {
            await _unitOfWork.Users.Create(new User
            {
                Name = $"Test User {i}",
                Email = $"Test{i}@gmail.com",
                City = $"Test City {i}",
                State = $"Test State {i}",
                Country = $"Test Country {i}",
                PhoneNumber = $"TestNum {i}",
                CreatedAt = DateTime.Now,
                Bio = $"Test Bio {i}",
                UserType = "T",
                Password = new PasswordHasher<User>().HashPassword(null, $"Test{i}_2004"),
                Role = role,
                Id = Guid.NewGuid()
            });
        }
        await _unitOfWork.SaveChangesAsync();
        
        var Emailresult = await _userServiceMock.GetUsers("","",1,"","", "Test City 3",5);
        Assert.NotNull(Emailresult.Data);
        Assert.Equal(2, Emailresult.status);
        Assert.Equal(1, Emailresult.Data.TotalCount);

        var Cityresult = await _userServiceMock.GetUsers("","",1,"","", "Test City 3",5);
        Assert.NotNull(Cityresult.Data);
        Assert.Equal(2, Cityresult.status);
        Assert.Equal(1, Cityresult.Data.TotalCount);

        var StateResult = await _userServiceMock.GetUsers("","",1,"","", "Test State 2",5);
        Assert.NotNull(StateResult.Data);
        Assert.Equal(2, StateResult.status);
        Assert.Equal(1, StateResult.Data.TotalCount);

        var CountryResult = await _userServiceMock.GetUsers("","",1,"","", "Test Country 1",5);
        Assert.NotNull(CountryResult.Data);
        Assert.Equal(2, CountryResult.status);
        Assert.Equal(1, CountryResult.Data.TotalCount);
    }
    [Fact]
    public async Task UserWithSortingTest()
    {
        var role = new Role
        {
            RoleName = "User",
            Id = Guid.NewGuid()
        };
        await _unitOfWork.Roles.Create(role);
        for (int i = 0; i < 5; i++)
        {
            await _unitOfWork.Users.Create(new User
            {
                Name = $"Test User {i}",
                Email = $"Test{i}@gmail.com",
                City = $"Test City {i}",
                State = $"Test State {i}",
                Country = $"Test Country {i}",
                PhoneNumber = $"TestNum {i}",
                CreatedAt = DateTime.Now.AddMinutes(i * 5),// Add 5 minutes for each user to ensure different CreatedAt values
                Bio = $"Test Bio {i}",
                UserType = "T",
                Password = new PasswordHasher<User>().HashPassword(null, $"Test{i}_2004"),
                Role = role,
                Id = Guid.NewGuid()
            });
        }
        await _unitOfWork.SaveChangesAsync();

        var AscendinNamegResult = await _userServiceMock.GetUsers("", "", 1, "Name", "asc", "", 5);
        Assert.NotNull(AscendinNamegResult.Data);
        Assert.Equal(2, AscendinNamegResult.status);
        Assert.Equal(5, AscendinNamegResult.Data.TotalCount);
        Assert.Equal("Test User 0", AscendinNamegResult.Data.Items[0].Name);

        var DescendingNameResult = await _userServiceMock.GetUsers("", "", 1, "Name", "desc", "", 5);
        Assert.NotNull(DescendingNameResult.Data);
        Assert.Equal(2, DescendingNameResult.status);
        Assert.Equal(5, DescendingNameResult.Data.TotalCount);
        Assert.Equal("Test User 4", DescendingNameResult.Data.Items[0].Name);

        var AscendingCreatedAtResult = await _userServiceMock.GetUsers("", "", 1, "CreatedAt", "asc", "", 5);
        Assert.NotNull(AscendingCreatedAtResult.Data);
        Assert.Equal(2, AscendingCreatedAtResult.status);
        Assert.Equal(5, AscendingCreatedAtResult.Data.TotalCount);
        Assert.Equal("Test User 0", AscendingCreatedAtResult.Data.Items[0].Name);

        var DescendingCreatedAtResult = await _userServiceMock.GetUsers("", "", 1, "CreatedAt", "desc", "", 5);
        Assert.NotNull(DescendingCreatedAtResult.Data);
        Assert.Equal(2, DescendingCreatedAtResult.status);
        Assert.Equal(5, DescendingCreatedAtResult.Data.TotalCount);
        Assert.Equal("Test User 4", DescendingCreatedAtResult.Data.Items[0].Name);

    }
    [Fact]
    public async Task UserTimeTest()
    {
        var role = new Role
        {
            RoleName = "User",
            Id = Guid.NewGuid()
        };
        await _unitOfWork.Roles.Create(role);
        for (int i = 0; i < 5; i++)
        {
            await _unitOfWork.Users.Create(new User
            {
                Name = $"Test User {i}",
                Email = $"Test{i}@gmail.com",
                City = $"Test City {i}",
                State = $"Test State {i}",
                Country = $"Test Country {i}",
                PhoneNumber = $"TestNum {i}",
                CreatedAt = DateTime.Now.AddMinutes(i * 5),// Add 5 minutes for each user to ensure different CreatedAt values
                Bio = $"Test Bio {i}",
                UserType = "T",
                Password = new PasswordHasher<User>().HashPassword(null, $"Test{i}_2004"),
                Role = role,
                Id = Guid.NewGuid()
            });
        }
        await _unitOfWork.SaveChangesAsync();
        var InBetweenResult = await _userServiceMock.GetUsers(DateTime.Now.AddMinutes(-5).ToString(), DateTime.Now.AddMinutes(9).ToString(), 1, "", "asc", "", 5);
        Assert.NotNull(InBetweenResult.Data);
        Assert.Equal(2, InBetweenResult.status);
        Assert.Equal(2, InBetweenResult.Data.TotalCount);

        var BeforeResult = await _userServiceMock.GetUsers(DateTime.Now.AddMinutes(-15).ToString(), DateTime.Now.AddMinutes(-6).ToString(), 1, "", "asc", "", 5);
        Assert.NotNull(BeforeResult.Data);
        Assert.Equal(2, BeforeResult.status);
        Assert.Equal(0, BeforeResult.Data.TotalCount);

        var AfterResult = await _userServiceMock.GetUsers(DateTime.Now.AddMinutes(6).ToString(), DateTime.Now.AddMinutes(21).ToString(), 1, "", "asc", "", 5);
        Assert.NotNull(AfterResult.Data);
        Assert.Equal(2, AfterResult.status);
        Assert.Equal(3, AfterResult.Data.TotalCount);

    }
    [Fact]
    public async Task UserFilterTest()
    {
        var role = new Role
        {
            RoleName = "User",
            Id = Guid.NewGuid()
        };
        await _unitOfWork.Roles.Create(role);
        for (int i = 0; i < 5; i++)
        {
            await _unitOfWork.Users.Create(new User
            {
                Name = $"Test User {i}",
                Email = $"Test{i}@gmail.com",
                City = $"Test City {i}",
                State = $"Test State {i}",
                Country = $"Test Country {i}",
                PhoneNumber = $"TestNum {i}",
                CreatedAt = DateTime.Now.AddMinutes(i * 5),// Add 5 minutes for each user to ensure different CreatedAt values
                Bio = $"Test Bio {i}",
                UserType = "T",
                Password = new PasswordHasher<User>().HashPassword(null, $"Test{i}_2004"),
                Role = role,
                Id = Guid.NewGuid()
            });
        }
        await _unitOfWork.SaveChangesAsync();

        var filterResult = await _userServiceMock.GetUsers(DateTime.Now.AddMinutes(6).ToString(), DateTime.Now.AddMinutes(21).ToString(), 1, "Name", "desc", "Test City", 5);
        Assert.NotNull(filterResult.Data);
        Assert.Equal(2, filterResult.status);
        Assert.Equal(3, filterResult.Data.TotalCount);
        Assert.Equal("Test User 4", filterResult.Data.Items[0].Name);
    }
}