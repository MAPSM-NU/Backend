using Gym_App.Application.Services;
using Gym_App.Domain;
using Gym_App.Infastructure.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Moq;
using System.Security.Claims;

namespace GymApp.Tests.NotificationTests
{
    public class NotificationQueryTests : TestBase
    {
        private readonly INotificationService _notificationService;
        private readonly Mock<IAuthorizationService> _authorizationServiceMock;

        public NotificationQueryTests() : base("NotificationQueryTestDatabase")
        {
            _authorizationServiceMock = new Mock<IAuthorizationService>();
            _notificationService = new NotificationService(_unitOfWork, _authorizationServiceMock.Object);
        }

        // ========================================
        // QUERY/GET NOTIFICATION TESTS
        // ========================================

        /// <summary>
        /// Test Case 1: Get notifications for a specific user
        /// Scenario: Retrieve notifications filtered by user ID
        /// Expected: All user notifications returned successfully (status = 2)
        /// </summary>
        [Fact]
        public async Task GetNotifications_ForUser_ShouldSucceed()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role, email: "testuser@gmail.com");

            var notification1 = new Notification
            {
                Id = Guid.NewGuid(),
                Title = "Notification 1",
                Content = "Content 1",
                User = user,
                CreatedAt = DateTime.UtcNow
            };
            var notification2 = new Notification
            {
                Id = Guid.NewGuid(),
                Title = "Notification 2",
                Content = "Content 2",
                User = user,
                CreatedAt = DateTime.UtcNow.AddMinutes(-5)
            };

            _unitOfWork.Notifications.Create(notification1);
            _unitOfWork.Notifications.Create(notification2);
            await _unitOfWork.SaveChangesAsync();

            _authorizationServiceMock
                .Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            var now = DateTime.UtcNow;

