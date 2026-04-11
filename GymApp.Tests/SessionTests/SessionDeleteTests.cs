using Gym_App.Application.Services;
using Gym_App.Domain;
using Gym_App.Infastructure.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GymApp.Tests.SessionTests
{
    public class SessionDeleteTests : TestBase
    {
        private readonly ISessionService _sessionService;
        private readonly Mock<IAuthorizationService> _authorizationService;
        public SessionDeleteTests() : base("SessionTestDatabase")
        {
            _authorizationService = new Mock<IAuthorizationService>();
            _sessionService = new SessionService(_unitOfWork, _authorizationService.Object);
        }
        [Fact]
        public async Task DeleteSessionTest()
        {
            var user1 = CreateTestUser(CreateTestRole());
            var user2 = CreateTestUser(CreateTestRole(), "test2@gmail.com", "Test2_2004", "T", "Test2");
            var users = new List<User> { user1, user2 };
            var session = CreateTestSession(users);
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());
            var result = await _sessionService.DeleteSession(new ClaimsPrincipal(), session.Id);
            Assert.NotNull(result);
            Assert.Equal("Session deleted successfully", result.msg);
            Assert.Equal(2, result.status);
        }
        [Fact]
        public async Task DeleteSessionNotFoundTest()
        {
            _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());
            var result = await _sessionService.DeleteSession(new ClaimsPrincipal(), Guid.NewGuid());
            Assert.NotNull(result);
            Assert.Equal("Session not found", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task DeleteSessionUnauthorizedTest()
        {
            var user1 = CreateTestUser(CreateTestRole());
            var user2 = CreateTestUser(CreateTestRole(), "test2@gmail.com", "Test2_2004", "T", "Test2");
            var users = new List<User> { user1, user2 };
            var session = CreateTestSession(users);
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Failed());
            var result = await _sessionService.DeleteSession(new ClaimsPrincipal(), session.Id);
            Assert.NotNull(result);
            Assert.Equal("Unauthorized", result.msg);
            Assert.Equal(1, result.status);
        }
    }
}
