using Gym_App.Application.Services;
using Gym_App.Domain;
using Gym_App.Infastructure.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Moq;
using System.Security.Claims;

namespace GymApp.Tests.NotificationTests
{
    public class NotificationDeletionTests : TestBase
    {
        private readonly INotificationService _notificationService;
        private readonly Mock<IAuthorizationService> _authorizationServiceMock;

        public NotificationDeletionTests() : base("NotificationDeletionTestDatabase")
        {
            _authorizationServiceMock = new Mock<IAuthorizationService>();
            _notificationService = new NotificationService(_unitOfWork, _authorizationServiceMock.Object);
        }

        // ========================================
        // DELETE NOTIFICATION TESTS
        // ========================================

        /// <summary>
        /// Test Case 1: Successfully delete a single notification
        /// Scenario: Valid notification ID provided and user authorized
        /// Expected: Notification is deleted successfully (status = 2)
        /// </summary>
        [Fact]
        public async Task DeleteNotification_WithValidId_ShouldSucceed()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role, email: "testuser@gmail.com");
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                Title = "Notification to Delete",
                Content = "Delete me",
                User = user,
                CreatedAt = DateTime.UtcNow
            };
            _unitOfWork.Notifications.Create(notification);
            await _unitOfWork.SaveChangesAsync();

            _authorizationServiceMock
                .Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT
            var result = await _notificationService.DeleteNotification(principal, notification.Id);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(2, result.status);
            Assert.Equal("Successful", result.msg);
        }

        /// <summary>
        /// Test Case 2: Delete notification with empty GUID
        /// Scenario: Empty GUID provided as notification ID
        /// Expected: Should fail with status 0
        /// </summary>
        [Fact]
        public async Task DeleteNotification_WithEmptyGuid_ShouldFail()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);
            await _unitOfWork.SaveChangesAsync();

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT
            var result = await _notificationService.DeleteNotification(principal, Guid.Empty);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(0, result.status);
            Assert.Equal("Invalid NotificationID", result.msg);
        }

        /// <summary>
        /// Test Case 3: Delete non-existent notification
        /// Scenario: Notification ID doesn't exist in database
        /// Expected: Should fail with status 0
        /// </summary>
        [Fact]
        public async Task DeleteNotification_WithNonExistentId_ShouldFail()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);
            await _unitOfWork.SaveChangesAsync();

            var nonExistentNotificationId = Guid.NewGuid();

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT
            var result = await _notificationService.DeleteNotification(principal, nonExistentNotificationId);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(0, result.status);
            Assert.Equal("Notification not found", result.msg);
        }

        /// <summary>
        /// Test Case 4: Delete notification with unauthorized user
        /// Scenario: User doesn't have permission to delete another user's notification
        /// Expected: Should fail with status 1 (Unauthorized)
        /// </summary>
        [Fact]
        public async Task DeleteNotification_WithUnauthorizedUser_ShouldFail()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                Title = "Test Notification",
                Content = "Test",
                User = user,
                CreatedAt = DateTime.UtcNow
            };
            _unitOfWork.Notifications.Create(notification);

            var unauthorizedUser = CreateTestUser(role, email: "other@gmail.com");
            await _unitOfWork.SaveChangesAsync();

            _authorizationServiceMock
                .Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Failed());

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, unauthorizedUser.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT
            var result = await _notificationService.DeleteNotification(principal, notification.Id);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(1, result.status);
            Assert.Equal("Unauthorized", result.msg);
        }

        /// <summary>
        /// Test Case 5: Delete multiple notifications sequentially
        /// Scenario: Delete multiple notifications one after another
        /// Expected: All deletions should succeed
        /// </summary>
        [Fact]
        public async Task DeleteMultipleNotifications_Sequentially_ShouldSucceed()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);

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
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Notifications.Create(notification1);
            _unitOfWork.Notifications.Create(notification2);
            await _unitOfWork.SaveChangesAsync();

            _authorizationServiceMock
                .Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT
            var result1 = await _notificationService.DeleteNotification(principal, notification1.Id);
            var result2 = await _notificationService.DeleteNotification(principal, notification2.Id);

            // ASSERT
            Assert.Equal(2, result1.status);
            Assert.Equal(2, result2.status);
        }

        /// <summary>
        /// Test Case 6: Delete all notifications for a user
        /// Scenario: Delete all notifications associated with a user
        /// Expected: All notifications deleted successfully (status = 2)
        /// </summary>
        [Fact]
        public async Task DeleteAllNotifications_ForUser_ShouldSucceed()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);

            var notification1 = new Notification
            {
                Id = Guid.NewGuid(),
                Title = "Notify 1",
                Content = "Content 1",
                User = user,
                CreatedAt = DateTime.UtcNow
            };
            var notification2 = new Notification
            {
                Id = Guid.NewGuid(),
                Title = "Notify 2",
                Content = "Content 2",
                User = user,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Notifications.Create(notification1);
            _unitOfWork.Notifications.Create(notification2);
            await _unitOfWork.SaveChangesAsync();

            _authorizationServiceMock
                .Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT
            var result = await _notificationService.DeleteAllNotifications(principal, user.Id);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(2, result.status);
            Assert.Equal("Successful", result.msg);
        }

        /// <summary>
        /// Test Case 7: Delete all notifications with invalid user ID
        /// Scenario: Empty GUID provided as user ID
        /// Expected: Should fail with status 0
        /// </summary>
        [Fact]
        public async Task DeleteAllNotifications_WithEmptyUserId_ShouldFail()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);
            await _unitOfWork.SaveChangesAsync();

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT
            var result = await _notificationService.DeleteAllNotifications(principal, Guid.Empty);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(0, result.status);
            Assert.Equal("Invalid UserID", result.msg);
        }

        /// <summary>
        /// Test Case 8: Delete all notifications for non-existent user
        /// Scenario: User ID doesn't exist in database
        /// Expected: Should fail with status 0
        /// </summary>
        [Fact]
        public async Task DeleteAllNotifications_WithNonExistentUser_ShouldFail()
        {
            // ARRANGE
            var nonExistentUserId = Guid.NewGuid();

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, nonExistentUserId.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT
            var result = await _notificationService.DeleteAllNotifications(principal, nonExistentUserId);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(0, result.status);
            Assert.Equal("User not found", result.msg);
        }

        /// <summary>
        /// Test Case 9: Delete all notifications with unauthorized user
        /// Scenario: User doesn't have permission to delete another user's notifications
        /// Expected: Should fail with status 1
        /// </summary>
        [Fact]
        public async Task DeleteAllNotifications_WithUnauthorizedUser_ShouldFail()
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

            // ACT
            var result = await _notificationService.DeleteAllNotifications(principal, user.Id);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(1, result.status);
            Assert.Equal("Faulty DTO", result.msg);
        }

        /// <summary>
        /// Test Case 10: Delete same notification twice
        /// Scenario: Attempt to delete a notification that was already deleted
        /// Expected: Second deletion should fail with status 0
        /// </summary>
        [Fact]
        public async Task DeleteNotification_DeleteTwice_SecondShouldFail()
        {
            // ARRANGE
            var role = CreateTestRole();
            var user = CreateTestUser(role);
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                Title = "Delete Twice",
                Content = "Test",
                User = user,
                CreatedAt = DateTime.UtcNow
            };
            _unitOfWork.Notifications.Create(notification);
            await _unitOfWork.SaveChangesAsync();

            _authorizationServiceMock
                .Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // ACT - Delete once
            var result1 = await _notificationService.DeleteNotification(principal, notification.Id);

            // Try to delete again
            var result2 = await _notificationService.DeleteNotification(principal, notification.Id);

            // ASSERT
            Assert.Equal(2, result1.status);
            Assert.Equal("Successful", result1.msg);

            Assert.Equal(0, result2.status);
            Assert.Equal("Notification not found", result2.msg);
        }
    }
}
