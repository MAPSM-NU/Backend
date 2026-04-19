using Gym_App.Application.Authorization;
using Gym_App.Application.Services;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infrastructure.DTOs.Session;
using Microsoft.AspNetCore.Authorization;
using Moq;
using System.Security.Claims;

namespace GymApp.Tests.SessionTests
{
    public class SessionCreationTests : TestBase
    {
        private readonly ISessionService _sessionService;
        private readonly Mock<ICachedAuthorizationService> _authorizationService;
        private readonly Mock<ICurrentUser> currentUser;
        public SessionCreationTests() : base("SessionTestDatabase")
        {
            _authorizationService = new Mock<ICachedAuthorizationService>();
            currentUser = new Mock<ICurrentUser>();
            _sessionService = new SessionService(_unitOfWork, _authorizationService.Object,currentUser.Object);
        }
        [Fact]
        public async Task CreateSessionTest()
        {
            var user1 = CreateTestUser(CreateTestRole());
            var user2 = CreateTestUser(CreateTestRole(), "test2@gmail.com", "Test2_2004", "T", "Test2");
            var userIds = new SessionUsersDTO { Users = {user1.Id,user2.Id } };
            _authorizationService.Setup(x => x.IsInListAsync(It.IsAny<List<Guid>>())).ReturnsAsync(true);
            var result = await _sessionService.CreateSession(userIds);
            Assert.NotNull(result);
            Assert.Equal("Session created successfully", result.msg);
            Assert.Equal(2, result.status);
        }
        [Fact]
        public async Task CreateSessionWithInvalidIdsTest()
        {
            var userIds = new SessionUsersDTO { Users = { Guid.NewGuid(), Guid.Empty } };
            var result = await _sessionService.CreateSession(userIds);
            Assert.NotNull(result);
            Assert.Equal("Invalid user IDs", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task CreateSessioUnauthorizedTest()
        {
            var userIds = new SessionUsersDTO { Users = { Guid.NewGuid(), Guid.NewGuid() } };
            _authorizationService.Setup(x => x.IsInListAsync(It.IsAny<List<Guid>>())).ReturnsAsync(false);
            var result = await _sessionService.CreateSession(userIds);
            Assert.NotNull(result);
            Assert.Equal("Unauthorized", result.msg);
            Assert.Equal(1, result.status);
        }
        [Fact]
        public async Task CreateSessionUsersNotFoundTest()
        {
            var userIds = new SessionUsersDTO { Users = { Guid.NewGuid(), Guid.NewGuid() } };
            _authorizationService.Setup(x => x.IsInListAsync(It.IsAny<List<Guid>>())).ReturnsAsync(true);
            var result = await _sessionService.CreateSession(userIds);
            Assert.NotNull(result);
            Assert.Equal("User(s) not found", result.msg);
            Assert.Equal(0, result.status);
        }
    }
}
