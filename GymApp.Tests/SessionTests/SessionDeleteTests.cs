using Gym_App.Application.Authorization;
using Gym_App.Application.Services;
using Gym_App.Domain;
using Gym_App.Infastructure.Interfaces.Services;
using Moq;

namespace GymApp.Tests.SessionTests
{
    public class SessionDeleteTests : TestBase
    {
        private readonly ISessionService _sessionService;
        private readonly Mock<ICachedAuthorizationService> _authorizationService;
        private readonly Mock<ICurrentUser> _currentUser;
        public SessionDeleteTests() : base("SessionTestDatabase")
        {
            _authorizationService = new Mock<ICachedAuthorizationService>();
            _currentUser = new Mock<ICurrentUser>();
            _sessionService = new SessionService(_unitOfWork, _authorizationService.Object,_currentUser.Object);
        }
        [Fact]
        public async Task DeleteSessionTest()
        {
            var user1 = CreateTestUser(CreateTestRole());
            var user2 = CreateTestUser(CreateTestRole(), "test2@gmail.com", "Test2_2004", "T", "Test2");
            var users = new List<User> { user1, user2 };
            var session = CreateTestSession(users);
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.IsInListAsync(It.IsAny<List<Guid>>())).ReturnsAsync(true);
            var result = await _sessionService.DeleteSession(session.Id);
            Assert.NotNull(result);
            Assert.Equal("Session deleted successfully", result.msg);
            Assert.Equal(2, result.status);
        }
        [Fact]
        public async Task DeleteSessionNotFoundTest()
        {
            _authorizationService.Setup(x => x.IsInListAsync(It.IsAny<List<Guid>>())).ReturnsAsync(true);
            var result = await _sessionService.DeleteSession(Guid.NewGuid());
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
            _authorizationService.Setup(x => x.IsInListAsync(It.IsAny<List<Guid>>())).ReturnsAsync(false);
            var result = await _sessionService.DeleteSession(session.Id);
            Assert.NotNull(result);
            Assert.Equal("Unauthorized", result.msg);
            Assert.Equal(1, result.status);
        }
    }
}
