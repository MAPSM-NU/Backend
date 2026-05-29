using Gym_App.Application.Authorization;
using Gym_App.Application.Services;
using Gym_App.Infastructure.DTOs.UserDTOs;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infrastructure.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymApp.Tests.UserTests
{
    public class UserUpdateTests : TestBase
    {
        private readonly IUserServise _userServiceMock;
        private readonly Mock<ITokenHandler> _tokenHandlerMock;
        private readonly Mock<IFileService> _fileService;
        private readonly Mock<ICachedAuthorizationService> _authorizationService;
        private readonly Mock<ILogger<UserService>> _loggerMock;
        public UserUpdateTests() : base("UserTestDatabase")
        {
            _tokenHandlerMock = new Mock<ITokenHandler>();
            _fileService = new Mock<IFileService>();
            _loggerMock = new Mock<ILogger<UserService>>();
            _authorizationService = new Mock<ICachedAuthorizationService>();
            _userServiceMock = new UserService(_unitOfWork, _tokenHandlerMock.Object, _fileService.Object, _loggerMock.Object
                ,_authorizationService.Object);
        }
        [Fact]
        public async Task UserUpdateTest()
        {
            var user = CreateTestUser(CreateTestRole());
            await _unitOfWork.SaveChangesAsync();
            var dto = new UserUpdateDTO
            {
                Id = user.Id,
                Name = "UpdatedTest",
                State = "UpdatedTestState",
                City = "UpdatedTestCity",
                Country = "UpdatedTestCountry",
                Bio = "UpdatedTestBio",
                HeightCm = 1,
                WeightKg = 1,
                PhoneNumber = "UpdatedtestNum",
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _userServiceMock.UpdateUser(dto);
            Assert.Equal(2, result.status);

            var userFromDatabase = await _unitOfWork.Users.GetById(user.Id);
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
        public async Task UserUpdateNotFoundTest()
        {
            var dto = new UserUpdateDTO
            {
                Id = Guid.NewGuid(),
                Name = "UpdatedTest",
                State = "UpdatedTestState",
                City = "UpdatedTestCity",
                Country = "UpdatedTestCountry",
                Bio = "UpdatedTestBio",
                HeightCm = 1,
                WeightKg = 1,
                PhoneNumber = "UpdatedtestNum",
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _userServiceMock.UpdateUser(dto);
            Assert.NotNull(result);
            Assert.Equal("User not found", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task UserUpdateNameInvalidTest()
        {
            var dto = new UserUpdateDTO
            {
                Id = Guid.NewGuid(),
                Name = "",
                State = "UpdatedTestState",
                City = "UpdatedTestCity",
                Country = "UpdatedTestCountry",
                Bio = "UpdatedTestBio",
                HeightCm = 1,
                WeightKg = 1,
                PhoneNumber = "UpdatedtestNum",
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _userServiceMock.UpdateUser(dto);
            Assert.NotNull(result);
            Assert.Equal("User not found", result.msg);
            Assert.Equal(0, result.status);

            var user = CreateTestUser(CreateTestRole());
            var user2 = CreateTestUser(CreateTestRole(), "test@gmail.com", "Test_2004", "T", "Test2");//the second user
            await _unitOfWork.SaveChangesAsync();

            var dto2 = new UserUpdateDTO
            {
                Id = user.Id,
                Name = "Test2", // Name already exists
                State = "UpdatedTestState",
                City = "UpdatedTestCity",
                Country = "UpdatedTestCountry",
                Bio = "UpdatedTestBio",
                HeightCm = 1,
                WeightKg = 1,
                PhoneNumber = "UpdatedtestNum",
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result2 = await _userServiceMock.UpdateUser(dto2);
            Assert.NotNull(result2);
            Assert.Equal("Name is not valid", result2.msg);
            Assert.Equal(0, result2.status);
        }
    }
}