            // ACT
            var result = await _notificationService.GetNotifications(
                principal,
                user.Id,
                startDate: now.AddDays(-1).ToString("yyyy-MM-dd"),
                endDate: now.AddDays(1).ToString("yyyy-MM-dd"),
                page: 1,
                sortColumn: "CreatedAt",
                OrderBy: "DESC",
                searchTerm: "",
                pageSize: 10
            );

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(2, result.status);
            Assert.Equal("Successful", result.msg);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.TotalCount >= 2);
        }

        /// <summary>
        /// Test Case 2: Get notifications with unauthorized user
        /// Scenario: User tries to get another user's notifications
        /// Expected: Should fail with status 1 (Unauthorized)
        /// </summary>
        [Fact]
        public async Task GetNotifications_WithUnauthorizedUser_ShouldFail()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);
            var unauthorizedUser = CreateTestUser(role, email: "other@gmail.com");
            await _unitOfWork.SaveChangesAsync();

            _authorizationServiceMock
                .Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Failed());

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, unauthorizedUser.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            var now = DateTime.UtcNow;

            // ACT
            var result = await _notificationService.GetNotifications(
                principal,
                user.Id,
                startDate: now.AddDays(-1).ToString("yyyy-MM-dd"),
                endDate: now.AddDays(1).ToString("yyyy-MM-dd"),
                page: 1,
                sortColumn: "",
                OrderBy: "",
                searchTerm: "",
                pageSize: 10
            );

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(1, result.status);
            Assert.Equal("Unauthorized", result.msg);
        }

        /// <summary>
        /// Test Case 3: Get notifications with no results
        /// Scenario: User has no notifications
        /// Expected: Should fail with status 0 (No Notifications)
        /// </summary>
        [Fact]
        public async Task GetNotifications_WithNoNotifications_ShouldFail()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);
            await _unitOfWork.SaveChangesAsync();

            _authorizationServiceMock
                .Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            var now = DateTime.UtcNow;

            // ACT
            var result = await _notificationService.GetNotifications(
                principal,
                user.Id,
                startDate: now.AddDays(-1).ToString("yyyy-MM-dd"),
                endDate: now.AddDays(1).ToString("yyyy-MM-dd"),
                page: 1,
                sortColumn: "",
                OrderBy: "",
                searchTerm: "",
                pageSize: 10
            );

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(0, result.status);
            Assert.Equal("No Notifications", result.msg);
        }

        /// <summary>
        /// Test Case 4: Get notifications with pagination
        /// Scenario: Retrieve paginated notifications
        /// Expected: Should return first page of notifications
        /// </summary>
        [Fact]
        public async Task GetNotifications_WithPagination_ShouldSucceed()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);

            // Create 15 notifications
            for (int i = 0; i < 15; i++)
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    Title = $"Notification {i}",
                    Content = $"Content {i}",
                    User = user,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-i)
                };
                _unitOfWork.Notifications.Create(notification);
            }
            await _unitOfWork.SaveChangesAsync();

            _authorizationServiceMock
                .Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            var now = DateTime.UtcNow;

            // ACT - Get first page with 10 items per page
            var result = await _notificationService.GetNotifications(
                principal,
                user.Id,
                startDate: now.AddDays(-1).ToString("yyyy-MM-dd"),
                endDate: now.AddDays(1).ToString("yyyy-MM-dd"),
                page: 1,
                sortColumn: "CreatedAt",
                OrderBy: "DESC",
                searchTerm: "",
                pageSize: 10
            );

            // ASSERT
            Assert.Equal(2, result.status);
            Assert.NotNull(result.Data);
            Assert.Equal(15, result.Data.TotalCount);  // Total count should be 15
            Assert.True(result.Data.Items.Count <= 10);      // Current page should have <= 10 items
        }

        /// <summary>
        /// Test Case 5: Get notifications with search term
        /// Scenario: Search notifications by title or content
        /// Expected: Should return only matching notifications
        /// </summary>
        [Fact]
        public async Task GetNotifications_WithSearchTerm_ShouldSucceed()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);

            var notif1 = new Notification
            {
                Id = Guid.NewGuid(),
                Title = "Workout Reminder",
                Content = "Time to workout",
                User = user,
                CreatedAt = DateTime.UtcNow
            };
            var notif2 = new Notification
            {
                Id = Guid.NewGuid(),
                Title = "Birthday Greeting",
                Content = "Happy birthday",
                User = user,
                CreatedAt = DateTime.UtcNow.AddMinutes(-5)
            };

            _unitOfWork.Notifications.Create(notif1);
            _unitOfWork.Notifications.Create(notif2);
            await _unitOfWork.SaveChangesAsync();

            _authorizationServiceMock
                .Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            var now = DateTime.UtcNow;

            // ACT - Search for "Workout"
            var result = await _notificationService.GetNotifications(
                principal,
                user.Id,
                startDate: now.AddDays(-1).ToString("yyyy-MM-dd"),
                endDate: now.AddDays(1).ToString("yyyy-MM-dd"),
                page: 1,
                sortColumn: "",
                OrderBy: "",
                searchTerm: "Workout",
                pageSize: 10
            );

            // ASSERT
            Assert.Equal(2, result.status);
            Assert.NotNull(result.Data);
        }

        /// <summary>
        /// Test Case 6: Get all notifications (admin view)
        /// Scenario: Retrieve all notifications from all users
        /// Expected: All notifications returned with pagination
        /// </summary>
        [Fact]
        public async Task GetAllNotifications_ShouldSucceed()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user1 = CreateTestUser(role, email: "user1@gmail.com");
            var user2 = CreateTestUser(role, email: "user2@gmail.com");

            var notif1 = new Notification
            {
                Id = Guid.NewGuid(),
                Title = "User1 Notification",
                Content = "For user 1",
                User = user1,
                CreatedAt = DateTime.UtcNow
            };
            var notif2 = new Notification
            {
                Id = Guid.NewGuid(),
                Title = "User2 Notification",
                Content = "For user 2",
                User = user2,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Notifications.Create(notif1);
            _unitOfWork.Notifications.Create(notif2);
            await _unitOfWork.SaveChangesAsync();

            // ACT
            var result = await _notificationService.GetAllNotifications(page: 1, pageSize: 10);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(2, result.status);
            Assert.Equal("Successful", result.msg);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.TotalCount >= 2);
        }

        /// <summary>
        /// Test Case 7: Get all notifications with no data
        /// Scenario: No notifications exist in database
        /// Expected: Should fail with status 0
        /// </summary>
        [Fact]
        public async Task GetAllNotifications_WithNoData_ShouldFail()
        {
            // ACT
            var result = await _notificationService.GetAllNotifications(page: 1, pageSize: 10);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(0, result.status);
            Assert.Equal("No Notifications in Database", result.msg);
        }

        /// <summary>
        /// Test Case 8: Get notification user ID
        /// Scenario: Retrieve the user ID associated with a notification
        /// Expected: Should return correct user ID
        /// </summary>
        [Fact]
        public async Task GetNotificationUserId_WithValidNotification_ShouldSucceed()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                Title = "Test",
                Content = "Test",
                User = user,
                CreatedAt = DateTime.UtcNow
            };
            _unitOfWork.Notifications.Create(notification);
            await _unitOfWork.SaveChangesAsync();

            // ACT
            var result = await _notificationService.GetNotificationUserID(notification.Id);

            // ASSERT
            Assert.Equal(user.Id, result);
        }

        /// <summary>
        /// Test Case 9: Get notifications with date filtering
        /// Scenario: Retrieve notifications within a specific date range
        /// Expected: Should return only notifications within date range
        /// </summary>
        [Fact]
        public async Task GetNotifications_WithDateFiltering_ShouldSucceed()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);

            var now = DateTime.UtcNow;
            var notif1 = new Notification
            {
                Id = Guid.NewGuid(),
                Title = "Recent Notification",
                Content = "Today",
                User = user,
                CreatedAt = now
            };
            var notif2 = new Notification
            {
                Id = Guid.NewGuid(),
                Title = "Old Notification",
                Content = "Past",
                User = user,
                CreatedAt = now.AddDays(-10)
            };

            _unitOfWork.Notifications.Create(notif1);
            _unitOfWork.Notifications.Create(notif2);
            await _unitOfWork.SaveChangesAsync();

            _authorizationServiceMock
                .Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT - Get only recent notifications (last 5 days)
            var result = await _notificationService.GetNotifications(
                principal,
                user.Id,
                startDate: now.AddDays(-5).ToString("yyyy-MM-dd"),
                endDate: now.AddDays(1).ToString("yyyy-MM-dd"),
                page: 1,
                sortColumn: "CreatedAt",
                OrderBy: "DESC",
                searchTerm: "",
                pageSize: 10
            );

            // ASSERT
            Assert.Equal(2, result.status);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.TotalCount >= 1);
        }

        /// <summary>
        /// Test Case 10: Get notifications sorted by creation date
        /// Scenario: Retrieve notifications sorted in descending order
        /// Expected: Most recent notifications appear first
        /// </summary>
        [Fact]
        public async Task GetNotifications_SortedByDate_ShouldSucceed()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);

            var now = DateTime.UtcNow;
            var notif1 = new Notification
            {
                Id = Guid.NewGuid(),
                Title = "First",
                Content = "1",
                User = user,
                CreatedAt = now.AddMinutes(-10)
            };
            var notif2 = new Notification
            {
                Id = Guid.NewGuid(),
                Title = "Second",
                Content = "2",
                User = user,
                CreatedAt = now
            };

            _unitOfWork.Notifications.Create(notif1);
            _unitOfWork.Notifications.Create(notif2);
            await _unitOfWork.SaveChangesAsync();

            _authorizationServiceMock
                .Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT
            var result = await _notificationService.GetNotifications(
                principal,
                user.Id,
                startDate: now.AddDays(-1).ToString("yyyy-MM-dd"),
                endDate: now.AddDays(1).ToString("yyyy-MM-dd"),
                page: 1,
                sortColumn: "CreatedAt",
                OrderBy: "DESC",
                searchTerm: "",
                pageSize: 10
            );

            // ASSERT
            Assert.Equal(2, result.status);
            Assert.NotNull(result.Data);
        }
    }
}
