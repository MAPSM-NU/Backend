using Gym_App.Application.Authorization;
using Gym_App.Application.Services;
using Gym_App.Infrastructure.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Globalization;

namespace GymApp.Tests.UserStats
{
    public class UserStatsAdding : TestBase
    {
        private readonly Mock<ICachedAuthorizationService> _authorizationService;
        private readonly Mock<ILogger<UserStatsService>> _logger;
        private readonly IUserStatsService _userStatsService;
        public UserStatsAdding() : base("UserStatsDatabase")
        {
            _authorizationService = new Mock<ICachedAuthorizationService>();
            _logger = new Mock<ILogger<UserStatsService>>();
            _userStatsService = new UserStatsService(_unitOfWork, _authorizationService.Object,_logger.Object);
        }
        //Daily adding
        [Fact]
        public async Task AddStatDaily()
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
            Assert.Equal("Daily stat done", result.msg);
            Assert.Equal(2, result.status);

            var stat = await _unitOfWork.UserStatDaily.GetUserStatsDaily(user.Id, DateOnly.FromDateTime(DateTime.Today));
            Assert.NotNull(stat);
            Assert.Equal(36, stat.totalReps);
            Assert.Equal(3, stat.totalExercisesCompleted);
            Assert.Equal(DateTime.Now.DayOfWeek.ToString(), stat.dayOfWeek);
        }
        [Fact]
        public async Task AddStatDaily2Times()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout1 = CreateTestWorkout(user);
            foreach (var exercise in workout1.ExerciseInstances)
            {
                exercise.IsCompleted = true;
                foreach (var set in exercise.Sets)
                {
                    set.ActualReps = 12;
                    set.ActualWeight = 12;
                }
            }
            var workout2 = CreateTestWorkout(user);
            foreach (var exercise in workout2.ExerciseInstances)
            {
                exercise.IsCompleted = true;
                foreach (var set in exercise.Sets)
                {
                    set.ActualReps = 12;
                    set.ActualWeight = 12;
                }
            }
            await _unitOfWork.SaveChangesAsync();
            var result = await _userStatsService.AddDailyStats(workout1);
            Assert.Equal("Daily stat done", result.msg);
            Assert.Equal(2, result.status);

            result = await _userStatsService.AddDailyStats(workout2);
            Assert.Equal("Daily stat done", result.msg);
            Assert.Equal(2, result.status);

