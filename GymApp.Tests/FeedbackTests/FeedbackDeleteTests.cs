using Gym_App.Application.Authorization;
using Gym_App.Application.Services;
using Gym_App.Infastructure.Interfaces.Services;
using Moq;

namespace GymApp.Tests.FeedbackTests
{
    public class FeedbackDeleteTests : TestBase
    {
        private readonly IFeedbackService _feedbackService;
        private readonly Mock<ICachedAuthorizationService> _authorizationService;

        public FeedbackDeleteTests() : base("FeedbackDeleteTestDatabase")
        {
            _authorizationService = new Mock<ICachedAuthorizationService>();
            _feedbackService = new FeedbackService(_unitOfWork, _authorizationService.Object);
        }

        [Fact]
        public async Task DeleteFeedbackSuccessTest()
        {
            // Arrange
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            var feedback = CreateTestFeedback(user, workout);
            await _unitOfWork.SaveChangesAsync();

            var feedbackId = feedback.Id;

            _authorizationService.Setup(x => x.IsUserAsync(user.Id)).ReturnsAsync(true);

            // Act
            var result = await _feedbackService.DeleteFeedback(feedbackId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.status);
            Assert.Equal("Success", result.msg);

            // Verify feedback was deleted from database
            var deletedFeedback = await _unitOfWork.Feedbacks.GetById(feedbackId);
            Assert.Null(deletedFeedback);

            // Verify only one feedback was deleted
            var feedbacksCount = _db.Feedbacks.Count();
            Assert.Equal(0, feedbacksCount);
        }

        [Fact]
        public async Task DeleteFeedbackWithEmptyIdTest()
        {
            // Arrange
            var emptyGuid = Guid.Empty;

            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            // Act
            var result = await _feedbackService.DeleteFeedback(emptyGuid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.status);
            Assert.Equal("Invalid feedback ID", result.msg);
        }

        [Fact]
        public async Task DeleteNonExistingFeedbackTest()
        {
            // Arrange
            var nonExistingFeedbackId = Guid.NewGuid();

            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            // Act
            var result = await _feedbackService.DeleteFeedback(nonExistingFeedbackId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.status);
            Assert.Equal("Feedback not found", result.msg);
        }

        [Fact]
        public async Task DeleteFeedbackWithUnauthorizedUserTest()
        {
            // Arrange
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            var feedback = CreateTestFeedback(user, workout);
            await _unitOfWork.SaveChangesAsync();

            _authorizationService.Setup(x => x.IsUserAsync(user.Id)).ReturnsAsync(false);

            // Act
            var result = await _feedbackService.DeleteFeedback(feedback.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.status);
            Assert.Equal("Unauthorized", result.msg);

            // Verify feedback was NOT deleted
            var deletedFeedback = await _unitOfWork.Feedbacks.GetById(feedback.Id);
            Assert.NotNull(deletedFeedback);
        }

        [Fact]
        public async Task DeleteFeedbackFromDifferentUserTest()
        {
            // Arrange
            var user1 = CreateTestUser(CreateTestRole());
            var user2 = CreateTestUser(CreateTestRole(), "user2@gmail.com");
            var workout = CreateTestWorkout(user1);
            var feedback = CreateTestFeedback(user1, workout);
            await _unitOfWork.SaveChangesAsync();

            // User2 tries to delete feedback from user1
            _authorizationService.Setup(x => x.IsUserAsync(user1.Id)).ReturnsAsync(false);

            // Act
            var result = await _feedbackService.DeleteFeedback(feedback.Id);

            // Assert
            Assert.Equal(1, result.status);
            Assert.Equal("Unauthorized", result.msg);

            // Verify feedback was NOT deleted
            var existingFeedback = await _unitOfWork.Feedbacks.GetById(feedback.Id);
            Assert.NotNull(existingFeedback);
        }

