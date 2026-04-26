using Gym_App.Application.Services;
using Gym_App.Domain;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Moq;
using System.Security.Claims;

namespace GymApp.Tests.FeedbackTests
{
    public class DeleteFeedbackTests : TestBase
    {
        private readonly IFeedbackService _feedbackService;
        private readonly Mock<IAuthorizationService> _authorizationServiceMock;

        public DeleteFeedbackTests() : base("FeedbackDeleteTestDatabase")
        {
            _authorizationServiceMock = new Mock<IAuthorizationService>();
            _feedbackService = new FeedbackService(_unitOfWork, _authorizationServiceMock.Object);
        }

        #region Setup Helpers
        private ClaimsPrincipal CreateTestClaimsPrincipal(string userId = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId ?? Guid.NewGuid().ToString())
            };
            var identity = new ClaimsIdentity(claims);
            return new ClaimsPrincipal(identity);
        }

        private async Task<(Feedback feedback, User user, Workout workout)> SetupFeedbackWithUserAndWorkout()
        {
            var role = CreateTestRole("User");
            var user = CreateTestUser(role);
            var workout = CreateTestWorkout(user);

            var feedback = new Feedback
            {
                Id = Guid.NewGuid(),
                Title = "Feedback to Delete",
                Type = "Positive",
                FeedbackText = "This feedback will be deleted",
                CaloriesBurned = 300,
                DurationMinutes = 45,
                CreatedAt = DateTime.UtcNow,
                User = user,
                Workout = workout,
                WorkoutID = workout.Id
            };

            await _unitOfWork.Feedbacks.Create(feedback);
            await _unitOfWork.SaveChangesAsync();

            return (feedback, user, workout);
        }
        #endregion

        #region Success Tests
        [Fact]
        public async Task DeleteFeedback_WithValidFeedback_ReturnsSuccess()
        {
            // Arrange
            var (feedback, user, _) = await SetupFeedbackWithUserAndWorkout();
            var claimsPrincipal = CreateTestClaimsPrincipal(user.Id.ToString());
            var feedbackId = feedback.Id;

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            // Act
            var result = await _feedbackService.DeleteFeedback(claimsPrincipal, feedbackId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.status); // Success
            Assert.Equal("Success", result.msg);

            // Verify feedback was deleted
            var deletedFeedback = await _unitOfWork.Feedbacks.GetById(feedbackId);
            Assert.Null(deletedFeedback);
        }

        [Fact]
        public async Task DeleteFeedback_MultipleTimesAfterFirstDelete_ReturnsFeedbackNotFound()
        {
            // Arrange
            var (feedback, user, _) = await SetupFeedbackWithUserAndWorkout();
            var claimsPrincipal = CreateTestClaimsPrincipal(user.Id.ToString());
            var feedbackId = feedback.Id;

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            // Act - First deletion
            var firstResult = await _feedbackService.DeleteFeedback(claimsPrincipal, feedbackId);
            Assert.Equal(2, firstResult.status);

            // Act - Second deletion (should fail)
            var secondResult = await _feedbackService.DeleteFeedback(claimsPrincipal, feedbackId);

            // Assert
            Assert.Equal(0, secondResult.status);
            Assert.Equal("Feedback not found", secondResult.msg);
        }
        #endregion

        #region Invalid ID Tests
        [Fact]
        public async Task DeleteFeedback_WithEmptyGuid_ReturnsBadRequest()
        {
            // Arrange
            var claimsPrincipal = CreateTestClaimsPrincipal(Guid.NewGuid().ToString());

            // Act
            var result = await _feedbackService.DeleteFeedback(claimsPrincipal, Guid.Empty);

            // Assert
            Assert.Equal(0, result.status);
            Assert.Equal("Invalid feedback ID", result.msg);
        }
        #endregion

        #region Feedback Not Found Tests
        [Fact]
        public async Task DeleteFeedback_WithNonExistentFeedback_ReturnsBadRequest()
        {
            // Arrange
            var nonExistentFeedbackId = Guid.NewGuid();
            var claimsPrincipal = CreateTestClaimsPrincipal(Guid.NewGuid().ToString());

            // Act
            var result = await _feedbackService.DeleteFeedback(claimsPrincipal, nonExistentFeedbackId);

            // Assert
            Assert.Equal(0, result.status);
            Assert.Equal("Feedback not found", result.msg);
        }
        #endregion

        #region Authorization Tests
        [Fact]
        public async Task DeleteFeedback_WithUnauthorizedUser_ReturnsForbidden()
        {
            // Arrange
            var (feedback, user, _) = await SetupFeedbackWithUserAndWorkout();
            var unauthorizedUserId = Guid.NewGuid().ToString();
            var claimsPrincipal = CreateTestClaimsPrincipal(unauthorizedUserId);

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Failed());

            // Act
            var result = await _feedbackService.DeleteFeedback(claimsPrincipal, feedback.Id);

            // Assert
            Assert.Equal(1, result.status); // Unauthorized
            Assert.Equal("Unauthorized", result.msg);

            // Verify feedback still exists
            var existingFeedback = await _unitOfWork.Feedbacks.GetById(feedback.Id);
            Assert.NotNull(existingFeedback);
            Assert.Equal(feedback.Id, existingFeedback.Id);
        }

        [Fact]
        public async Task DeleteFeedback_WithDifferentUserClaims_ReturnsForbidden()
        {
            // Arrange
            var role = CreateTestRole("User");
            var feedbackOwner = CreateTestUser(role, email: "owner@gmail.com");
            var unauthorizedUser = CreateTestUser(role, email: "hacker@gmail.com");

            var workout = CreateTestWorkout(feedbackOwner);
            var feedback = new Feedback
            {
                Id = Guid.NewGuid(),
                Title = "Owner's Feedback",
                Type = "Positive",
                FeedbackText = "Owner's feedback",
                CreatedAt = DateTime.UtcNow,
                User = feedbackOwner,
                Workout = workout,
                WorkoutID = workout.Id
            };

            await _unitOfWork.Feedbacks.Create(feedback);
            await _unitOfWork.SaveChangesAsync();

            var unauthorizedClaimsPrincipal = CreateTestClaimsPrincipal(unauthorizedUser.Id.ToString());

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Failed());

            // Act
            var result = await _feedbackService.DeleteFeedback(unauthorizedClaimsPrincipal, feedback.Id);

            // Assert
            Assert.Equal(1, result.status);
            Assert.Equal("Unauthorized", result.msg);

            // Verify feedback was not deleted
            var stillExistingFeedback = await _unitOfWork.Feedbacks.GetById(feedback.Id);
            Assert.NotNull(stillExistingFeedback);
        }
        #endregion

        #region Sequential Delete Tests
        [Fact]
        public async Task DeleteFeedback_SequentialDeletionAttempts_FirstSucceedsSecondFails()
        {
            // Arrange
            var (feedback, user, _) = await SetupFeedbackWithUserAndWorkout();
            var claimsPrincipal = CreateTestClaimsPrincipal(user.Id.ToString());
            var feedbackId = feedback.Id;

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            // Act - First deletion should succeed
            var result1 = await _feedbackService.DeleteFeedback(claimsPrincipal, feedbackId);

            // Second deletion attempt should fail
            var result2 = await _feedbackService.DeleteFeedback(claimsPrincipal, feedbackId);

            // Assert
            Assert.Equal(2, result1.status); // Success
            Assert.Equal(0, result2.status); // Failure
            Assert.Equal("Feedback not found", result2.msg);
        }
        #endregion
    }
}
