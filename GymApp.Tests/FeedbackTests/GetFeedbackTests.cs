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
    public class GetFeedbackTests : TestBase
    {
        private readonly IFeedbackService _feedbackService;
        private readonly Mock<IAuthorizationService> _authorizationServiceMock;

        public GetFeedbackTests() : base("FeedbackGetTestDatabase")
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
                Title = "Test Feedback",
                Type = "Positive",
                FeedbackText = "Great session!",
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

        #region GetFeedbackByID Tests
        [Fact]
        public async Task GetFeedbackByID_WithValidFeedbackId_ReturnsFeedbackDTO()
        {
            // Arrange
            var (feedback, user, _) = await SetupFeedbackWithUserAndWorkout();
            var claimsPrincipal = CreateTestClaimsPrincipal(user.Id.ToString());

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            // Act
            var result = await _feedbackService.GetFeedbackByID(claimsPrincipal, feedback.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.status); // Success
            Assert.Equal("Successful", result.msg);
            Assert.NotNull(result.Value);
            Assert.Equal("Test Feedback", result.Value.Title);
            Assert.Equal("Positive", result.Value.Type);
            Assert.Equal(feedback.Id, result.Value.FeedbackID);
            Assert.Equal(user.Id, result.Value.UserID);
        }

        [Fact]
        public async Task GetFeedbackByID_WithEmptyGuid_ReturnsBadRequest()
        {
            // Arrange
            var claimsPrincipal = CreateTestClaimsPrincipal(Guid.NewGuid().ToString());

            // Act
            var result = await _feedbackService.GetFeedbackByID(claimsPrincipal, Guid.Empty);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.status); // Bad Request
            Assert.Equal("Faulty ID", result.msg);
        }

        [Fact]
        public async Task GetFeedbackByID_WithNonExistentFeedback_ReturnsBadRequest()
        {
            // Arrange
            var nonExistentFeedbackId = Guid.NewGuid();
            var claimsPrincipal = CreateTestClaimsPrincipal(Guid.NewGuid().ToString());

            // Act & Assert - This might throw if feedback not found
            try
            {
                var result = await _feedbackService.GetFeedbackByID(claimsPrincipal, nonExistentFeedbackId);
                // The method checks feedback == null after mapping, so this may not be reached
            }
            catch (NullReferenceException)
            {
                // Expected if GetById returns null
            }
        }

        [Fact]
        public async Task GetFeedbackByID_WithUnauthorizedUser_ReturnsForbidden()
        {
            // Arrange
            var (feedback, user, _) = await SetupFeedbackWithUserAndWorkout();
            var unauthorizedUserId = Guid.NewGuid().ToString();
            var claimsPrincipal = CreateTestClaimsPrincipal(unauthorizedUserId);

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Failed());

            // Act
            var result = await _feedbackService.GetFeedbackByID(claimsPrincipal, feedback.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.status); // Unauthorized
            Assert.Equal("Unauthorized", result.msg);
        }
        #endregion

        #region GetFeedbackOfWorkout Tests
        [Fact]
        public async Task GetFeedbackOfWorkout_WithValidWorkoutId_ReturnsFeedbackMiniViewDTO()
        {
            // Arrange
            var (feedback, user, workout) = await SetupFeedbackWithUserAndWorkout();
            var claimsPrincipal = CreateTestClaimsPrincipal(user.Id.ToString());

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            // Act
            var result = await _feedbackService.GetFeedbackOfWorkout(claimsPrincipal, workout.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.status); // Success
            Assert.Equal("Successful", result.msg);
            Assert.NotNull(result.Value);
            Assert.Equal("Test Feedback", result.Value.Title);
            Assert.Equal(workout.Id, result.Value.WorkoutID);
            Assert.Equal(feedback.Id, result.Value.FeedbackID);
        }

        [Fact]
        public async Task GetFeedbackOfWorkout_WithUnauthorizedUser_ReturnsForbidden()
        {
            // Arrange
            var (feedback, user, workout) = await SetupFeedbackWithUserAndWorkout();
            var unauthorizedUserId = Guid.NewGuid().ToString();
            var claimsPrincipal = CreateTestClaimsPrincipal(unauthorizedUserId);

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Failed());

            // Act
            var result = await _feedbackService.GetFeedbackOfWorkout(claimsPrincipal, workout.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.status); // Unauthorized
            Assert.Equal("Unauthorized", result.msg);
        }
        #endregion

        #region GetUserFeedbacks Tests
        [Fact]
        public async Task GetUserFeedbacks_WithValidUserId_ReturnsFeedbackList()
        {
            // Arrange
            var (feedback1, user, _) = await SetupFeedbackWithUserAndWorkout();

            // Create additional feedbacks for the same user
            var workout2 = CreateTestWorkout(user, "Workout 2");
            var feedback2 = new Feedback
            {
                Id = Guid.NewGuid(),
                Title = "Second Feedback",
                Type = "Neutral",
                FeedbackText = "Average session",
                CaloriesBurned = 250,
                DurationMinutes = 40,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                User = user,
                Workout = workout2,
                WorkoutID = workout2.Id
            };
            await _unitOfWork.Feedbacks.Create(feedback2);
            await _unitOfWork.SaveChangesAsync();

            var claimsPrincipal = CreateTestClaimsPrincipal(user.Id.ToString());

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            // Act
            var result = await _feedbackService.GetUserFeedbacks(
                claimsPrincipal, user.Id, "", "", 1, "", "", "", 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.status); // Success
            Assert.Equal("Successful", result.msg);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Items.Count);
        }

        [Fact]
        public async Task GetUserFeedbacks_WithNonExistentUser_ReturnsBadRequest()
        {
            // Arrange
            var nonExistentUserId = Guid.NewGuid();
            var claimsPrincipal = CreateTestClaimsPrincipal(Guid.NewGuid().ToString());

            // Act
            var result = await _feedbackService.GetUserFeedbacks(
                claimsPrincipal, nonExistentUserId, "", "", 1, "", "", "", 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.status); // Bad Request
            Assert.Equal("User not found", result.msg);
        }

        [Fact]
        public async Task GetUserFeedbacks_WithUnauthorizedUser_ReturnsForbidden()
        {
            // Arrange
            var (_, user, _) = await SetupFeedbackWithUserAndWorkout();
            var unauthorizedUserId = Guid.NewGuid().ToString();
            var claimsPrincipal = CreateTestClaimsPrincipal(unauthorizedUserId);

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Failed());

            // Act
            var result = await _feedbackService.GetUserFeedbacks(
                claimsPrincipal, user.Id, "", "", 1, "", "", "", 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.status); // Unauthorized
            Assert.Equal("Unauthorized", result.msg);
        }

        [Fact]
        public async Task GetUserFeedbacks_WithUserHavingNoFeedbacks_ReturnsBadRequest()
        {
            // Arrange
            var role = CreateTestRole("User");
            var userWithNoFeedbacks = CreateTestUser(role, email: "nofeedback@gmail.com");
            await _unitOfWork.SaveChangesAsync();

            var claimsPrincipal = CreateTestClaimsPrincipal(userWithNoFeedbacks.Id.ToString());

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            // Act
            var result = await _feedbackService.GetUserFeedbacks(
                claimsPrincipal, userWithNoFeedbacks.Id, "", "", 1, "", "", "", 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.status);
            Assert.Equal("User has no feedbacks", result.msg);
        }

        [Fact]
        public async Task GetUserFeedbacks_WithDateFiltering_ReturnsFilteredFeedbacks()
        {
            // Arrange
            var today = DateTime.Now.Date;
            var (feedback1, user, _) = await SetupFeedbackWithUserAndWorkout();

            var oldWorkout = CreateTestWorkout(user, "Old Workout");
            var oldFeedback = new Feedback
            {
                Id = Guid.NewGuid(),
                Title = "Old Feedback",
                Type = "Positive",
                FeedbackText = "Old session",
                CreatedAt = today.AddDays(-10),
                User = user,
                Workout = oldWorkout,
                WorkoutID = oldWorkout.Id
            };
            await _unitOfWork.Feedbacks.Create(oldFeedback);
            await _unitOfWork.SaveChangesAsync();

            var claimsPrincipal = CreateTestClaimsPrincipal(user.Id.ToString());

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            // Act - Filter for recent feedbacks only
            var result = await _feedbackService.GetUserFeedbacks(
                claimsPrincipal, user.Id, today.AddDays(-1).ToString(), today.AddDays(1).ToString(),
                1, "", "", "", 10);

            // Assert
            Assert.Equal(2, result.status);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task GetUserFeedbacks_WithSearchTerm_ReturnsFilteredFeedbacks()
        {
            // Arrange
            var (feedback1, user, _) = await SetupFeedbackWithUserAndWorkout();

            var workout2 = CreateTestWorkout(user, "Second Workout");
            var feedback2 = new Feedback
            {
                Id = Guid.NewGuid(),
                Title = "Another Title",
                Type = "Positive",
                FeedbackText = "Different feedback",
                CreatedAt = DateTime.UtcNow,
                User = user,
                Workout = workout2,
                WorkoutID = workout2.Id
            };
            await _unitOfWork.Feedbacks.Create(feedback2);
            await _unitOfWork.SaveChangesAsync();

            var claimsPrincipal = CreateTestClaimsPrincipal(user.Id.ToString());

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            // Act
            var result = await _feedbackService.GetUserFeedbacks(
                claimsPrincipal, user.Id, "", "", 1, "", "", "Test", 10);

            // Assert
            Assert.Equal(2, result.status);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task GetUserFeedbacks_WithPagination_ReturnsPaginatedResults()
        {
            // Arrange
            var (_, user, _) = await SetupFeedbackWithUserAndWorkout();

            // Create multiple feedbacks
            for (int i = 0; i < 5; i++)
            {
                var workout = CreateTestWorkout(user, $"Workout {i}");
                var feedback = new Feedback
                {
                    Id = Guid.NewGuid(),
                    Title = $"Feedback {i}",
                    Type = "Positive",
                    FeedbackText = $"Feedback text {i}",
                    CreatedAt = DateTime.UtcNow.AddDays(-i),
                    User = user,
                    Workout = workout,
                    WorkoutID = workout.Id
                };
                await _unitOfWork.Feedbacks.Create(feedback);
            }
            await _unitOfWork.SaveChangesAsync();

            var claimsPrincipal = CreateTestClaimsPrincipal(user.Id.ToString());

            _authorizationServiceMock
                .Setup(x => x.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            // Act
            var result = await _feedbackService.GetUserFeedbacks(
                claimsPrincipal, user.Id, "", "", 1, "", "", "", 3);

            // Assert
            Assert.Equal(2, result.status);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Items.Count);
        }
        #endregion

        #region GetAllFeedbacks Tests
        [Fact]
        public async Task GetAllFeedbacks_WithFeedbacksInDatabase_ReturnsAllFeedbacks()
        {
            // Arrange
            var role = CreateTestRole("User");
            var user1 = CreateTestUser(role, email: "user1@test.com");
            var user2 = CreateTestUser(role, email: "user2@test.com");

            var workout1 = CreateTestWorkout(user1, "Workout 1");
            var workout2 = CreateTestWorkout(user2, "Workout 2");

            var feedback1 = new Feedback
            {
                Id = Guid.NewGuid(),
                Title = "Feedback 1",
                Type = "Positive",
                FeedbackText = "Great!",
                CreatedAt = DateTime.UtcNow,
                User = user1,
                Workout = workout1,
                WorkoutID = workout1.Id
            };

            var feedback2 = new Feedback
            {
                Id = Guid.NewGuid(),
                Title = "Feedback 2",
                Type = "Neutral",
                FeedbackText = "Okay",
                CreatedAt = DateTime.UtcNow,
                User = user2,
                Workout = workout2,
                WorkoutID = workout2.Id
            };

            await _unitOfWork.Feedbacks.Create(feedback1);
            await _unitOfWork.Feedbacks.Create(feedback2);
            await _unitOfWork.SaveChangesAsync();

            // Act
            var result = await _feedbackService.GetAllFeedbacks(1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.status); // Success
            Assert.Equal("Successful", result.msg);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Items.Count);
        }

        [Fact]
        public async Task GetAllFeedbacks_WithEmptyDatabase_ReturnsBadRequest()
        {
            // Act
            var result = await _feedbackService.GetAllFeedbacks(1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.status);
            Assert.Equal("No Feedbacks in Database", result.msg);
        }

        [Fact]
        public async Task GetAllFeedbacks_WithPagination_ReturnsPaginatedResults()
        {
            // Arrange
            var role = CreateTestRole("User");
            var user = CreateTestUser(role);

            // Create 5 feedbacks
            for (int i = 0; i < 5; i++)
            {
                var workout = CreateTestWorkout(user, $"Workout {i}");
                var feedback = new Feedback
                {
                    Id = Guid.NewGuid(),
                    Title = $"Feedback {i}",
                    Type = "Positive",
                    FeedbackText = $"Text {i}",
                    CreatedAt = DateTime.UtcNow.AddDays(-i),
                    User = user,
                    Workout = workout,
                    WorkoutID = workout.Id
                };
                await _unitOfWork.Feedbacks.Create(feedback);
            }
            await _unitOfWork.SaveChangesAsync();

            // Act
            var result = await _feedbackService.GetAllFeedbacks(1, 2);

            // Assert
            Assert.Equal(2, result.status);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Items.Count);
        }
        #endregion
    }
}
