using Gym_App.Application.Authorization;
using Gym_App.Application.Services;
using Gym_App.Domain;
using Gym_App.Infastructure.DTOs.Session;
using Gym_App.Infastructure.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GymApp.Tests.SessionTests
{
    public class SessionMessagesManagementTests : TestBase
    {
        private readonly ISessionService _sessionService;
        private readonly Mock<ICachedAuthorizationService> _authorizationService;
        private readonly Mock<ICurrentUser> _currentUser;
        public SessionMessagesManagementTests() : base("SessionTestDatabase")
        {
            _authorizationService = new Mock<ICachedAuthorizationService>();
            _currentUser = new Mock<ICurrentUser>();
            _sessionService = new SessionService(_unitOfWork, _authorizationService.Object, _currentUser.Object);
        }
        public async Task AddMessagesToSessionTest()//The creation of a message includes putting session in it which means that messages already save in session by default which makes this tesy uselsess
        {
            return;
        }
        [Fact]
        public async Task RemoveMessagesFromSessionTest()
        {
            var session = CreateTestSession(new List<User> { CreateTestUser(CreateTestRole()) });
            var message = CreateTestMessage(session.Users.First(), session);
            var message2 = CreateTestMessage(session.Users.First(), session);
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.IsInListAsync(It.IsAny<List<Guid>>())).ReturnsAsync(true);
            var id = new ClaimsIdentity(claims: [
                new Claim(JwtRegisteredClaimNames.Sub, session.Users.First().Id.ToString())
            ]);
            ClaimsPrincipal userprincipal = new ClaimsPrincipal(id);
            var dto = new SessionMessagesDTO
            {
                messagesID = new List<Guid> { message.Id, message2.Id }
            };
            _currentUser.Setup(x => x.UserID).Returns(session.Users.First().Id);
            var result = await _sessionService.DeleteMessages(session.Id, dto);
            Assert.Equal("Messages removed successfully", result.msg);
            Assert.Equal(2, result.status);

            var updatedSession = await _unitOfWork.Sessions.GetById(session.Id);
            Assert.Equal(0, updatedSession.Messages.Count);
        }
        [Fact]
        public async Task RemoveMessagesInvalidDataTest()
        {
            var result = await _sessionService.DeleteMessages( Guid.Empty, new SessionMessagesDTO
            {
                messagesID = new List<Guid> { Guid.NewGuid() }
            });
            Assert.NotNull(result);
            Assert.Equal("Invalid session ID or messages", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task RemoveMessagesSessionNotFoundTest()
        {
            var result = await _sessionService.DeleteMessages( Guid.NewGuid(), new SessionMessagesDTO
            {
                messagesID = new List<Guid> { Guid.NewGuid() }
            });
            Assert.NotNull(result);
            Assert.Equal("Session not found", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task RemoveMessagesUnauthorizedTest()
        {
            var session = CreateTestSession(new List<User> { CreateTestUser(CreateTestRole()) });
            var message = CreateTestMessage(session.Users.First(), session);
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.IsInListAsync(It.IsAny<List<Guid>>())).ReturnsAsync(false);
            var result = await _sessionService.DeleteMessages(session.Id, new SessionMessagesDTO
            {
                messagesID = new List<Guid> { message.Id }
            });
            Assert.NotNull(result);
            Assert.Equal("Unauthorized", result.msg);
            Assert.Equal(1, result.status);
        }
        [Fact]
        public async Task RemoveMessagesNotSameUserTest()
        {
            var session = CreateTestSession(new List<User> { CreateTestUser(CreateTestRole()) });
            var message = CreateTestMessage(session.Users.First(), session);
            var message2 = CreateTestMessage(session.Users.First(), session);
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.IsInListAsync(It.IsAny<List<Guid>>())).ReturnsAsync(true);
            var id = new ClaimsIdentity(claims: [
                new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString())
            ]);
            ClaimsPrincipal userprincipal = new ClaimsPrincipal(id);
            var dto = new SessionMessagesDTO
            {
                messagesID = new List<Guid> { message.Id, message2.Id }
            };
            var result = await _sessionService.DeleteMessages(session.Id, dto);
            Assert.Equal("Unauthorized", result.msg);
            Assert.Equal(1, result.status);
        }
        [Fact]
        public async Task RemoveMessagesInvalidSenderTest()
        {
            var session = CreateTestSession(new List<User> { CreateTestUser(CreateTestRole()) });
            var message = CreateTestMessage(session.Users.First(), session);
            var message2 = CreateTestMessage(session.Users.First(), session);
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.IsInListAsync(It.IsAny<List<Guid>>())).ReturnsAsync(true);
            var id = new ClaimsIdentity(claims: [
                new Claim(JwtRegisteredClaimNames.Sub, Guid.Empty.ToString())
            ]);
            ClaimsPrincipal userprincipal = new ClaimsPrincipal(id);
            _currentUser.Setup(x => x.UserID).Returns(Guid.Empty);
            var dto = new SessionMessagesDTO
            {
                messagesID = new List<Guid> { message.Id, message2.Id }
            };
            var result = await _sessionService.DeleteMessages(session.Id, dto);
            Assert.Equal("Invalid sender ID", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task RemoveMessagesNotInSessionTest()
        {
            var session = CreateTestSession(new List<User> { CreateTestUser(CreateTestRole()) });
            var message = CreateTestMessage(session.Users.First(), session);
            var message2 = CreateTestMessage(session.Users.First(), session);
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.IsInListAsync(It.IsAny<List<Guid>>())).ReturnsAsync(true);
            var id = new ClaimsIdentity(claims: [
                new Claim(JwtRegisteredClaimNames.Sub, session.Users.First().Id.ToString())
            ]);
            ClaimsPrincipal userprincipal = new ClaimsPrincipal(id);
            _currentUser.Setup(x => x.User).Returns(userprincipal);
            var dto = new SessionMessagesDTO
            {
                messagesID = new List<Guid> { Guid.NewGuid() }
            };
            var result = await _sessionService.DeleteMessages(session.Id, dto);
            Assert.Equal("One or more messages not found in the session", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task RemoveMessagesEmptyListTest()
        {
            var session = CreateTestSession(new List<User> { CreateTestUser(CreateTestRole()) });
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.IsInListAsync(It.IsAny<List<Guid>>())).ReturnsAsync(true);
            var id = new ClaimsIdentity(claims: [
                new Claim(JwtRegisteredClaimNames.Sub, session.Users.First().Id.ToString())
            ]);
            ClaimsPrincipal userprincipal = new ClaimsPrincipal(id);
            _currentUser.Setup(x => x.User).Returns(userprincipal);
            var dto = new SessionMessagesDTO
            {
                messagesID = new List<Guid>()
            };
            var result = await _sessionService.DeleteMessages(session.Id, dto);
            Assert.Equal("No messages found in the session", result.msg);
            Assert.Equal(0, result.status);
        }
    }
}
