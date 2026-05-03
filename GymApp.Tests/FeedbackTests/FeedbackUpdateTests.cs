using Gym_App.Application.Authorization;
using Gym_App.Application.Services;
using Gym_App.Infastructure.DTOs.Feedback;
using Gym_App.Infastructure.Interfaces.Services;
using Moq;

namespace GymApp.Tests.FeedbackTests
{
    public class FeedbackUpdateTests : TestBase
    {
        private readonly IFeedbackService _feedbackService;
        private readonly Mock<ICachedAuthorizationService> _authorizationService;

        public FeedbackUpdateTests() : base("FeedbackUpdateTestDatabase")
        {
            _authorizationService = new Mock<ICachedAuthorizationService>();
            _feedbackService = new FeedbackService(_unitOfWork, _authorizationService.Object);
        }

        [Fact]
        public async Task UpdateFeedbackSuccessTest()
        {
            // Arrange
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            var feedback = CreateTestFeedback(user, workout);
            await _unitOfWork.SaveChangesAsync();

            var updateDTO = new FeedbackUpdateDTO
            {
                Title = "Updated Workout",
                Type = "Advanced",
                FeedbackText = "Updated feedback text",
                CaloriesBurned = 350,
                DurationMinutes = 60
            };

            _authorizationService.Setup(x => x.IsUserAsync(user.Id)).ReturnsAsync(true);

            // Act
            var result = await _feedbackService.UpdateFeedback(feedback.Id, updateDTO);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.status);
            Assert.Equal("Success", result.msg);

            // Verify feedback was updated in database
            var updatedFeedback = await _unitOfWork.Feedbacks.GetById(feedback.Id);
            Assert.Equal("Updated Workout", updatedFeedback.Title);
            Assert.Equal("Advanced", updatedFeedback.Type);
            Assert.Equal("Updated feedback text", updatedFeedback.FeedbackText);
            Assert.Equal(350, updatedFeedback.CaloriesBurned);
            Assert.Equal(60, updatedFeedback.DurationMinutes);
        }

        [Fact]
        public async Task UpdateFeedbackWithNullDTOTest()
        {
            // Arrange
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            var feedback = CreateTestFeedback(user, workout);
            await _unitOfWork.SaveChangesAsync();

            _authorizationService.Setup(x => x.IsUserAsync(user.Id)).ReturnsAsync(true);

            // Act
            var result = await _feedbackService.UpdateFeedback(feedback.Id, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.status);
            Assert.Equal("Invalid DTO", result.msg);
        }