            var stat = await _unitOfWork.UserStatDaily.GetUserStatsDaily(user.Id, DateOnly.FromDateTime(DateTime.Today));
            Assert.NotNull(stat);
            Assert.Equal(36*2, stat.totalReps);
            Assert.Equal(3*2, stat.totalExercisesCompleted);
            Assert.Equal(DateTime.Now.DayOfWeek.ToString(), stat.dayOfWeek);
        }
        [Fact]
        public async Task AddStatDailyButNoUser()
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
            Assert.Equal("Given workout is or has important values as null", result.msg);
            Assert.Equal(0, result.status);
        }
        //Weekly Adding
        [Fact]
        public async Task AddStatWeeklyGivenDailyStat()
        {
            var user = CreateTestUser(CreateTestRole());
            var dailySet1 = CreateTestDailyStat(user, date:DateOnly.FromDateTime(DateTime.Now));
            var dailySet2 = CreateTestDailyStat(user, date:DateOnly.FromDateTime(DateTime.Now.AddDays(1)));
            await _unitOfWork.SaveChangesAsync();

            var result = await _userStatsService.AddWeeklyStats(dailySet1);
            Assert.Equal("Weekly stat updated", result.msg);

            result = await _userStatsService.AddWeeklyStats(dailySet2);
            Assert.Equal("Weekly stat updated", result.msg);

            var stat = await _unitOfWork.UserStatWeekly.GetUserStatsWeekly(user.Id, ISOWeek.GetWeekOfYear(DateTime.Now), DateTime.Now.Year);
            Assert.NotNull(stat);
            Assert.Equal(24, stat.totalRepsCompleted);
            Assert.Equal(6, stat.totalExercisesCompleted);
            Assert.Equal(2, stat.activeDays);
            Assert.Equal(6, stat.totalSetsCompleted);
            Assert.Equal(24, stat.KcaloriesBurned);
            Assert.Equal(2, stat.totalWorkoutsCompleted);
            Assert.Equal(100, stat.workoutCompletionRate);
        }
        [Fact]
        public async Task AddStatWeeklyUsingUser()
        {
            var user = CreateTestUser(CreateTestRole());
            var dailySet1 = CreateTestDailyStat(user, date: DateOnly.FromDateTime(DateTime.Now));
            var dailySet2 = CreateTestDailyStat(user, date: DateOnly.FromDateTime(DateTime.Now.AddDays(1)));
            await _unitOfWork.SaveChangesAsync();

            var result = await _userStatsService.AddWeeklyStats(user);
            Assert.Equal("Weekly stat updated", result.msg);

            var stat = await _unitOfWork.UserStatWeekly.GetUserStatsWeekly(user.Id, ISOWeek.GetWeekOfYear(DateTime.Now), DateTime.Now.Year);
            Assert.NotNull(stat);
            Assert.Equal(24, stat.totalRepsCompleted);
            Assert.Equal(6, stat.totalExercisesCompleted);
            Assert.Equal(2, stat.activeDays);
            Assert.Equal(6, stat.totalSetsCompleted);
            Assert.Equal(24, stat.KcaloriesBurned);
            Assert.Equal(2, stat.totalWorkoutsCompleted);
            Assert.Equal(100, stat.workoutCompletionRate);
        }
        [Fact]
        public async Task AddStatWeeklyWithUserIdtButNotFound()
        {
            var user = CreateTestUser(CreateTestRole());
            await _unitOfWork.SaveChangesAsync();

            var result = await _userStatsService.AddWeeklyStats(user);
            Assert.Equal("No daily stats in this week", result.msg);
        }
        [Fact]
        public async Task AddStatMonthlyWithWeeklyStat()
        {
            var user = CreateTestUser(CreateTestRole());
            var weeklyStat1 = CreateTestWeeklyStat(
                user,
                DateOnly.FromDateTime(DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek)),
                ISOWeek.GetWeekOfYear(DateTime.Now)
            );
            var weeklyStat2 = CreateTestWeeklyStat(
                user,
                DateOnly.FromDateTime(DateTime.Today.AddDays(7-(int)DateTime.Today.DayOfWeek)),
                ISOWeek.GetWeekOfYear(DateTime.Now.AddDays(7))
            );
            await _unitOfWork.SaveChangesAsync();

            var result = await _userStatsService.AddMonthlyStats(weeklyStat1);
            Assert.Equal("Month stat updated", result.msg);

            result = await _userStatsService.AddMonthlyStats(weeklyStat2);
            Assert.Equal("Month stat updated", result.msg);

            var stat = await _unitOfWork.UserStatMonthly.GetUserStatsMonthly(user.Id, DateTime.Now.Month, DateTime.Now.Year);
            Assert.NotNull(stat);
            Assert.Equal(72, stat.totalRepsCompleted);
            Assert.Equal(18, stat.totalExercisesCompleted);
            Assert.Equal(6, stat.activeDays);
            Assert.Equal(18, stat.totalSetsCompleted);
            Assert.Equal(72, stat.KcaloriesBurned);
            Assert.Equal(6, stat.totalWorkoutsCompleted);
            Assert.Equal(100, stat.workoutCompletionRate);
        }
        [Fact]
        public async Task AddStatMonthlyWithUser()
        {
            var user = CreateTestUser(CreateTestRole());
            var weeklyStat1 = CreateTestWeeklyStat(
                user,
                DateOnly.FromDateTime(DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek)),
                ISOWeek.GetWeekOfYear(DateTime.Now)
            );
            var weeklyStat2 = CreateTestWeeklyStat(
                user,
                DateOnly.FromDateTime(DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek)),
                ISOWeek.GetWeekOfYear(DateTime.Now.AddDays(-7))
            );
            await _unitOfWork.SaveChangesAsync();

            var result = await _userStatsService.AddMonthlyStats(user);
            Assert.Equal("Month stat updated", result.msg);

            var stat = await _unitOfWork.UserStatMonthly.GetUserStatsMonthly(user.Id, DateTime.Now.Month, DateTime.Now.Year);
            Assert.NotNull(stat);
            Assert.Equal(72, stat.totalRepsCompleted);
            Assert.Equal(18, stat.totalExercisesCompleted);
            Assert.Equal(6, stat.activeDays);
            Assert.Equal(18, stat.totalSetsCompleted);
            Assert.Equal(72, stat.KcaloriesBurned);
            Assert.Equal(6, stat.totalWorkoutsCompleted);
            Assert.Equal(100, stat.workoutCompletionRate);
        }
    }
}
