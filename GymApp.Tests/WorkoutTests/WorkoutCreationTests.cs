using Gym_App.Application.Authorization;
using Gym_App.Application.Services;
using Gym_App.Infastructure.DTOs.WorkoutDTOs;
using Gym_App.Infastructure.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Moq;
using System.Security.Claims;

namespace GymApp.Tests.WorkoutTests
{
    public class WorkoutCreationTests : TestBase
    {
        private readonly IWorkoutService _workoutService;
        private readonly Mock<ICachedAuthorizationService> _authorizationService;
        public WorkoutCreationTests() : base("WorkoutTestsDatabase")
        {
            _authorizationService = new Mock<ICachedAuthorizationService>();
            _workoutService = new WorkoutService(_unitOfWork, _authorizationService.Object);
        }
        //Creating workout with all its possibilities of failure
        [Fact]
        public async Task CreateWorkoutTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = new WorkoutCreationDTO
            {
                Name = "Test Workout",
                Description = "This is a test workout",
                UserID = user.Id,
                Date = DateTime.Now,
                Day = "Monday",
                Difficulty = "Medium"
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.CreateWorkout(workout);
            Assert.NotNull(result);
            Assert.Equal("Workout created successfully", result.msg);
            Assert.Equal(2, result.status);
        }
        [Fact]
        public async Task CreateWorkoutWithNonExistingUserTest()
        {
            var workout = new WorkoutCreationDTO
            {
                Name = "Test Workout",
                Description = "This is a test workout",
                UserID = Guid.NewGuid(), // Non-existing user ID
                Date = DateTime.Now,
                Day = "Monday",
                Difficulty = "Medium"
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.CreateWorkout(workout);
            Assert.NotNull(result);
            Assert.Equal("User not found", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task CreateWorkoutWithUnauthorizedUserTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = new WorkoutCreationDTO
            {
                Name = "Test Workout",
                Description = "This is a test workout",
                UserID = user.Id,
                Date = DateTime.Now,
                Day = "Monday",
                Difficulty = "Medium"
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(false);
            var result = await _workoutService.CreateWorkout(workout);
            Assert.NotNull(result);
            Assert.Equal("Forbidden from access", result.msg);
            Assert.Equal(1, result.status);
        }
        [Fact]
        public async Task CreateWorkoutWithInvalidDataTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = new WorkoutCreationDTO
            {
                Name = "", // Invalid name
                Description = "This is a test workout",
                UserID = user.Id,
                Date = DateTime.Now,
                Day = "Monday",
                Difficulty = "Medium"
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.CreateWorkout(workout);
            Assert.NotNull(result);
            Assert.Equal("Invalid workout data", result.msg);
            Assert.Equal(0, result.status);
        }
    }
}
