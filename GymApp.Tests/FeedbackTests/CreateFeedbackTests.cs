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
    public class CreateFeedbackTests : TestBase
    {
        private readonly IFeedbackService _feedbackService;
        private readonly Mock<IAuthorizationService> _authorizationServiceMock;

        public CreateFeedbackTests() : base("FeedbackTestDatabase")
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

        private async Task<(User user, Workout workout, Role role)> SetupTestUserWithWorkout()
        {
            var role = CreateTestRole("User");
            var user = CreateTestUser(role);
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            return (user, workout, role);
        }
        #endregion

        #region Success Tests
        [Fact]
        public async Task CreateFeedback_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var (user, workout, role) = await SetupTestUserWithWorkout();
            var claimsPrincipal = CreateTestClaimsPrincipal(user.Id.ToString());

            var feedbackDTO = new FeedbackCreationDTO
            {
                Title = "Great Workout",
                Type = "Positive",
                FeedbackText = "Had a very productive session today",
                CaloriesBurned = 500,
                DurationMinutes = 60,
                WorkoutID = workout.Id
            };

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            // Act
            var result = await _feedbackService.CreateFeedback(claimsPrincipal, user.Id, feedbackDTO);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.status); // Success
            Assert.Equal("Success", result.msg);

            // Verify feedback was saved
            var savedFeedback = await _unitOfWork.Feedbacks.GetById(workout.Feedback.Id);
            Assert.NotNull(savedFeedback);
            Assert.Equal("Great Workout", savedFeedback.Title);
            Assert.Equal(500, savedFeedback.CaloriesBurned);
        }

        [Fact]
        public async Task CreateFeedback_WithValidDataAndNoCaloriesBurned_ReturnsSuccess()
        {
            // Arrange
            var (user, workout, role) = await SetupTestUserWithWorkout();
            var claimsPrincipal = CreateTestClaimsPrincipal(user.Id.ToString());

            var feedbackDTO = new FeedbackCreationDTO
            {
                Title = "Quick Session",
                Type = "Neutral",
                FeedbackText = "Short but effective",
                CaloriesBurned = 0,
                DurationMinutes = 0,
                WorkoutID = workout.Id
            };

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            // Act
            var result = await _feedbackService.CreateFeedback(claimsPrincipal, user.Id, feedbackDTO);

            // Assert
            Assert.Equal(2, result.status);
            Assert.Equal("Success", result.msg);
        }
        #endregion

        #region Invalid DTO Tests
        [Fact]
        public async Task CreateFeedback_WithNullDTO_ReturnsBadRequest()
        {
            // Arrange
            var (user, _, _) = await SetupTestUserWithWorkout();
            var claimsPrincipal = CreateTestClaimsPrincipal(user.Id.ToString());

            // Act
            var result = await _feedbackService.CreateFeedback(claimsPrincipal, user.Id, null);

            // Assert
            Assert.Equal(0, result.status); // Bad Request
            Assert.Equal("Invalid DTO", result.msg);
        }
        #endregion

        #region User Not Found Tests
        [Fact]
        public async Task CreateFeedback_WithNonExistentUser_ReturnsBadRequest()
        {
            // Arrange
            var nonExistentUserId = Guid.NewGuid();
            var claimsPrincipal = CreateTestClaimsPrincipal(nonExistentUserId.ToString());

            var feedbackDTO = new FeedbackCreationDTO
            {
                Title = "Test Feedback",
                Type = "Positive",
                FeedbackText = "Test",
                CaloriesBurned = 100,
                DurationMinutes = 30,
                WorkoutID = Guid.NewGuid()
            };

            // Act
            var result = await _feedbackService.CreateFeedback(claimsPrincipal, nonExistentUserId, feedbackDTO);

            // Assert
            Assert.Equal(0, result.status);
            Assert.Equal("User not found", result.msg);
        }
        #endregion

        #region Authorization Tests
        [Fact]
        public async Task CreateFeedback_WithUnauthorizedUser_ReturnsForbidden()
        {
            // Arrange
            var (user, workout, _) = await SetupTestUserWithWorkout();
            var unauthorizedUserId = Guid.NewGuid().ToString();
            var claimsPrincipal = CreateTestClaimsPrincipal(unauthorizedUserId);

            var feedbackDTO = new FeedbackCreationDTO
            {
                Title = "Test Feedback",
                Type = "Positive",
                FeedbackText = "Test",
                CaloriesBurned = 100,
                DurationMinutes = 30,
                WorkoutID = workout.Id
            };

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Failed());

            // Act
            var result = await _feedbackService.CreateFeedback(claimsPrincipal, user.Id, feedbackDTO);

            // Assert
            Assert.Equal(1, result.status); // Unauthorized
            Assert.Equal("Unauthorized", result.msg);
        }
        #endregion

        #region Workout Not Found Tests
        [Fact]
        public async Task CreateFeedback_WithNonExistentWorkout_ReturnsBadRequest()
        {
            // Arrange
            var role = CreateTestRole("User");
            var user = CreateTestUser(role);
            await _unitOfWork.SaveChangesAsync();

            var claimsPrincipal = CreateTestClaimsPrincipal(user.Id.ToString());

            var feedbackDTO = new FeedbackCreationDTO
            {
                Title = "Test Feedback",
                Type = "Positive",
                FeedbackText = "Test",
                CaloriesBurned = 100,
                DurationMinutes = 30,
                WorkoutID = Guid.NewGuid()
            };

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            // Act
            var result = await _feedbackService.CreateFeedback(claimsPrincipal, user.Id, feedbackDTO);

            // Assert
            Assert.Equal(0, result.status);
            Assert.Equal("Workout not found", result.msg);
        }
        #endregion

        #region Existing Feedback Tests
        [Fact]
        public async Task CreateFeedback_WhenFeedbackAlreadyExists_ReturnsBadRequest()
        {
            // Arrange
            var (user, workout, _) = await SetupTestUserWithWorkout();

            // Create existing feedback
            var existingFeedback = new Feedback
            {
                Id = Guid.NewGuid(),
                Title = "Existing Feedback",
                Type = "Positive",
                FeedbackText = "Already exists",
                CreatedAt = DateTime.UtcNow,
                User = user,
                Workout = workout,
                WorkoutID = workout.Id
            };

            await _unitOfWork.Feedbacks.Create(existingFeedback);
            workout.Feedback = existingFeedback;
            await _unitOfWork.Workouts.Update(workout);
            await _unitOfWork.SaveChangesAsync();

            var claimsPrincipal = CreateTestClaimsPrincipal(user.Id.ToString());

            var feedbackDTO = new FeedbackCreationDTO
            {
                Title = "New Feedback",
                Type = "Positive",
                FeedbackText = "Should fail",
                CaloriesBurned = 100,
                DurationMinutes = 30,
                WorkoutID = workout.Id
            };

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            // Act
            var result = await _feedbackService.CreateFeedback(claimsPrincipal, user.Id, feedbackDTO);

            // Assert
            Assert.Equal(0, result.status);
            Assert.Equal("Feedback already exists", result.msg);
        }
        #endregion

        #region Workout Belongs To User Tests
        [Fact]
        public async Task CreateFeedback_WithWorkoutNotBelongingToUser_ReturnsBadRequest()
        {
            // Arrange
            var role = CreateTestRole("User");
            var user1 = CreateTestUser(role, email: "user1@gmail.com");
            var user2 = CreateTestUser(role, email: "user2@gmail.com");
            var workoutOfUser2 = CreateTestWorkout(user2);
            await _unitOfWork.SaveChangesAsync();

            var claimsPrincipal = CreateTestClaimsPrincipal(user1.Id.ToString());

            var feedbackDTO = new FeedbackCreationDTO
            {
                Title = "Test Feedback",
                Type = "Positive",
                FeedbackText = "Test",
                CaloriesBurned = 100,
                DurationMinutes = 30,
                WorkoutID = workoutOfUser2.Id
            };

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            // Act
            var result = await _feedbackService.CreateFeedback(claimsPrincipal, user1.Id, feedbackDTO);

            // Assert - Will return BadRequest because GetWorkoutByUserId only looks for user1's workouts
            Assert.Equal(0, result.status);
            Assert.Equal("Workout not found", result.msg);
        }
        #endregion
    }
}
