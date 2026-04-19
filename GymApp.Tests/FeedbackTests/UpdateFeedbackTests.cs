using Gym_App.Application.Services;
using Gym_App.Domain;
using Gym_App.Infastructure.DTOs.Feedback;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Moq;
using System.Security.Claims;

namespace GymApp.Tests.FeedbackTests
{
    public class UpdateFeedbackTests : TestBase
    {
        private readonly IFeedbackService _feedbackService;
        private readonly Mock<IAuthorizationService> _authorizationServiceMock;

        public UpdateFeedbackTests() : base("FeedbackUpdateTestDatabase")
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
                Title = "Original Title",
                Type = "Positive",
                FeedbackText = "Original feedback text",
                CaloriesBurned = 300,
                DurationMinutes = 45,
                CreatedAt = DateTime.UtcNow,
                User = user,
                Workout = workout,
                WorkoutID = workout.Id
            };

            await _unitOfWork.Feedbacks.Create(feedback);
            workout.Feedback = feedback;
            await _unitOfWork.Workouts.Update(workout);
            await _unitOfWork.SaveChangesAsync();

            return (feedback, user, workout);
        }
        #endregion

        #region Success Tests
        [Fact]
        public async Task UpdateFeedback_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var (feedback, user, _) = await SetupFeedbackWithUserAndWorkout();
            var claimsPrincipal = CreateTestClaimsPrincipal(user.Id.ToString());

            var feedbackUpdateDTO = new FeedbackUpdateDTO
            {
                Title = "Updated Title",
                Type = "Very Positive",
                FeedbackText = "Updated feedback text",
                CaloriesBurned = 500,
                DurationMinutes = 60
            };

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            // Act
            var result = await _feedbackService.UpdateFeedback(claimsPrincipal, feedback.Id, feedbackUpdateDTO);

            // Assert
            Assert.Equal(2, result.status); // Success
            Assert.Equal("Success", result.msg);

            // Verify the update
            var updatedFeedback = await _unitOfWork.Feedbacks.GetById(feedback.Id);
            Assert.Equal("Updated Title", updatedFeedback.Title);
            Assert.Equal("Very Positive", updatedFeedback.Type);
            Assert.Equal("Updated feedback text", updatedFeedback.FeedbackText);
            Assert.Equal(500, updatedFeedback.CaloriesBurned);
            Assert.Equal(60, updatedFeedback.DurationMinutes);
        }

        [Fact]
        public async Task UpdateFeedback_WithPartialUpdate_ReturnsSuccess()
        {
            // Arrange
            var (feedback, user, _) = await SetupFeedbackWithUserAndWorkout();
            var claimsPrincipal = CreateTestClaimsPrincipal(user.Id.ToString());

            var feedbackUpdateDTO = new FeedbackUpdateDTO
            {
                Title = "Only Title Updated",
                Type = "",
                FeedbackText = "",
                CaloriesBurned = 0,
                DurationMinutes = 0
            };

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            // Act
            var result = await _feedbackService.UpdateFeedback(claimsPrincipal, feedback.Id, feedbackUpdateDTO);

            // Assert
            Assert.Equal(2, result.status);
            Assert.Equal("Success", result.msg);

            // Verify only changed properties were updated
            var updatedFeedback = await _unitOfWork.Feedbacks.GetById(feedback.Id);
            Assert.Equal("Only Title Updated", updatedFeedback.Title);
        }

        [Fact]
        public async Task UpdateFeedback_WithAllFieldsEmpty_ReturnsSuccess()
        {
            // Arrange
            var (feedback, user, _) = await SetupFeedbackWithUserAndWorkout();
            var originalTitle = feedback.Title;
            var claimsPrincipal = CreateTestClaimsPrincipal(user.Id.ToString());

            var feedbackUpdateDTO = new FeedbackUpdateDTO
            {
                Title = "",
                Type = "",
                FeedbackText = "",
                CaloriesBurned = 0,
                DurationMinutes = 0
            };

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            // Act
            var result = await _feedbackService.UpdateFeedback(claimsPrincipal, feedback.Id, feedbackUpdateDTO);

            // Assert
            Assert.Equal(2, result.status);

            // Verify feedback remains unchanged
            var unchangedFeedback = await _unitOfWork.Feedbacks.GetById(feedback.Id);
            Assert.Equal(originalTitle, unchangedFeedback.Title);
        }
        #endregion

        #region Invalid DTO Tests
        [Fact]
        public async Task UpdateFeedback_WithNullDTO_ReturnsBadRequest()
        {
            // Arrange
            var (feedback, user, _) = await SetupFeedbackWithUserAndWorkout();
            var claimsPrincipal = CreateTestClaimsPrincipal(user.Id.ToString());

            // Act
            var result = await _feedbackService.UpdateFeedback(claimsPrincipal, feedback.Id, null);

            // Assert
            Assert.Equal(0, result.status);
            Assert.Equal("Invalid DTO", result.msg);
        }
        #endregion

        #region Feedback Not Found Tests
        [Fact]
        public async Task UpdateFeedback_WithNonExistentFeedback_ReturnsBadRequest()
        {
            // Arrange
            var nonExistentFeedbackId = Guid.NewGuid();
            var claimsPrincipal = CreateTestClaimsPrincipal(Guid.NewGuid().ToString());

            var feedbackUpdateDTO = new FeedbackUpdateDTO
            {
                Title = "Updated Title",
                Type = "Positive",
                FeedbackText = "Updated text",
                CaloriesBurned = 100,
                DurationMinutes = 30
            };

            // Act
            var result = await _feedbackService.UpdateFeedback(claimsPrincipal, nonExistentFeedbackId, feedbackUpdateDTO);

            // Assert
            Assert.Equal(0, result.status);
            Assert.Equal("Feedback not found", result.msg);
        }
        #endregion

        #region Authorization Tests
        [Fact]
        public async Task UpdateFeedback_WithUnauthorizedUser_ReturnsForbidden()
        {
            // Arrange
            var (feedback, user, _) = await SetupFeedbackWithUserAndWorkout();
            var unauthorizedUserId = Guid.NewGuid().ToString();
            var claimsPrincipal = CreateTestClaimsPrincipal(unauthorizedUserId);

            var feedbackUpdateDTO = new FeedbackUpdateDTO
            {
                Title = "Updated Title",
                Type = "Positive",
                FeedbackText = "Updated text",
                CaloriesBurned = 100,
                DurationMinutes = 30
            };

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Failed());

            // Act
            var result = await _feedbackService.UpdateFeedback(claimsPrincipal, feedback.Id, feedbackUpdateDTO);

            // Assert
            Assert.Equal(1, result.status); // Unauthorized
            Assert.Equal("Unauthorized", result.msg);
        }

        [Fact]
        public async Task UpdateFeedback_WithDifferentUser_ReturnsForbidden()
        {
            // Arrange
            var role = CreateTestRole("User");
            var user1 = CreateTestUser(role, email: "user1@gmail.com");
            var user2 = CreateTestUser(role, email: "user2@gmail.com");

            var workout = CreateTestWorkout(user1);
            var feedback = new Feedback
            {
                Id = Guid.NewGuid(),
                Title = "User1's Feedback",
                Type = "Positive",
                FeedbackText = "User1's feedback",
                CreatedAt = DateTime.UtcNow,
                User = user1,
                Workout = workout,
                WorkoutID = workout.Id
            };

            await _unitOfWork.Feedbacks.Create(feedback);
            await _unitOfWork.SaveChangesAsync();

            var claimsPrincipal = CreateTestClaimsPrincipal(user2.Id.ToString());

            var feedbackUpdateDTO = new FeedbackUpdateDTO
            {
                Title = "Hacked Title",
                Type = "Negative",
                FeedbackText = "Hacked feedback",
                CaloriesBurned = 0,
                DurationMinutes = 0
            };

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Failed());

            // Act
            var result = await _feedbackService.UpdateFeedback(claimsPrincipal, feedback.Id, feedbackUpdateDTO);

            // Assert
            Assert.Equal(1, result.status);
            Assert.Equal("Unauthorized", result.msg);
        }
        #endregion
    }
}
