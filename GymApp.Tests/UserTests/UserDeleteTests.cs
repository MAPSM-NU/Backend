using Gym_App.Application.Authorization;
using Gym_App.Application.Services;
using Gym_App.Domain;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infrastructure.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymApp.Tests.UserTests
{
    public class UserDeleteTests : TestBase
    {
        private readonly IUserServise _userServiceMock;
        private readonly Mock<ITokenHandler> _tokenHandlerMock;
        private readonly Mock<IFileService> _fileService;
        private readonly Mock<ICachedAuthorizationService> _authorizationService;
        private readonly Mock<ILogger<UserService>> _loggerMock;
        public UserDeleteTests() : base("UserTestDatabase")
        {
            _tokenHandlerMock = new Mock<ITokenHandler>();
            _fileService = new Mock<IFileService>();
            _loggerMock = new Mock<ILogger<UserService>>();
            _authorizationService = new Mock<ICachedAuthorizationService>();
            _userServiceMock = new UserService(_unitOfWork, _tokenHandlerMock.Object, _fileService.Object, _loggerMock.Object
                ,_authorizationService.Object);
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
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _userServiceMock.DeleteUser(userId);
            Assert.Equal(2, result.status);
            var user = await _userServiceMock.GetUserByID(userId);
            Assert.Null(user.Value);
        }
        [Fact]
        public async Task UserDeleteNotFoundTest()
        {
            var result = await _userServiceMock.DeleteUser(Guid.NewGuid());
            Assert.NotNull(result);
            Assert.Equal("User not found", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task UserDeleteInvalidIdTest()
        {
            var result = await _userServiceMock.DeleteUser(Guid.Empty);
            Assert.NotNull(result);
            Assert.Equal("Invalid user ID", result.msg);
            Assert.Equal(0, result.status);
        }
    }
}
