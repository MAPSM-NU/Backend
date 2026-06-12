using Gym_App.Application.Authorization;
using Gym_App.Application.Hubs;
using Gym_App.Application.Services;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infrastructure.DTOs.Exercise;
using Gym_App.Infrastructure.DTOs.Workout;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Moq;

namespace GymApp.Tests.UserStats
{
    public class UserStatsTests : TestBase
    {
        private readonly IWorkoutService _workoutService;
        private readonly Mock<ICachedAuthorizationService> _authorizationService;
        private readonly Mock<IWorkoutNotificationSink> _notificationService;
        private readonly Mock<ILogger<WorkoutService>> _logger;
        private readonly IServiceScopeFactory _serviceProvider;
        private readonly Mock<ILogger<StatsChecker>> _loggerFactory;
        public UserStatsTests() : base("UserStatsDatabase")
        {
            _authorizationService = new Mock<ICachedAuthorizationService>();
            _notificationService = new Mock<IWorkoutNotificationSink>();
            _logger = new Mock<ILogger<WorkoutService>>();
            _loggerFactory = new Mock<ILogger<StatsChecker>>();
            _serviceProvider = new ServiceCollection()
                .AddSingleton(_unitOfWork)
                .AddSingleton(_authorizationService.Object)
                .AddSingleton(_notificationService.Object)
                .AddSingleton(_logger.Object)
                .AddSingleton(_loggerFactory.Object)
                .BuildServiceProvider()
                .GetRequiredService<IServiceScopeFactory>();
            _workoutService = new WorkoutService(_unitOfWork, _authorizationService.Object, _notificationService.Object, _logger.Object);
        }
        [Fact]
        public async Task CheckMissedWorkoutTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            workout.ScheduledStartTime = DateTime.Today.AddDays(-1);
            workout.IsCompleted = false;
            await _unitOfWork.SaveChangesAsync();

            var statsChecker = new StatsChecker(_serviceProvider, _loggerFactory.Object);
            await statsChecker.CheckMissedWorkout();