        [Fact]
        public async Task UpdateNonExistingFeedbackTest()
        {
            // Arrange
            var nonExistingFeedbackId = Guid.NewGuid();

            var updateDTO = new FeedbackUpdateDTO
            {
                Title = "Updated Workout",
                Type = "Advanced",
                FeedbackText = "Updated feedback text",
                CaloriesBurned = 350,
                DurationMinutes = 60
            };

            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            // Act
            var result = await _feedbackService.UpdateFeedback(nonExistingFeedbackId, updateDTO);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.status);
            Assert.Equal("Feedback not found", result.msg);
        }

        [Fact]
        public async Task UpdateFeedbackWithUnauthorizedUserTest()
        {
            // Arrange
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            var feedback = CreateTestFeedback(user, workout);
            await _unitOfWork.SaveChangesAsync();

            var updateDTO = new FeedbackUpdateDTO
            {
                Title = "Updated Workout",
                Type = "Advanced",
                FeedbackText = "Updated feedback text",
                CaloriesBurned = 350,
                DurationMinutes = 60
            };

            _authorizationService.Setup(x => x.IsUserAsync(user.Id)).ReturnsAsync(false);

            // Act
            var result = await _feedbackService.UpdateFeedback(feedback.Id, updateDTO);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.status);
            Assert.Equal("Unauthorized", result.msg);
        }

        [Fact]
        public async Task UpdateFeedbackWithEmptyTitleTest()
        {
            // Arrange
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            var feedback = CreateTestFeedback(user, workout, "Original Title");
            await _unitOfWork.SaveChangesAsync();

            var updateDTO = new FeedbackUpdateDTO
            {
                Title = "", // Empty title - the service updates it
                Type = "Advanced",
                FeedbackText = "Updated feedback text",
                CaloriesBurned = 350,
                DurationMinutes = 60
            };

            _authorizationService.Setup(x => x.IsUserAsync(user.Id)).ReturnsAsync(true);

            // Act
            var result = await _feedbackService.UpdateFeedback(feedback.Id, updateDTO);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.status);

            // Service updates title to empty string if provided
            var updatedFeedback = await _unitOfWork.Feedbacks.GetById(feedback.Id);
            Assert.Equal("", updatedFeedback.Title);
        }

        [Fact]
        public async Task UpdateFeedbackWithZeroCaloriesTest()
        {
            // Arrange
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            var feedback = CreateTestFeedback(user, workout, caloriesBurned: 250);
            await _unitOfWork.SaveChangesAsync();

            var updateDTO = new FeedbackUpdateDTO
            {
                Title = "Updated Workout",
                Type = "Advanced",
                FeedbackText = "Updated feedback text",
                CaloriesBurned = 0, // Zero calories should not update
                DurationMinutes = 60
            };

            _authorizationService.Setup(x => x.IsUserAsync(user.Id)).ReturnsAsync(true);

            // Act
            var result = await _feedbackService.UpdateFeedback(feedback.Id, updateDTO);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.status);

            // CaloriesBurned should not be updated if it's 0
            var updatedFeedback = await _unitOfWork.Feedbacks.GetById(feedback.Id);
            Assert.Equal(250, updatedFeedback.CaloriesBurned);
        }

        [Fact]
        public async Task UpdateFeedbackWithZeroDurationTest()
        {
            // Arrange
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            var feedback = CreateTestFeedback(user, workout, durationMinutes: 45);
            await _unitOfWork.SaveChangesAsync();

            var updateDTO = new FeedbackUpdateDTO
            {
                Title = "Updated Workout",
                Type = "Advanced",
                FeedbackText = "Updated feedback text",
                CaloriesBurned = 350,
                DurationMinutes = 0 // Zero duration should not update
            };

            _authorizationService.Setup(x => x.IsUserAsync(user.Id)).ReturnsAsync(true);

            // Act
            var result = await _feedbackService.UpdateFeedback(feedback.Id, updateDTO);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.status);

            // DurationMinutes should not be updated if it's 0
            var updatedFeedback = await _unitOfWork.Feedbacks.GetById(feedback.Id);
            Assert.Equal(45, updatedFeedback.DurationMinutes);
        }

        [Fact]
        public async Task UpdateFeedbackPartialUpdateTest()
        {
            // Arrange
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            var feedback = CreateTestFeedback(user, workout, "Original Title", "Original Type", "Original Text", 100, 30);
            await _unitOfWork.SaveChangesAsync();

            var updateDTO = new FeedbackUpdateDTO
            {
                Title = "Updated Title",
                Type = "Original Type", // Not changing
                FeedbackText = "Updated Text",
                CaloriesBurned = 250, // Changing
                DurationMinutes = 0 // Zero so won't update
            };

            _authorizationService.Setup(x => x.IsUserAsync(user.Id)).ReturnsAsync(true);

            // Act
            var result = await _feedbackService.UpdateFeedback(feedback.Id, updateDTO);

            // Assert
            Assert.Equal(2, result.status);

            var updatedFeedback = await _unitOfWork.Feedbacks.GetById(feedback.Id);
            Assert.Equal("Updated Title", updatedFeedback.Title);
            Assert.Equal("Original Type", updatedFeedback.Type);
            Assert.Equal("Updated Text", updatedFeedback.FeedbackText);
            Assert.Equal(250, updatedFeedback.CaloriesBurned);
            Assert.Equal(30, updatedFeedback.DurationMinutes); // Should remain unchanged
        }

        [Fact]
        public async Task UpdateFeedbackMultipleTimesTest()
        {
            // Arrange
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            var feedback = CreateTestFeedback(user, workout);
            await _unitOfWork.SaveChangesAsync();

            _authorizationService.Setup(x => x.IsUserAsync(user.Id)).ReturnsAsync(true);

            var updateDTO1 = new FeedbackUpdateDTO
            {
                Title = "First Update",
                Type = "Type1",
                FeedbackText = "First update text",
                CaloriesBurned = 100,
                DurationMinutes = 10
            };

            var updateDTO2 = new FeedbackUpdateDTO
            {
                Title = "Second Update",
                Type = "Type2",
                FeedbackText = "Second update text",
                CaloriesBurned = 200,
                DurationMinutes = 20
            };

            // Act
            var result1 = await _feedbackService.UpdateFeedback(feedback.Id, updateDTO1);
            await _unitOfWork.SaveChangesAsync();

            var result2 = await _feedbackService.UpdateFeedback(feedback.Id, updateDTO2);

            // Assert
            Assert.Equal(2, result1.status);
            Assert.Equal(2, result2.status);

            var finalFeedback = await _unitOfWork.Feedbacks.GetById(feedback.Id);
            Assert.Equal("Second Update", finalFeedback.Title);
            Assert.Equal("Type2", finalFeedback.Type);
            Assert.Equal("Second update text", finalFeedback.FeedbackText);
            Assert.Equal(200, finalFeedback.CaloriesBurned);
            Assert.Equal(20, finalFeedback.DurationMinutes);
        }

        [Fact]
        public async Task UpdateFeedbackWithLargeDataTest()
        {
            // Arrange
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            var feedback = CreateTestFeedback(user, workout);
            await _unitOfWork.SaveChangesAsync();

            var longFeedbackText = new string('x', 2000);

            var updateDTO = new FeedbackUpdateDTO
            {
                Title = new string('a', 100),
                Type = new string('b', 20),
                FeedbackText = longFeedbackText,
                CaloriesBurned = 9999,
                DurationMinutes = 999
            };

            _authorizationService.Setup(x => x.IsUserAsync(user.Id)).ReturnsAsync(true);

            // Act
            var result = await _feedbackService.UpdateFeedback(feedback.Id, updateDTO);

            // Assert
            Assert.Equal(2, result.status);

            var updatedFeedback = await _unitOfWork.Feedbacks.GetById(feedback.Id);
            Assert.Equal(100, updatedFeedback.Title.Length);
            Assert.Equal(2000, updatedFeedback.FeedbackText.Length);
            Assert.Equal(9999, updatedFeedback.CaloriesBurned);
            Assert.Equal(999, updatedFeedback.DurationMinutes);
        }
    }
}
