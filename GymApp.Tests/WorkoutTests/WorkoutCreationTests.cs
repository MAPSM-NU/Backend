using Gym_App.Application.Authorization;
using Gym_App.Application.Hubs;
using Gym_App.Application.Services;
using Gym_App.Domain;
using Gym_App.Infastructure.DTOs.WorkoutDTOs;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infrastructure.DTOs.Exercise;
using Gym_App.Infrastructure.DTOs.Workout;
using Gym_App.Infrastructure.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace GymApp.Tests.WorkoutTests
{
    public class WorkoutCreationTests : TestBase
    {
        private readonly IWorkoutService _workoutService;
        private readonly Mock<ICachedAuthorizationService> _authorizationService;
        private readonly Mock<IWorkoutNotificationSink> _notificationService;
        private readonly Mock<ILogger<WorkoutService>> _logger;
        private readonly Mock<IUserStatsService> _stats;
        public WorkoutCreationTests() : base("WorkoutTestsDatabase")
        {
            _authorizationService = new Mock<ICachedAuthorizationService>();
            _notificationService = new Mock<IWorkoutNotificationSink>();
            _logger = new Mock<ILogger<WorkoutService>>();
            _stats = new Mock<IUserStatsService>();
            _workoutService = new WorkoutService(_unitOfWork, _authorizationService.Object, _notificationService.Object, _stats.Object, _logger.Object);
        }
        //Creating workout with all its possibilities of failure
        [Fact]
        public async Task CreateWorkoutTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var exercise = CreateTestExercise();
            var workout = new WorkoutCreationDTO
            {
                Name = "Test Workout",
                Description = "This is a test workout",
                Date = DateTime.Now,
                Day = "Monday",
                Difficulty = "Medium",
                Type = "Strength",
                ExerciseDetails = new List<ExerciseWorkoutCreationDTO>
                {
                    new ExerciseWorkoutCreationDTO
                    {
                        ExerciseId = exercise.Id,
                        IsCompleted = false,
                        Name = exercise.Name,
                        Notes = "Test notes",
                        PlannedReps = 10,
                        PlannedWeight = 50.2022f,
                        Sets = new List<WorkoutSetDTO>
                        {
                            new WorkoutSetDTO
                            {
                                SetNumber = 1,
                                ActualReps = 10,
                                ActualWeight = 50.20f
                            },
                            new WorkoutSetDTO
                            {
                                SetNumber = 2,
                                ActualReps = 8,
                                ActualWeight = 60.60f
                            }
                        }
                    }
                }

            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.CreateWorkoutWithExercisesAsync(user.Id, workout);
            Assert.NotNull(result);
            Assert.Equal("Workout and exercises with sets created successfully", result.msg);
            Assert.Equal(2, result.status);
        }
        [Fact]
        public async Task CreateWorkoutWithNonExistingUserTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var exercise = CreateTestExercise();
            var workout = new WorkoutCreationDTO
            {
                Name = "Test Workout",
                Description = "This is a test workout",
                Date = DateTime.Now,
                Day = "Monday",
                Difficulty = "Medium",
                Type = "Strength"
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.CreateWorkoutWithExercisesAsync(Guid.NewGuid(), workout);//non-existing user ID
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
                Date = DateTime.Now,
                Day = "Monday",
                Difficulty = "Medium",
                Type = "Strength"
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(false);
            var result = await _workoutService.CreateWorkoutWithExercisesAsync(user.Id, workout);
            Assert.NotNull(result);
            Assert.Equal("Unauthorized", result.msg);
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
                Date = DateTime.Now,
                Day = "Monday",
                Difficulty = "Medium",
                Type = "Strength"
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.CreateWorkoutWithExercisesAsync(user.Id, workout);
            Assert.NotNull(result);
            Assert.Equal("Invalid workout data", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task CreateWorkoutWithNonExistingExerciseTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = new WorkoutCreationDTO
            {
                Name = "Test Workout",
                Description = "This is a test workout",
                Date = DateTime.Now,
                Day = "Monday",
                Difficulty = "Medium",
                Type = "Strength",
                ExerciseDetails = new List<ExerciseWorkoutCreationDTO>
                {
                    new ExerciseWorkoutCreationDTO
                    {
                        ExerciseId = Guid.NewGuid(), // Non-existing exercise ID
                        IsCompleted = false,
                        Name = "Test Exercise",
                        Notes = "Test notes",
                        PlannedReps = 10,
                        PlannedWeight = 50,
                        Sets = new List<WorkoutSetDTO>
                        {
                            new WorkoutSetDTO
                            {
                                SetNumber = 1,
                                ActualReps = 10,
                                ActualWeight = 50.90f
                            }
                        }
                    }
                }
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.CreateWorkoutWithExercisesAsync(user.Id, workout);
            Assert.NotNull(result);
            Assert.Equal("Workout created successfully, but no valid exercises were added", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task CreateWorkoutButNoSetsAdded()
        {
            var user = CreateTestUser(CreateTestRole());
            var exercise = CreateTestExercise();
            var workout = new WorkoutCreationDTO
            {
                Name = "Test Workout",
                Description = "This is a test workout",
                Date = DateTime.Now,
                Day = "Monday",
                Difficulty = "Medium",
                Type = "Strength",
                ExerciseDetails = new List<ExerciseWorkoutCreationDTO>
                {
                    new ExerciseWorkoutCreationDTO
                    {
                        ExerciseId = exercise.Id,
                        IsCompleted = false,
                        Name = exercise.Name,
                        Notes = "Test notes",
                        PlannedReps = 10,
                        PlannedWeight = 50,
                        Sets = new List<WorkoutSetDTO>() // No sets added
                    }
                }
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.CreateWorkoutWithExercisesAsync(user.Id, workout);
            Assert.NotNull(result);
            Assert.Equal("Workout and exercises created successfully, but no sets were added", result.msg);
            Assert.Equal(2, result.status);
        }
    }
}