            var stats = await _unitOfWork.UserStats.GetUserStatsByUserId(user.Id);
            Assert.NotNull(stats);
            Assert.Equal(200, stats.totalWorkoutsCompleted);
            Assert.False(workout.IsCompleted);
            Assert.Equal(0, stats.workoutStreak);
            Assert.Equal(1, stats.totalWorkoutsMissed);
        }
        [Fact]
        public async Task CheckMissedWorkoutNoMissedTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            workout.ScheduledStartTime = DateTime.Today.AddDays(1);
            workout.IsCompleted = true;
            await _unitOfWork.SaveChangesAsync();
            var statsChecker = new StatsChecker(_serviceProvider, _loggerFactory.Object);
            await statsChecker.CheckMissedWorkout();
            var stats = await _unitOfWork.UserStats.GetUserStatsByUserId(user.Id);
            Assert.NotNull(stats);
            Assert.Equal(0, stats.totalWorkoutsMissed);
        }
        [Fact]
        public async Task CheckMissedWorkoutMultipleWorkoutsTest()
        {
            var user = CreateTestUser(CreateTestRole());
            for (int i = 0; i < 3; i++)
            {
                var workout = CreateTestWorkout(user);
                workout.ScheduledStartTime = DateTime.Today.AddDays(-1);
                workout.IsCompleted = false;
            }
            await _unitOfWork.SaveChangesAsync();
            var statsChecker = new StatsChecker(_serviceProvider, _loggerFactory.Object);
            await statsChecker.CheckMissedWorkout();
            var stats = await _unitOfWork.UserStats.GetUserStatsByUserId(user.Id);
            Assert.NotNull(stats);
            Assert.Equal(3, stats.totalWorkoutsMissed);
        }

        // Workouts finished tests

        [Fact]
        public async Task WorkoutFinisheFailed()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();

            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.StartWorkoutAsync(workout.Id, user.Id);
            Assert.Equal("Workout started", result.msg);
            Assert.Equal(2, result.status);

            result = await _workoutService.CompleteWorkoutAsync(workout.Id, user.Id);
            Assert.Equal("Either you didn't finish atleast 50% of the exercises in the workout or there were less than 3", result.msg);
            Assert.Equal(0, result.status);

        }
        [Fact]
        public async Task WorkoutFinishedFailedNotAuthorized()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            workout.ScheduledStartTime = DateTime.Now.AddMinutes(-30);
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(false);
            var result = await _workoutService.StartWorkoutAsync(workout.Id, user.Id);
            Assert.Equal("Unauthorized", result.msg);
            Assert.Equal(1, result.status);
            result = await _workoutService.CompleteWorkoutAsync(workout.Id, user.Id);
            Assert.Equal("Unauthorized", result.msg);
            Assert.Equal(1, result.status);
        }
        public async Task WorkoutFinishedExercisesNotEnough()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            workout.ScheduledStartTime = DateTime.Now.AddMinutes(-30);
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            var result = await _workoutService.StartWorkoutAsync(workout.Id, user.Id);
            Assert.Equal("Workout started", result.msg);
            Assert.Equal(2, result.status);

            var progressDto = new WorkoutUpdateProgressDTO
            {
                WorkoutId = workout.Id,
                Exercises = new List<ExerciseUpdateProgressDTO>
                {
                    new ExerciseUpdateProgressDTO
                    {
                        ExerciseId = workout.ExerciseInstances!.First().Id,
                        IsCompleted = true,
                        CompletedAt = DateTime.Now.AddMinutes(-10)
                    }
                }
            };

            result = await _workoutService.UpdateWorkoutProgressAsync(user.Id, progressDto);
            Assert.Equal("Progress updated successfully", result.msg);
            Assert.Equal(2, result.status);

            result = await _workoutService.CompleteWorkoutAsync(workout.Id, user.Id);
            Assert.Equal("Either you didn't finish atleast 50% of the exercises in the workout or there were less than 3", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task WorkoutFinishedSuccess()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            workout.ScheduledStartTime = DateTime.Now.AddMinutes(-30);
            workout.ActualStartTime = DateTime.Now.AddMinutes(-30);
            workout.hasStarted = true;
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            //var result = await _workoutService.StartWorkoutAsync(workout.Id, user.Id);
            //Assert.Equal("Workout started", result.msg);
            //Assert.Equal(2, result.status);

            var progressDto = new WorkoutUpdateProgressDTO
            {
                WorkoutId = workout.Id,
                Exercises = new List<ExerciseUpdateProgressDTO>
                {
                    new ExerciseUpdateProgressDTO
                    {
                        ExerciseId = workout.ExerciseInstances!.First().Id,
                        IsCompleted = true,
                        CompletedAt = DateTime.Now.AddMinutes(-10)
                    },
                    new ExerciseUpdateProgressDTO
                    {
                        ExerciseId = workout.ExerciseInstances!.Last().Id,
                        IsCompleted = true,
                        CompletedAt = DateTime.Now.AddMinutes(-5)
                    }
                }
            };

            var result = await _workoutService.UpdateWorkoutProgressAsync(user.Id, progressDto);
            Assert.Equal("Progress updated successfully", result.msg);
            Assert.Equal(2, result.status);

            result = await _workoutService.CompleteWorkoutAsync(workout.Id, user.Id);
            Assert.Equal(2, result.status);

            var stats = await _unitOfWork.UserStats.GetUserStatsByUserId(user.Id);
            Assert.NotNull(stats);
            Assert.Equal(1, stats.totalWorkoutsCompleted);
            Assert.Equal(1, stats.workoutStreak);
            Assert.Equal(1, stats.longestStreak);
            Assert.Equal(100, stats.workoutCompletionRate);
            Assert.Equal(2, stats.totalExercisesCompleted);
            //Assert.Equal(0.5, stats.totalHours); will be done later
        }
    }
}
