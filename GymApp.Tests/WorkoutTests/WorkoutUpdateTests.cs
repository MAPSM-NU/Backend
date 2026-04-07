using Gym_App.Application.Services;
using Gym_App.Domain;
using Gym_App.Infastructure.DTOs.WorkoutDTOs;
using Gym_App.Infastructure.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Moq;
using System.Security.Claims;

namespace GymApp.Tests.WorkoutTests
{
    public class WorkoutUpdateTests : TestBase
    {
        private readonly IWorkoutService _workoutService;
        private readonly Mock<IAuthorizationService> _authorizationService;
        public WorkoutUpdateTests() : base("WorkoutTestsDatabase")
        {
            _authorizationService = new Mock<IAuthorizationService>();
            _workoutService = new WorkoutService(_unitOfWork, _authorizationService.Object);
        }
        //Updating workout with all its possibilities of failure
        [Fact]
        public async Task UpdateWorkoutTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workoutId = Guid.NewGuid();
            var workout = CreateTestWorkout(user, "Test Workout", "This is a test workout", workoutId);
            await _unitOfWork.SaveChangesAsync();
            var workoutUpdate = new WorkoutUpdateDTO
            {
                Name = "Updated Workout",
                Description = "This is an updated test workout",
                Date = DateTime.Now.AddDays(1),
                Day = "Tuesday",
                Difficulty = "Hard",
            };
            _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());
            var result = await _workoutService.UpdateWorkout(new ClaimsPrincipal(), workout.Id, workoutUpdate);
            Assert.NotNull(result);
            Assert.Equal("Workout updated successfully", result.msg);
            Assert.Equal(2, result.status);
        }
        [Fact]
        public async Task UpdateNonExistingWorkoutTest()
        {
            var workoutUpdate = new WorkoutUpdateDTO
            {
                Name = "Updated Workout",
                Description = "This is an updated test workout",
                Date = DateTime.Now.AddDays(1),
                Day = "Tuesday",
                Difficulty = "Hard",
            };
            _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());
            var result = await _workoutService.UpdateWorkout(new ClaimsPrincipal(), Guid.NewGuid(), workoutUpdate);
            Assert.NotNull(result);
            Assert.Equal("Workout not found", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task UpdateWorkoutWithUnauthorizedUserTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var workoutUpdate = new WorkoutUpdateDTO
            {
                Name = "Updated Workout",
                Description = "This is an updated test workout",
                Date = DateTime.Now.AddDays(1),
                Day = "Tuesday",
                Difficulty = "Hard",
            };
            _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Failed());
            var result = await _workoutService.UpdateWorkout(new ClaimsPrincipal(), workout.Id, workoutUpdate);
            Assert.NotNull(result);
            Assert.Equal("Forbidden from access", result.msg);
            Assert.Equal(1, result.status);
        }
        [Fact]
        public async Task UpdateWorkoutWithInvalidDataTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var workoutUpdate = new WorkoutUpdateDTO
            {
                Name = "test name",
                Description = "This is an updated test workout",
                Date = DateTime.Now.AddDays(1),
                Day = "Tuesday",
                Difficulty = "Hard",
            };
            _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());
            var result = await _workoutService.UpdateWorkout(new ClaimsPrincipal(), Guid.Empty, workoutUpdate);//Passing an empty id to check If it will pass validation checks
            Assert.NotNull(result);
            Assert.Equal("Invalid workout data", result.msg);
            Assert.Equal(0, result.status);
        }
    }
}