        [Fact]
        public async Task DeleteMultipleFeedbacksTest()
        {
            // Arrange
            var user = CreateTestUser(CreateTestRole());
            var workout1 = CreateTestWorkout(user, "Workout 1");
            var workout2 = CreateTestWorkout(user, "Workout 2");
            var feedback1 = CreateTestFeedback(user, workout1);
            var feedback2 = CreateTestFeedback(user, workout2);
            await _unitOfWork.SaveChangesAsync();

            var feedback1Id = feedback1.Id;
            var feedback2Id = feedback2.Id;

            _authorizationService.Setup(x => x.IsUserAsync(user.Id)).ReturnsAsync(true);

            // Act
            var result1 = await _feedbackService.DeleteFeedback(feedback1Id);
            await _unitOfWork.SaveChangesAsync();

            var result2 = await _feedbackService.DeleteFeedback(feedback2Id);

            // Assert
            Assert.Equal(2, result1.status);
            Assert.Equal(2, result2.status);

            var feedbacksCount = _db.Feedbacks.Count();
            Assert.Equal(0, feedbacksCount);
        }

        [Fact]
        public async Task DeleteFeedbackThenVerifyNotFoundTest()
        {
            // Arrange
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            var feedback = CreateTestFeedback(user, workout);
            await _unitOfWork.SaveChangesAsync();

            var feedbackId = feedback.Id;

            _authorizationService.Setup(x => x.IsUserAsync(user.Id)).ReturnsAsync(true);

            // Act
            var deleteResult = await _feedbackService.DeleteFeedback(feedbackId);
            await _unitOfWork.SaveChangesAsync();

            // Try to delete again
            var secondDeleteResult = await _feedbackService.DeleteFeedback(feedbackId);

            // Assert
            Assert.Equal(2, deleteResult.status);
            Assert.Equal(0, secondDeleteResult.status);
            Assert.Equal("Feedback not found", secondDeleteResult.msg);
        }

        [Fact]
        public async Task DeleteFeedbackPreservesOtherFeedbacksTest()
        {
            // Arrange
            var user = CreateTestUser(CreateTestRole());
            var workout1 = CreateTestWorkout(user, "Workout 1");
            var workout2 = CreateTestWorkout(user, "Workout 2");
            var feedback1 = CreateTestFeedback(user, workout1, "Feedback 1");
            var feedback2 = CreateTestFeedback(user, workout2, "Feedback 2");
            await _unitOfWork.SaveChangesAsync();

            var feedback1Id = feedback1.Id;
            var feedback2Id = feedback2.Id;

            _authorizationService.Setup(x => x.IsUserAsync(user.Id)).ReturnsAsync(true);

            // Act
            var deleteResult = await _feedbackService.DeleteFeedback(feedback1Id);
            await _unitOfWork.SaveChangesAsync();

            // Assert
            Assert.Equal(2, deleteResult.status);

            var remainingFeedback = await _unitOfWork.Feedbacks.GetById(feedback2Id);
            Assert.NotNull(remainingFeedback);
            Assert.Equal("Feedback 2", remainingFeedback.Title);
            Assert.Equal(feedback2Id, remainingFeedback.Id);

            var feedbacksCount = _db.Feedbacks.Count();
            Assert.Equal(1, feedbacksCount);
        }

        [Fact]
        public async Task DeleteFeedbackWithNullAuthorizationResponseTest()
        {
            // Arrange
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            var feedback = CreateTestFeedback(user, workout);
            await _unitOfWork.SaveChangesAsync();

            // Setup authorization to return false (unauthorized)
            _authorizationService.Setup(x => x.IsUserAsync(user.Id)).ReturnsAsync(false);

            // Act
            var result = await _feedbackService.DeleteFeedback(feedback.Id);

            // Assert
            Assert.Equal(1, result.status);
            Assert.Equal("Unauthorized", result.msg);
        }

        [Fact]
        public async Task DeleteFeedbackVerifyDatabaseIntegrityTest()
        {
            // Arrange
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            var feedback = CreateTestFeedback(user, workout);
            await _unitOfWork.SaveChangesAsync();

            var initialCount = _db.Feedbacks.Count();

            _authorizationService.Setup(x => x.IsUserAsync(user.Id)).ReturnsAsync(true);

            // Act
            var result = await _feedbackService.DeleteFeedback(feedback.Id);
            await _unitOfWork.SaveChangesAsync();

            var finalCount = _db.Feedbacks.Count();

            // Assert
            Assert.Equal(2, result.status);
            Assert.Equal(1, initialCount);
            Assert.Equal(0, finalCount);
        }
    }
}
