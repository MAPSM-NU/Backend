using Gym_App.Application.Services;
using Gym_App.Domain;
using Gym_App.Infastructure.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Client;
using Moq;
using System.Security.Claims;

namespace GymApp.Tests.SessionTests
{
    public class SessionCreationTests : TestBase
    {
        private readonly ISessionService _sessionService;
        private readonly Mock<IAuthorizationService> _authorizationService;
        public SessionCreationTests() : base("SessionTestDatabase")
        {
            _authorizationService = new Mock<IAuthorizationService>();
            _sessionService = new SessionService(_unitOfWork, _authorizationService.Object);
        }
        [Fact]
        public async Task CreateSessionTest()
        {
            var user1 = CreateTestUser(CreateTestRole());
            var user2 = CreateTestUser(CreateTestRole(), "test2@gmail.com", "Test2_2004", "T", "Test2");
            var userIds = new List<Guid> { user1.Id, user2.Id };
            _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());
            var result = await _sessionService.CreateSession(new ClaimsPrincipal(), userIds);
            Assert.NotNull(result);
            Assert.Equal("Session created successfully", result.msg);
            Assert.Equal(2, result.status);
        }
        [Fact]
        public async Task CreateSessionWithInvalidIdsTest()
        {
            var userIds = new List<Guid> { Guid.NewGuid(), Guid.Empty };
            var result = await _sessionService.CreateSession(new ClaimsPrincipal(), userIds);
            Assert.NotNull(result);
            Assert.Equal("Invalid user IDs", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task CreateSessioUnauthorizedTest()
        {
            var userIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Failed());
            var result = await _sessionService.CreateSession(new ClaimsPrincipal(), userIds);
            Assert.NotNull(result);
            Assert.Equal("Unauthorized", result.msg);
            Assert.Equal(1, result.status);
        }
        [Fact]
        public async Task CreateSessionUsersNotFoundTest()
        {
            var userIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());
            var result = await _sessionService.CreateSession(new ClaimsPrincipal(), userIds);
            Assert.NotNull(result);
            Assert.Equal("User(s) not found", result.msg);
            Assert.Equal(0, result.status);
        }
    }
}
