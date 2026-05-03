using Gym_App.Application.Authorization;
using Gym_App.Application.Hubs;
using Gym_App.Application.Services;
using Gym_App.Infastructure.DTOs.Notification;
using Gym_App.Infastructure.Interfaces.Services;
using Moq;
using System.Security.Claims;

namespace GymApp.Tests.NotificationTests
{
    public class NotificationCreationTests : TestBase
    {
        private readonly INotificationService _notificationService;
        private readonly Mock<ICachedAuthorizationService> _authorizationServiceMock;
        private readonly Mock<INotificationSink> _notificationSinkMock;

        public NotificationCreationTests() : base("NotificationCreationTestDatabase")
        {
            _authorizationServiceMock = new Mock<ICachedAuthorizationService>();
            _notificationSinkMock = new Mock<INotificationSink>();
            _notificationService = new NotificationService(_unitOfWork, _authorizationServiceMock.Object, _notificationSinkMock.Object);
        }

        // ========================================
        // CREATE NOTIFICATION TESTS
        // ========================================

        /// <summary>
        /// Test Case 1: Successfully create a new notification
        /// Scenario: Valid notification data and user provided
        /// Expected: Notification is created successfully (status = 2)
        /// </summary>
        [Fact]
        public async Task CreateNotification_WithValidData_ShouldSucceed()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role, email: "testuser@gmail.com");
            await _unitOfWork.SaveChangesAsync();

            _authorizationServiceMock.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            var notificationDto = new NotificationCreationDTO
            {
                Title = "Workout Reminder",
                Content = "Time for your scheduled workout"
            };

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT
            var result = await _notificationService.CreateNotification(user.Id, notificationDto);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(2, result.status);
            Assert.Equal("Successful", result.msg);
        }

        /// <summary>
        /// Test Case 2: Create notification with null input
        /// Scenario: Null notification data provided
        /// Expected: Should fail with status 0 (Bad Request)
        /// </summary>
        [Fact]
        public async Task CreateNotification_WithNullInput_ShouldFail()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);
            await _unitOfWork.SaveChangesAsync();

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            NotificationCreationDTO nullNotification = null;

            // ACT
            var result = await _notificationService.CreateNotification(user.Id, nullNotification);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(0, result.status);
            Assert.Equal("Faulty DTO", result.msg);
        }

        /// <summary>
        /// Test Case 3: Create notification for non-existent user
        /// Scenario: User ID doesn't exist in database
        /// Expected: Should fail with status 0
        /// </summary>
        [Fact]
        public async Task CreateNotification_WithNonExistentUser_ShouldFail()
        {
            // ARRANGE
            var nonExistentUserId = Guid.NewGuid();

            var notificationDto = new NotificationCreationDTO
            {
                Title = "Test Notification",
                Content = "Test Content"
            };

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, nonExistentUserId.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT
            var result = await _notificationService.CreateNotification(nonExistentUserId, notificationDto);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(0, result.status);
            Assert.Equal("User not found", result.msg);
        }

        /// <summary>
        /// Test Case 4: Create notification with unauthorized user
        /// Scenario: User doesn't have permission to create notification for another user
        /// Expected: Should fail with status 1 (Unauthorized)
        /// </summary>
        [Fact]
        public async Task CreateNotification_WithUnauthorizedUser_ShouldFail()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);
            var unauthorizedUser = CreateTestUser(role, email: "other@gmail.com");
            await _unitOfWork.SaveChangesAsync();

            _authorizationServiceMock.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(false);

            var notificationDto = new NotificationCreationDTO
            {
                Title = "Unauthorized Notification",
                Content = "This should not be created"
            };

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, unauthorizedUser.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT
            var result = await _notificationService.CreateNotification(user.Id, notificationDto);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(1, result.status);
            Assert.Equal("Faulty DTO", result.msg);
        }

        /// <summary>
        /// Test Case 5: Create notification with empty title
        /// Scenario: Notification title is null or empty
        /// Expected: Should succeed (service doesn't validate empty title)
        /// </summary>
        [Fact]
        public async Task CreateNotification_WithEmptyTitle_ShouldSucceed()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);
            await _unitOfWork.SaveChangesAsync();

            _authorizationServiceMock.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            var notificationDto = new NotificationCreationDTO
            {
                Title = "",
                Content = "Content without title"
            };

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT
            var result = await _notificationService.CreateNotification(user.Id, notificationDto);

            // ASSERT
            Assert.Equal(2, result.status);
            Assert.Equal("Successful", result.msg);
        }

        /// <summary>
        /// Test Case 6: Create notification with null content
        /// Scenario: Notification content is null (title required, content optional)
        /// Expected: Should succeed
        /// </summary>
        [Fact]
        public async Task CreateNotification_WithNullContent_ShouldSucceed()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);
            await _unitOfWork.SaveChangesAsync();

            _authorizationServiceMock.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            var notificationDto = new NotificationCreationDTO
            {
                Title = "Title Only",
                Content = null
            };

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT
            var result = await _notificationService.CreateNotification(user.Id, notificationDto);

            // ASSERT
            Assert.Equal(2, result.status);
            Assert.Equal("Successful", result.msg);
        }

        /// <summary>
        /// Test Case 7: Create multiple notifications for same user
        /// Scenario: User receives multiple notifications
        /// Expected: All notifications should be created successfully
        /// </summary>
        [Fact]
        public async Task CreateMultipleNotifications_ForSameUser_ShouldSucceed()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);
            await _unitOfWork.SaveChangesAsync();

            _authorizationServiceMock.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            var notification1Dto = new NotificationCreationDTO { Title = "Notification 1", Content = "Content 1" };
            var notification2Dto = new NotificationCreationDTO { Title = "Notification 2", Content = "Content 2" };
            var notification3Dto = new NotificationCreationDTO { Title = "Notification 3", Content = "Content 3" };

            // ACT
            var result1 = await _notificationService.CreateNotification(user.Id, notification1Dto);
            var result2 = await _notificationService.CreateNotification(user.Id, notification2Dto);
            var result3 = await _notificationService.CreateNotification(user.Id, notification3Dto);

            // ASSERT
            Assert.Equal(2, result1.status);
            Assert.Equal(2, result2.status);
            Assert.Equal(2, result3.status);
        }

        /// <summary>
        /// Test Case 8: Create notification with long title and content
        /// Scenario: Notification with maximum length strings
        /// Expected: Should succeed
        /// </summary>
        [Fact]
        public async Task CreateNotification_WithLongContent_ShouldSucceed()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);
            await _unitOfWork.SaveChangesAsync();

            _authorizationServiceMock.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            var longTitle = new string('A', 100);
            var longContent = new string('B', 1000);

            var notificationDto = new NotificationCreationDTO
            {
                Title = longTitle,
                Content = longContent
            };

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT
            var result = await _notificationService.CreateNotification(user.Id, notificationDto);

            // ASSERT
            Assert.Equal(2, result.status);
            Assert.Equal("Successful", result.msg);
        }
    }
}
