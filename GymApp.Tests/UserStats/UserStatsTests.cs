using Gym_App.Application.Authorization;
using Gym_App.Application.Hubs;
using Gym_App.Application.Services;
using Gym_App.Domain;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infrastructure.DTOs.Exercise;
using Gym_App.Infrastructure.DTOs.Workout;
using Gym_App.Infrastructure.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Globalization;
using User = Gym_App.Domain.User;

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
        private readonly Mock<ILogger<UserStatsService>> _loggerStats;
        private readonly IUserStatsService _userStatsService;
        public UserStatsTests() : base("UserStatsDatabase")
        {
            _authorizationService = new Mock<ICachedAuthorizationService>();
            _notificationService = new Mock<IWorkoutNotificationSink>();
            _logger = new Mock<ILogger<WorkoutService>>();
            _loggerFactory = new Mock<ILogger<StatsChecker>>();
            _loggerStats = new Mock<ILogger<UserStatsService>>();
            _userStatsService = new UserStatsService(_unitOfWork, _authorizationService.Object, _loggerStats.Object);
            _serviceProvider = new ServiceCollection()
                .AddSingleton(_unitOfWork)
                .AddSingleton(_authorizationService.Object)
                .AddSingleton(_notificationService.Object)
                .AddSingleton(_logger.Object)
                .AddSingleton(_loggerFactory.Object)
                .BuildServiceProvider()
                .GetRequiredService<IServiceScopeFactory>();
            _workoutService = new WorkoutService(_unitOfWork, _authorizationService.Object, _notificationService.Object, _userStatsService, _logger.Object);
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
                        ExerciseInstanceId = workout.ExerciseInstances!.First().Id,
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
                        ExerciseInstanceId = workout.ExerciseInstances!.First().Id,
                        IsCompleted = true,
                        CompletedAt = DateTime.Now.AddMinutes(-10)
                    },
                    new ExerciseUpdateProgressDTO
                    {
                        ExerciseInstanceId = workout.ExerciseInstances!.Last().Id,
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
        [Fact]
        public async Task FinishWorkoutWithStats()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            workout.ScheduledStartTime = DateTime.Now.AddMinutes(-30);
            workout.ActualStartTime = DateTime.Now.AddMinutes(-30);
            workout.hasStarted = true;
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            var progressDto = new WorkoutUpdateProgressDTO
            {
                WorkoutId = workout.Id,
                Exercises = new List<ExerciseUpdateProgressDTO>
                {
                    new ExerciseUpdateProgressDTO
                    {
                        ExerciseInstanceId = workout.ExerciseInstances!.First().Id,
                        IsCompleted = true,
                        CompletedAt = DateTime.Now.AddMinutes(-10)
                    },
                    new ExerciseUpdateProgressDTO
                    {
                        ExerciseInstanceId = workout.ExerciseInstances!.Last().Id,
                        IsCompleted = true,
                        CompletedAt = DateTime.Now.AddMinutes(-5)
                    }
                }
            };

            var result = await _workoutService.UpdateWorkoutProgressAsync(user.Id, progressDto);
            Assert.Equal(2, result.status);

            result = await _workoutService.CompleteWorkoutAsync(workout.Id, user.Id);
            Assert.Equal("Workout completed", result.msg);
            Assert.Equal(2, result.status);

            var statDaily = await _unitOfWork.UserStatDaily.GetUserStatsDaily(user.Id, DateOnly.FromDateTime(DateTime.Today));
            Assert.NotNull(statDaily);
            Assert.Equal(30, statDaily.totalReps);
            Assert.Equal(3, statDaily.totalExercisesCompleted);
            Assert.Equal(DateTime.Now.DayOfWeek.ToString(), statDaily.dayOfWeek);

            var statWeekly = await _unitOfWork.UserStatWeekly.GetUserStatsWeekly(user.Id, ISOWeek.GetWeekOfYear(DateTime.Now), DateTime.Now.Year);
            Assert.NotNull(statWeekly);
            Assert.Equal(30, statWeekly.totalRepsCompleted);
            Assert.Equal(3, statWeekly.totalExercisesCompleted);
            Assert.Equal(1, statWeekly.activeDays);
            Assert.Equal(3, statWeekly.totalSetsCompleted);
            Assert.Equal(36, statWeekly.KcaloriesBurned);
            Assert.Equal(1, statWeekly.totalWorkoutsCompleted);
            Assert.Equal(100, statWeekly.workoutCompletionRate);

            var statMonthly = await _unitOfWork.UserStatMonthly.GetUserStatsMonthly(user.Id, DateTime.Now.Month, DateTime.Now.Year);
            Assert.NotNull(statMonthly);
            Assert.Equal(30, statMonthly.totalRepsCompleted);
            Assert.Equal(3, statMonthly.totalExercisesCompleted);
            Assert.Equal(1, statMonthly.activeDays);
            Assert.Equal(3, statMonthly.totalSetsCompleted);
            Assert.Equal(36, statMonthly.KcaloriesBurned);
            Assert.Equal(1, statMonthly.totalWorkoutsCompleted);
            Assert.Equal(100, statMonthly.workoutCompletionRate);
        }
        [Fact]
        public async Task FinishMultipleWorkoutsWithStats()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            var progressDto = await createProgressDto(user, workout, DateTime.Now.AddMinutes(-30));

            var result = await _workoutService.UpdateWorkoutProgressAsync(user.Id, progressDto);
            Assert.Equal(2, result.status);

            result = await _workoutService.CompleteWorkoutAsync(workout.Id, user.Id);
            Assert.Equal("Workout completed", result.msg);
            Assert.Equal(2, result.status);

            var workout2 = CreateTestWorkout(user);
            progressDto = await createProgressDto(user, workout2, DateTime.Now.AddMinutes(-30));

            result = await _workoutService.UpdateWorkoutProgressAsync(user.Id, progressDto);
            Assert.Equal(2, result.status);

            result = await _workoutService.CompleteWorkoutAsync(workout2.Id, user.Id);
            Assert.Equal("Workout completed", result.msg);
            Assert.Equal(2, result.status);

            var statDaily = await _unitOfWork.UserStatDaily.GetUserStatsDaily(user.Id, DateOnly.FromDateTime(DateTime.Today));
            Assert.NotNull(statDaily);
            Assert.Equal(30*2, statDaily.totalReps);
            Assert.Equal(3*2, statDaily.totalExercisesCompleted);
            Assert.Equal(DateTime.Now.DayOfWeek.ToString(), statDaily.dayOfWeek);

            var statWeekly = await _unitOfWork.UserStatWeekly.GetUserStatsWeekly(user.Id, ISOWeek.GetWeekOfYear(DateTime.Now), DateTime.Now.Year);
            Assert.NotNull(statWeekly);
            Assert.Equal(1, statWeekly.userStatsDaily.Count);
            //Assert.Equal(30*2, statWeekly.totalRepsCompleted);
            Assert.Equal(3*2, statWeekly.totalExercisesCompleted);
            Assert.Equal(1, statWeekly.activeDays);
            Assert.Equal(3*2, statWeekly.totalSetsCompleted);
            Assert.Equal(36*2, statWeekly.KcaloriesBurned);
            Assert.Equal(1*2, statWeekly.totalWorkoutsCompleted);
            Assert.Equal(100, statWeekly.workoutCompletionRate);

            var statMonthly = await _unitOfWork.UserStatMonthly.GetUserStatsMonthly(user.Id, DateTime.Now.Month, DateTime.Now.Year);
            Assert.NotNull(statMonthly);
            Assert.Equal(30*2, statMonthly.totalRepsCompleted);
            Assert.Equal(3*2, statMonthly.totalExercisesCompleted);
            Assert.Equal(1, statMonthly.activeDays);
            Assert.Equal(3*2, statMonthly.totalSetsCompleted);
            Assert.Equal(36 * 2, statMonthly.KcaloriesBurned);
            Assert.Equal(1*2, statMonthly.totalWorkoutsCompleted);
            Assert.Equal(100, statMonthly.workoutCompletionRate);
        }
        //[Fact]
        //public async Task FinishMultipleWorkoutsWithStatsAndDifferentDays()
        //{
        //    var user = CreateTestUser(CreateTestRole());
        //    var workout = CreateTestWorkout(user);
        //    var progressDto = await createProgressDto(user, workout, DateTime.Now.AddMinutes(-30));

        //    var result = await _workoutService.UpdateWorkoutProgressAsync(user.Id, progressDto);
        //    Assert.Equal("Progress updated successfully", result.msg);
        //    Assert.Equal(2, result.status);

        //    result = await _workoutService.CompleteWorkoutAsync(workout.Id, user.Id);
        //    Assert.Equal("Workout completed", result.msg);
        //    Assert.Equal(2, result.status);

        //    var workout2 = CreateTestWorkout(user);
        //    progressDto = await createProgressDto(user, workout2, DateTime.Now.AddMinutes(-30));

        //    result = await _workoutService.UpdateWorkoutProgressAsync(user.Id, progressDto);
        //    Assert.Equal("Progress updated successfully", result.msg);
        //    Assert.Equal(2, result.status);

        //    result = await _workoutService.CompleteWorkoutAsync(workout.Id, user.Id);
        //    Assert.Equal("Workout completed", result.msg);
        //    Assert.Equal(2, result.status);

        //    var yesterdayDailySet = CreateTestDailyStat(user, DateOnly.FromDateTime(DateTime.Now.AddDays(-2)));// Have to manually add third set since the sevice add the current day itself
        //    await _unitOfWork.SaveChangesAsync();
        //    result = await _userStatsService.AddWeeklyStats(user);
        //    Assert.Equal(2, result.status);

        //    result = await _userStatsService.AddMonthlyStats(user);
        //    Assert.Equal(2, result.status);

        //    result = await _workoutService.CompleteWorkoutAsync(workout.Id, user.Id);
        //    Assert.Equal("Workout completed", result.msg);
        //    Assert.Equal(2, result.status);

        //    var statDaily = await _unitOfWork.UserStatDaily.GetUserStatsDaily(user.Id, DateOnly.FromDateTime(DateTime.Today));
        //    Assert.NotNull(statDaily);
        //    Assert.Equal(30 * 2, statDaily.totalReps);
        //    Assert.Equal(3 * 2, statDaily.totalExercisesCompleted);
        //    Assert.Equal(DateTime.Now.DayOfWeek.ToString(), statDaily.dayOfWeek);

        //    var statWeekly = await _unitOfWork.UserStatWeekly.GetUserStatsWeekly(user.Id, ISOWeek.GetWeekOfYear(DateTime.Now), DateTime.Now.Year);
        //    Assert.NotNull(statWeekly);
        //    Assert.Equal(2, statWeekly.userStatsDaily.Count);
        //    //Assert.Equal(30*2, statWeekly.totalRepsCompleted);
        //    Assert.Equal(3 * 3, statWeekly.totalExercisesCompleted);
        //    Assert.Equal(2, statWeekly.activeDays);
        //    Assert.Equal(3 * 3, statWeekly.totalSetsCompleted);
        //    Assert.Equal(36 * 3, statWeekly.KcaloriesBurned);
        //    Assert.Equal(1 * 3, statWeekly.totalWorkoutsCompleted);
        //    Assert.Equal(100, statWeekly.workoutCompletionRate);

        //    var statMonthly = await _unitOfWork.UserStatMonthly.GetUserStatsMonthly(user.Id, DateTime.Now.Month, DateTime.Now.Year);
        //    Assert.NotNull(statMonthly);
        //    Assert.Equal(30 * 3, statMonthly.totalRepsCompleted);
        //    Assert.Equal(3 * 3, statMonthly.totalExercisesCompleted);
        //    Assert.Equal(2, statMonthly.activeDays);
        //    Assert.Equal(3 * 3, statMonthly.totalSetsCompleted);
        //    Assert.Equal(36 * 3, statMonthly.KcaloriesBurned);
        //    Assert.Equal(1 * 3, statMonthly.totalWorkoutsCompleted);
        //    Assert.Equal(100, statMonthly.workoutCompletionRate);
        //}
        private async Task<WorkoutUpdateProgressDTO> createProgressDto(User user,Workout workout,DateTime startDate)
        {
            
            workout.ScheduledStartTime = startDate;
            workout.ActualStartTime = startDate;
            workout.hasStarted = true;
            await _unitOfWork.Workouts.Create(workout);
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            var progressDto = new WorkoutUpdateProgressDTO
            {
                WorkoutId = workout.Id,
                Exercises = new List<ExerciseUpdateProgressDTO>
                {
                    new ExerciseUpdateProgressDTO
                    {
                        ExerciseInstanceId = workout.ExerciseInstances!.First().Id,
                        IsCompleted = true,
                        CompletedAt = DateTime.Now.AddMinutes(-10)
                    },
                    new ExerciseUpdateProgressDTO
                    {
                        ExerciseInstanceId = workout.ExerciseInstances!.Last().Id,
                        IsCompleted = true,
                        CompletedAt = DateTime.Now.AddMinutes(-5)
                    },
                    new ExerciseUpdateProgressDTO
                    {
                        ExerciseInstanceId = workout.ExerciseInstances.ElementAt(2).Id,
                        IsCompleted = true,
                        CompletedAt = DateTime.Now.AddMinutes(-5),
                    },
                }
            };
            return progressDto;
        }
        
    }
}
