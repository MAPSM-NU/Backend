

using Gym_App.Application.Authorization;
using Gym_App.Application.Services;
using Gym_App.Domain;
using Gym_App.Infastructure.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GymApp.Tests.SessionTests
{
    public class SessionQueryTests : TestBase
    {
        private readonly ISessionService _sessionService;
        private readonly Mock<ICachedAuthorizationService> _authorizationService;
        private readonly Mock<ICurrentUser> _currentUser;
        public SessionQueryTests() : base("SessionTestDatabase")
        {
            _authorizationService = new Mock<ICachedAuthorizationService>();
            _currentUser = new Mock<ICurrentUser>();
            _sessionService = new SessionService(_unitOfWork, _authorizationService.Object, _currentUser.Object);
        }
        [Fact]
        public async Task GetSessionMessagesDateTest()
        {
            var session = CreateTestSession(new List<User> { CreateTestUser(CreateTestRole()) });
            for (int i = 0; i < 5; i++)
            {
                CreateTestMessage(session.Users.First(), session, $"Test message {i}", DateTime.UtcNow.AddMinutes(i * 5));
            }
            await _unitOfWork.SaveChangesAsync();

            _authorizationService.Setup(x => x.IsInListAsync(It.IsAny<List<Guid>>())).ReturnsAsync(true);

            var messages = await _sessionService.GetSessionMessages(session.Id, DateTime.UtcNow.AddMinutes(-5).ToString(), DateTime.UtcNow.AddMinutes(11).ToString(), 1, "", "", "", 5);
            Assert.Equal(3, messages.Data!.TotalCount);
        }
        [Fact]
        public async Task GetSessionMessagesDateInvalidTest()
        {
            var session = CreateTestSession(new List<User> { CreateTestUser(CreateTestRole()) });
            for (int i = 0; i < 5; i++)
            {
                CreateTestMessage(session.Users.First(), session, $"Test message {i}", DateTime.UtcNow.AddMinutes(i * 5));
            }
            await _unitOfWork.SaveChangesAsync();

            _authorizationService.Setup(x => x.IsInListAsync(It.IsAny<List<Guid>>())).ReturnsAsync(true);

            var messages = await _sessionService.GetSessionMessages(session.Id, "wrong date", "wrong date", 1, "", "", "", 5);
            Assert.Equal(5, messages.Data!.TotalCount);
        }
        [Fact]
        public async Task GetSessionMessagesSearchTest()
        {
            var session = CreateTestSession(new List<User> { CreateTestUser(CreateTestRole()) });
            for (int i = 0; i < 5; i++)
            {
                CreateTestMessage(session.Users.First(), session, $"Test message {i}", DateTime.UtcNow.AddMinutes(i * 5));
            }
            await _unitOfWork.SaveChangesAsync();

            _authorizationService.Setup(x => x.IsInListAsync(It.IsAny<List<Guid>>())).ReturnsAsync(true);
            var messages = await _sessionService.GetSessionMessages(session.Id, "", "", 1, "Test", "Test", "Test", 5);
            Assert.Equal(5, messages.Data!.TotalCount);
            var messages2 = await _sessionService.GetSessionMessages(session.Id, "", "", 1, "", "", "Test message 1", 5);
            Assert.Equal(1, messages2.Data!.TotalCount);
        }
        [Fact]
        public async Task GetSessionMessageSortTest()
        {
            var session = CreateTestSession(new List<User> { CreateTestUser(CreateTestRole()) });
            for (int i = 0; i < 5; i++)
            {
                CreateTestMessage(session.Users.First(), session, $"Test message {i}", DateTime.UtcNow.AddMinutes(i * 5));
            }
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.IsInListAsync(It.IsAny<List<Guid>>())).ReturnsAsync(true);
            var messagesAsc = await _sessionService.GetSessionMessages(session.Id, "", "", 1, "date", "asc", "", 5);
            Assert.Equal("Test message 0", messagesAsc.Data!.Items.First().Content);
            var messagesDesc = await _sessionService.GetSessionMessages(session.Id, "", "", 1, "date", "desc", "", 5);
            Assert.Equal("Test message 4", messagesDesc.Data!.Items.First().Content);
        }
    }
}
