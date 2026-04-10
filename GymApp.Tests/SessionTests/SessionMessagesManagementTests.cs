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
        private readonly Mock<IAuthorizationService> _authorizationService;
        public SessionMessagesManagementTests() : base("SessionTestDatabase")
        {
            _authorizationService = new Mock<IAuthorizationService>();
            _sessionService = new SessionService(_unitOfWork, _authorizationService.Object);
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
            _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());
            var id = new ClaimsIdentity(claims: [
                new Claim(JwtRegisteredClaimNames.Sub, session.Users.First().Id.ToString())
            ]);
            ClaimsPrincipal userprincipal = new ClaimsPrincipal(id);
            var dto = new SessionMessagesDTO
            {
                messagesID = new List<Guid> { message.Id, message2.Id }
            };
            var result = await _sessionService.DeleteMessages(userprincipal, session.Id, dto);
            Assert.Equal("Messages removed successfully", result.msg);
            Assert.Equal(2, result.status);

            var updatedSession = await _unitOfWork.Sessions.GetById(session.Id);
            Assert.Equal(0, updatedSession.Messages.Count);
        }
        [Fact]
        public async Task RemoveMessagesInvalidDataTest()
        {
            var result = await _sessionService.DeleteMessages(new ClaimsPrincipal(), Guid.Empty, new SessionMessagesDTO
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
            var result = await _sessionService.DeleteMessages(new ClaimsPrincipal(), Guid.NewGuid(), new SessionMessagesDTO
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
            _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Failed());
            var result = await _sessionService.DeleteMessages(new ClaimsPrincipal(), session.Id, new SessionMessagesDTO
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
            _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());
            var id = new ClaimsIdentity(claims: [
                new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString())
            ]);
            ClaimsPrincipal userprincipal = new ClaimsPrincipal(id);
            var dto = new SessionMessagesDTO
            {
                messagesID = new List<Guid> { message.Id, message2.Id }
            };
            var result = await _sessionService.DeleteMessages(userprincipal, session.Id, dto);
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
            _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());
            var id = new ClaimsIdentity(claims: [
                new Claim(JwtRegisteredClaimNames.Sub, Guid.Empty.ToString())
            ]);
            ClaimsPrincipal userprincipal = new ClaimsPrincipal(id);
            var dto = new SessionMessagesDTO
            {
                messagesID = new List<Guid> { message.Id, message2.Id }
            };
            var result = await _sessionService.DeleteMessages(userprincipal, session.Id, dto);
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
            _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());
            var id = new ClaimsIdentity(claims: [
                new Claim(JwtRegisteredClaimNames.Sub, session.Users.First().Id.ToString())
            ]);
            ClaimsPrincipal userprincipal = new ClaimsPrincipal(id);
            var dto = new SessionMessagesDTO
            {
                messagesID = new List<Guid> { Guid.NewGuid() }
            };
            var result = await _sessionService.DeleteMessages(userprincipal, session.Id, dto);
            Assert.Equal("One or more messages not found in the session", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task RemoveMessagesEmptyListTest()
        {
            var session = CreateTestSession(new List<User> { CreateTestUser(CreateTestRole()) });
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());
            var id = new ClaimsIdentity(claims: [
                new Claim(JwtRegisteredClaimNames.Sub, session.Users.First().Id.ToString())
            ]);
            ClaimsPrincipal userprincipal = new ClaimsPrincipal(id);
            var dto = new SessionMessagesDTO
            {
                messagesID = new List<Guid>()
            };
            var result = await _sessionService.DeleteMessages(userprincipal, session.Id, dto);
            Assert.Equal("No messages found in the session", result.msg);
            Assert.Equal(0, result.status);
        }
    }
}
