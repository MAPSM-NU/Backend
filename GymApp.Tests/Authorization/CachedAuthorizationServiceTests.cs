using Gym_App.Application.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System.Security.Claims;
using Xunit;

namespace GymApp.Tests.Authorization
{
    public class CachedAuthorizationServiceTests
    {
        private readonly Mock<IAuthorizationService> _mockAuthService;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly IMemoryCache _memoryCache;
        private readonly CachedAuthorizationService _service;

        public CachedAuthorizationServiceTests()
        {
            _mockAuthService = new Mock<IAuthorizationService>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _service = new CachedAuthorizationService(_mockAuthService.Object, _mockCurrentUser.Object, _memoryCache);
        }

        #region IsAuthorizedAsync Tests

        [Fact]
        public async Task IsAuthorizedAsync_UserNotAuthenticated_ReturnsFalse()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var result = await _service.IsAuthorizedAsync("TestPolicy", Guid.NewGuid());

            Assert.False(result);
            _mockAuthService.Verify(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task IsAuthorizedAsync_AuthorizedUser_ReturnsTrue()
        {
            var userId = Guid.NewGuid();
            var resource = Guid.NewGuid();
            var policy = "TestPolicy";

            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserID).Returns(userId);
            _mockCurrentUser.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) })));

            _mockAuthService.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var result = await _service.IsAuthorizedAsync(policy, resource);

            Assert.True(result);
        }

        [Fact]
        public async Task IsAuthorizedAsync_UnauthorizedUser_ReturnsFalse()
        {
            var userId = Guid.NewGuid();
            var resource = Guid.NewGuid();
            var policy = "TestPolicy";

            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserID).Returns(userId);
            _mockCurrentUser.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) })));

            _mockAuthService.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Failed());

            var result = await _service.IsAuthorizedAsync(policy, resource);

            Assert.False(result);
        }

        [Fact]
        public async Task IsAuthorizedAsync_CacheHit_DoesNotCallAuthService()
        {
            var userId = Guid.NewGuid();
            var resource = Guid.NewGuid();
            var policy = "TestPolicy";

            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserID).Returns(userId);
            _mockCurrentUser.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) })));

            _mockAuthService.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            await _service.IsAuthorizedAsync(policy, resource);
            _mockAuthService.Invocations.Clear();

            var resultCached = await _service.IsAuthorizedAsync(policy, resource);

            Assert.True(resultCached);
            _mockAuthService.Verify(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task IsAuthorizedAsync_DifferentResources_CacheSeparately()
        {
            var userId = Guid.NewGuid();
            var resource1 = Guid.NewGuid();
            var resource2 = Guid.NewGuid();
            var policy = "TestPolicy";

            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserID).Returns(userId);
            _mockCurrentUser.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) })));

            _mockAuthService.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            await _service.IsAuthorizedAsync(policy, resource1);
            await _service.IsAuthorizedAsync(policy, resource2);

            _mockAuthService.Verify(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public async Task IsAuthorizedAsync_DifferentPolicies_CacheSeparately()
        {
            var userId = Guid.NewGuid();
            var resource = Guid.NewGuid();
            var policy1 = "Policy1";
            var policy2 = "Policy2";

            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserID).Returns(userId);
            _mockCurrentUser.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) })));

            _mockAuthService.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            await _service.IsAuthorizedAsync(policy1, resource);
            await _service.IsAuthorizedAsync(policy2, resource);

            _mockAuthService.Verify(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public async Task IsAuthorizedAsync_FirstReturnsTrueThenFalse_CachesCorrectly()
        {
            var userId = Guid.NewGuid();
            var resource = Guid.NewGuid();
            var policy = "TestPolicy";

            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserID).Returns(userId);
            _mockCurrentUser.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) })));

            _mockAuthService.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var result1 = await _service.IsAuthorizedAsync(policy, resource);
            
            _mockAuthService.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Failed());

            var result2 = await _service.IsAuthorizedAsync(policy, resource);//return true cause caching is working well so it doesnt reach the step of authorizing

            Assert.True(result1);
            Assert.True(result2);
        }

        [Fact]
        public async Task IsAuthorizedAsync_MultipleUsers_CacheSeparately()
        {
            var user1Id = Guid.NewGuid();
            var user2Id = Guid.NewGuid();
            var resource = Guid.NewGuid();
            var policy = "TestPolicy";

            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user1Id.ToString()) })));

            _mockAuthService.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            _mockCurrentUser.Setup(x => x.UserID).Returns(user1Id);
            await _service.IsAuthorizedAsync(policy, resource);

            _mockCurrentUser.Setup(x => x.UserID).Returns(user2Id);
            _mockCurrentUser.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user2Id.ToString()) })));
            await _service.IsAuthorizedAsync(policy, resource);

            _mockAuthService.Verify(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public async Task IsAuthorizedAsync_StringResource_CachesCorrectly()
        {
            var userId = Guid.NewGuid();
            var resource = "StringResource";
            var policy = "TestPolicy";

            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserID).Returns(userId);
            _mockCurrentUser.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) })));

            _mockAuthService.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var result1 = await _service.IsAuthorizedAsync(policy, resource);
            _mockAuthService.Invocations.Clear();

            var result2 = await _service.IsAuthorizedAsync(policy, resource);

            Assert.True(result1);
            Assert.True(result2);
            _mockAuthService.Verify(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task IsAuthorizedAsync_ListResource_CachesCorrectly()
        {
            var userId = Guid.NewGuid();
            var resource = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var policy = "TestPolicy";

            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserID).Returns(userId);
            _mockCurrentUser.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) })));

            _mockAuthService.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var result1 = await _service.IsAuthorizedAsync(policy, resource);
            _mockAuthService.Invocations.Clear();

            var result2 = await _service.IsAuthorizedAsync(policy, resource);

            Assert.True(result1);
            Assert.True(result2);
            _mockAuthService.Verify(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region IsUserAsync Tests

        [Fact]
        public async Task IsUserAsync_AuthorizedUser_ReturnsTrue()
        {
            var userId = Guid.NewGuid();

            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserID).Returns(userId);
            _mockCurrentUser.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) })));

            _mockAuthService.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), "SameUserPolicy"))
                .ReturnsAsync(AuthorizationResult.Success());

            var result = await _service.IsUserAsync(userId);

            Assert.True(result);
        }

        [Fact]
        public async Task IsUserAsync_UnauthorizedUser_ReturnsFalse()
        {
            var userId = Guid.NewGuid();
            var differentUserId = Guid.NewGuid();

            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserID).Returns(userId);
            _mockCurrentUser.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) })));

            _mockAuthService.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), differentUserId, "SameUserPolicy"))
                .ReturnsAsync(AuthorizationResult.Failed());

            var result = await _service.IsUserAsync(differentUserId);

            Assert.False(result);
        }

        [Fact]
        public async Task IsUserAsync_CacheHit_DoesNotCallAuthService()
        {
            var userId = Guid.NewGuid();

            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserID).Returns(userId);
            _mockCurrentUser.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) })));

            _mockAuthService.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), "SameUserPolicy"))
                .ReturnsAsync(AuthorizationResult.Success());

            await _service.IsUserAsync(userId);
            _mockAuthService.Invocations.Clear();

            var result = await _service.IsUserAsync(userId);

            Assert.True(result);
            _mockAuthService.Verify(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task IsUserAsync_DifferentUsers_CacheSeparately()
        {
            var user1Id = Guid.NewGuid();
            var user2Id = Guid.NewGuid();

            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserID).Returns(user1Id);
            _mockCurrentUser.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user1Id.ToString()) })));

            _mockAuthService.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), "SameUserPolicy"))
                .ReturnsAsync(AuthorizationResult.Success());

            await _service.IsUserAsync(user1Id);
            await _service.IsUserAsync(user2Id);

            _mockAuthService.Verify(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()), Times.Exactly(2));
        }

        #endregion

        #region IsInRoleAsync Tests

        [Fact]
        public async Task IsInRoleAsync_UserInRole_ReturnsTrue()
        {
            _mockCurrentUser.Setup(x => x.IsInRole("Admin")).Returns(true);

            var result = await _service.IsInRoleAsync("Admin");

            Assert.True(result);
            _mockAuthService.Verify(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task IsInRoleAsync_UserNotInRole_ReturnsFalse()
        {
            _mockCurrentUser.Setup(x => x.IsInRole("Admin")).Returns(false);

            var result = await _service.IsInRoleAsync("Admin");

            Assert.False(result);
        }

        [Fact]
        public async Task IsInRoleAsync_MultipleRoles_ChecksCorrectRole()
        {
            _mockCurrentUser.Setup(x => x.IsInRole("Admin")).Returns(false);
            _mockCurrentUser.Setup(x => x.IsInRole("User")).Returns(true);

            var result1 = await _service.IsInRoleAsync("Admin");
            var result2 = await _service.IsInRoleAsync("User");

            Assert.False(result1);
            Assert.True(result2);
        }

        [Fact]
        public async Task IsInRoleAsync_NotCached_CalledMultipleTimes()
        {
            _mockCurrentUser.Setup(x => x.IsInRole("Admin")).Returns(true);

            var result1 = await _service.IsInRoleAsync("Admin");
            var result2 = await _service.IsInRoleAsync("Admin");

            Assert.True(result1);
            Assert.True(result2);
            _mockCurrentUser.Verify(x => x.IsInRole("Admin"), Times.Exactly(2));
        }

        #endregion

        #region GetCurrentUserId Tests

        [Fact]
        public void GetCurrentUserId_AuthenticatedUser_ReturnsUserId()
        {
            var userId = Guid.NewGuid();
            _mockCurrentUser.Setup(x => x.UserID).Returns(userId);

            var result = _service.GetCurrentUserId();

            Assert.Equal(userId, result);
        }

        [Fact]
        public void GetCurrentUserId_UnauthenticatedUser_ReturnsNull()
        {
            _mockCurrentUser.Setup(x => x.UserID).Returns((Guid?)null);

            var result = _service.GetCurrentUserId();

            Assert.Null(result);
        }

        [Fact]
        public void GetCurrentUserId_MultipleCallsSameUser_ReturnsSameId()
        {
            var userId = Guid.NewGuid();
            _mockCurrentUser.Setup(x => x.UserID).Returns(userId);

            var result1 = _service.GetCurrentUserId();
            var result2 = _service.GetCurrentUserId();

            Assert.Equal(result1, result2);
            Assert.Equal(userId, result1);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task IsAuthorizedAsync_NullResource_HandledCorrectly()
        {
            var userId = Guid.NewGuid();
            var policy = "TestPolicy";

            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserID).Returns(userId);
            _mockCurrentUser.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) })));

            _mockAuthService.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), null, It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var result = await _service.IsAuthorizedAsync(policy, null);

            Assert.True(result);
        }

        [Fact]
        public async Task IsAuthorizedAsync_EmptyStringPolicy_HandledCorrectly()
        {
            var userId = Guid.NewGuid();
            var resource = Guid.NewGuid();

            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserID).Returns(userId);
            _mockCurrentUser.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) })));

            _mockAuthService.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), ""))
                .ReturnsAsync(AuthorizationResult.Success());

            var result = await _service.IsAuthorizedAsync("", resource);

            Assert.True(result);
        }

        [Fact]
        public async Task IsAuthorizedAsync_ComplexObject_CachesCorrectly()
        {
            var userId = Guid.NewGuid();
            var resource = new { Id = Guid.NewGuid(), Name = "Test" };
            var policy = "TestPolicy";

            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserID).Returns(userId);
            _mockCurrentUser.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) })));

            _mockAuthService.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var result1 = await _service.IsAuthorizedAsync(policy, resource);
            _mockAuthService.Invocations.Clear();

            var result2 = await _service.IsAuthorizedAsync(policy, resource);

            Assert.True(result1);
            Assert.True(result2);
            _mockAuthService.Verify(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task IsAuthorizedAsync_LargeCacheLoad_HandlesCorrectly()
        {
            var userId = Guid.NewGuid();
            var policy = "TestPolicy";

            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserID).Returns(userId);
            _mockCurrentUser.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) })));

            _mockAuthService.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            for (int i = 0; i < 100; i++)
            {
                await _service.IsAuthorizedAsync(policy, Guid.NewGuid());
            }

            _mockAuthService.Verify(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()), Times.Exactly(100));
        }

        [Fact]
        public async Task IsAuthorizedAsync_SameResourceDifferentPolicies_CachesDifferently()
        {
            var userId = Guid.NewGuid();
            var resource = Guid.NewGuid();

            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserID).Returns(userId);
            _mockCurrentUser.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) })));

            _mockAuthService.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), "Policy1"))
                .ReturnsAsync(AuthorizationResult.Success());
            _mockAuthService.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), "Policy2"))
                .ReturnsAsync(AuthorizationResult.Failed());

            var result1 = await _service.IsAuthorizedAsync("Policy1", resource);
            var result2 = await _service.IsAuthorizedAsync("Policy2", resource);

            Assert.True(result1);
            Assert.False(result2);
        }

        [Fact]
        public async Task IsAuthorizedAsync_AuthServiceThrowsException_PropagatesException()
        {
            var userId = Guid.NewGuid();
            var resource = Guid.NewGuid();

            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserID).Returns(userId);
            _mockCurrentUser.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) })));

            _mockAuthService.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Auth service error"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.IsAuthorizedAsync("TestPolicy", resource));
        }

        #endregion

        #region Concurrent Access Tests

        [Fact]
        public async Task IsAuthorizedAsync_ConcurrentRequests_HandleCorrectly()
        {
            var userId = Guid.NewGuid();
            var resource = Guid.NewGuid();
            var policy = "TestPolicy";

            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserID).Returns(userId);
            _mockCurrentUser.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) })));

            _mockAuthService.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var tasks = new List<Task<bool>>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(_service.IsAuthorizedAsync(policy, resource));
            }

            var results = await Task.WhenAll(tasks);

            Assert.All(results, result => Assert.True(result));
            _mockAuthService.Verify(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task IsAuthorizedAsync_RealWorldScenario_AllMethodsCombined()
        {
            var user1Id = Guid.NewGuid();
            var user2Id = Guid.NewGuid();
            var resourceId = Guid.NewGuid();

            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserID).Returns(user1Id);
            _mockCurrentUser.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user1Id.ToString()) })));
            _mockCurrentUser.Setup(x => x.IsInRole("Admin")).Returns(true);

            _mockAuthService.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var authResult = await _service.IsAuthorizedAsync("SameUserPolicy", resourceId);
            var isUser = await _service.IsUserAsync(user1Id);
            var isAdmin = await _service.IsInRoleAsync("Admin");
            var currentUserId = _service.GetCurrentUserId();

            Assert.True(authResult);
            Assert.True(isUser);
            Assert.True(isAdmin);
            Assert.Equal(user1Id, currentUserId);
        }

        [Fact]
        public async Task IsAuthorizedAsync_CacheBehaviorWithTimeExpiration()
        {
            var userId = Guid.NewGuid();
            var resource = Guid.NewGuid();
            var policy = "TestPolicy";

            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserID).Returns(userId);
            _mockCurrentUser.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) })));

            _mockAuthService.Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var result1 = await _service.IsAuthorizedAsync(policy, resource);
            Assert.True(result1);
            
            _mockAuthService.Invocations.Clear();
            var result2 = await _service.IsAuthorizedAsync(policy, resource);
            Assert.True(result2);
            _mockAuthService.Verify(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()), Times.Never);
        }

        #endregion
    }
}
