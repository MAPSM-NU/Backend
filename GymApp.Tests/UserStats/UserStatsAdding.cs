using Gym_App.Application.Authorization;
using Gym_App.Application.Services;
using Gym_App.Core;
using Gym_App.Domain;
using Gym_App.Infrastructure.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace GymApp.Tests.UserStats
{
    public class UserStatsAdding : TestBase
    {
        private readonly Mock<ICachedAuthorizationService> _authorizationService;
        private readonly Mock<ILogger<UserStatsService>> _logger;
        private readonly IUserStatsService _userStatsService;
        public UserStatsAdding(string databaseName) : base("UserStatsDatabase")
        {
            _authorizationService = new Mock<ICachedAuthorizationService>();
            _logger = new Mock<ILogger<UserStatsService>>();
            _userStatsService = new UserStatsService(_unitOfWork, _authorizationService.Object,_logger.Object);
        }
        //Daily adding
        [Fact]
        public async Task AddUserStatDaily()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            foreach (var exercise in workout.ExerciseInstances) 
            {
                exercise.IsCompleted = true;
                foreach(var set in exercise.Sets)
                {
                    set.ActualReps = 12;
                    set.ActualWeight = 12;
                }
            }
            await _unitOfWork.SaveChangesAsync();
            var result = await _userStatsService.AddDailyStats(workout);
            Assert.Equal("Daily Stat done", result.msg);
            Assert.Equal(2, result.status);

            var stat = await _unitOfWork.UserStatDaily.GetUserStatsDaily(user.Id, DateOnly.FromDateTime(DateTime.Today));
            Assert.NotNull(stat);
            Assert.Equal(12, stat.totalReps);
            Assert.Equal(3, stat.totalExercisesCompleted);
            Assert.Equal(DateTime.Now.DayOfWeek.ToString(), stat.dayOfWeek);
        }
        [Fact]
        public async Task AddUserStatDailyButNoUser()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            foreach (var exercise in workout.ExerciseInstances)
            {
                exercise.IsCompleted = true;
                foreach (var set in exercise.Sets)
                {
                    set.ActualReps = 12;
                    set.ActualWeight = 12;
                }
            }
            await _unitOfWork.SaveChangesAsync();
            workout.User = null;
            var result = await _userStatsService.AddDailyStats(workout);
            Assert.Equal("No User Found", result.msg);
            Assert.Equal(0, result.status);
        }
        //Weekly Adding
        [Fact]
        public async Task AddUserStatWeekly()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout1 = CreateTestWorkout(user);
            var workout2 = CreateTestWorkout(user);
            foreach (var workout in new List<Workout>{ workout1, workout2 })
            {
                foreach (var exercise in workout.ExerciseInstances)
                {
                    exercise.IsCompleted = true;
                    foreach (var set in exercise.Sets)
                    {
                        set.ActualReps = 12;
                        set.ActualWeight = 12;
                    }
                }
            }
            await _unitOfWork.SaveChangesAsync();
            var dailySet = CreateTestDailyStat(user);
        }
    }
}
