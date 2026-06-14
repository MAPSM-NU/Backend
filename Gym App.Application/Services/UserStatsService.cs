using Gym_App.Application.Authorization;
using Gym_App.Application.Hubs;
using Gym_App.Core;
using Gym_App.Domain;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Transfer_Classes;
using Gym_App.Infrastructure.DTOs.UserStats;
using Gym_App.Infrastructure.Interfaces.Services;
using Microsoft.AspNet.Identity;
using Microsoft.Extensions.Logging;
using System.Globalization;
using User = Gym_App.Domain.User;

namespace Gym_App.Application.Services
{
    public class UserStatsService : IUserStatsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICachedAuthorizationService _authorizationService;
        private readonly ILogger<UserStatsService> _logger;
        public UserStatsService(IUnitOfWork unitOfWork, ICachedAuthorizationService authorizationService, ILogger<UserStatsService> logger)
        {
            _unitOfWork = unitOfWork;
            _authorizationService = authorizationService;
            _logger = logger;
        }
        public async Task<SettersResponse> AddDailyStats(Workout workout)
        {
            if(workout == null || workout.User == null)
            {
                _logger.LogError($"Given workout is or has important values as null");
                return new SettersResponse { msg = "Given workout is or has important values as null", status = 0 };
            }
            var dailyStat = await _unitOfWork.UserStatDaily.GetUserStatsDaily(workout.User.Id, DateOnly.FromDateTime(DateTime.Now));
            if(dailyStat == null)
            {
                dailyStat = new UserStatsDaily
                {
                    user = workout.User,
                    userId = workout.User.Id,
                    date = DateOnly.FromDateTime(DateTime.Now),
                    year = DateTime.Now.Year,
                };

                dailyStat.KcaloriesBurned += workout.CaloriesBurned;
                foreach (var exercise in workout.ExerciseInstances!)
                {
                    dailyStat.totalExercisesCompleted++;
                    foreach (var set in exercise.Sets!)
                    {
                        dailyStat.totalSetsCompleted++;
                        dailyStat.totalReps += (int)set.ActualReps!;
                        dailyStat.totalWeightLifted += (double)set.ActualWeight!;
                    }
                }
                await _unitOfWork.UserStatDaily.Create(dailyStat);
                await _unitOfWork.SaveChangesAsync();
                return new SettersResponse { msg = "Daily stat created and done", status = 2 };
            }

            dailyStat.KcaloriesBurned += workout.CaloriesBurned;
            foreach(var exercise in workout.ExerciseInstances!)
            {
                dailyStat.totalExercisesCompleted++;
                foreach(var set in exercise.Sets!)
                {
                    dailyStat.totalSetsCompleted++;
                    dailyStat.totalReps += (int)set.ActualReps!;
                }
            }

            await _unitOfWork.UserStatDaily.Update(dailyStat);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { msg = "Daily stat done", status = 2 };
        }

        public async Task<SettersResponse> AddWeeklyStats(User user)
        {
            if( user == null)
            {
                _logger.LogError("Empty User given in while trying to retrieve weekly stats");
                return new SettersResponse { msg = "Empty user given", status = 0 };
            }
            var weekStart = DateOnly.FromDateTime(DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek));
            var weekNumber = ISOWeek.GetWeekOfYear(DateTime.Now);

            var weeklyStats = await _unitOfWork.UserStatWeekly.GetUserStatsWeekly(user.Id, weekNumber, DateTime.Now.Year);

            var weeklyDailyStats = await _unitOfWork.UserStatDaily.GetUserStatsDaysList(user.Id, weekStart, weekStart.AddDays(7));
            if(weeklyDailyStats.Count == 0)
            {
                _logger.LogError("Somehow Weekly Stats was called eventhough there are no daily stats recorder in current week");
                return new SettersResponse { msg = "No daily stats in this week", status = 0 };
            }

            if(weeklyStats == null)
            {
                weeklyStats = new UserStatsWeekly
                {
                    user = user,
                    userId = user.Id,
                    weekDate = weekStart,
                    weekNumber = weekNumber,
                    year = DateTime.Now.Year,
                };
                foreach(var item in weeklyDailyStats)
                {
                    weeklyStats.activeDays++;
                    weeklyStats.KcaloriesBurned += item.KcaloriesBurned;
                    weeklyStats.totalExercisesCompleted += item.totalExercisesCompleted;
                    weeklyStats.totalRepsCompleted += item.totalReps;
                    weeklyStats.totalWorkoutsCompleted++;
                    weeklyStats.totalSetsCompleted += item.totalSetsCompleted;
                    weeklyStats.totalWeightLifted += item.totalWeightLifted;
                    weeklyStats.workoutCompletionRate = weeklyStats.totalWorkoutsMissed == 0 ? 100 : weeklyStats.totalWorkoutsCompleted / (weeklyStats.totalWorkoutsCompleted + weeklyStats.totalWorkoutsMissed) * 100;
                    weeklyStats.userStatsDaily.Add(item);
                }
                await _unitOfWork.UserStatWeekly.Create(weeklyStats);
                await _unitOfWork.SaveChangesAsync();
                return new SettersResponse { msg = "Weekly stat created and updated", status = 2 };
            }

            foreach (var item in weeklyDailyStats)
            {
                weeklyStats.activeDays++;
                weeklyStats.KcaloriesBurned += item.KcaloriesBurned;
                weeklyStats.totalExercisesCompleted += item.totalExercisesCompleted;
                weeklyStats.totalRepsCompleted += item.totalReps;
                weeklyStats.totalWorkoutsCompleted++;
                weeklyStats.totalSetsCompleted += item.totalSetsCompleted;
                weeklyStats.totalWeightLifted += item.totalWeightLifted;
                weeklyStats.workoutCompletionRate = weeklyStats.totalWorkoutsMissed == 0 ? 100 : weeklyStats.totalWorkoutsCompleted / (weeklyStats.totalWorkoutsCompleted + weeklyStats.totalWorkoutsMissed) * 100;
                weeklyStats.userStatsDaily.Add(item);
            }
            await _unitOfWork.UserStatWeekly.Update(weeklyStats);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { msg = "Weekly stat updated", status = 2 };
        }
        public async Task<SettersResponse> AddWeeklyStats(UserStatsDaily usd)
        {
            if(usd == null)
            {
                _logger.LogError("Empty User Daily Stats given in while trying to retrieve weekly stats");
                return new SettersResponse { msg = "Empty User Daily Stats given", status = 0 };
            }

            var weekStart = DateOnly.FromDateTime(DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek));
            var weekNumber = ISOWeek.GetWeekOfYear(DateTime.Now);

            var weeklyStats = await _unitOfWork.UserStatWeekly.GetUserStatsWeekly(usd.userId, weekNumber, DateTime.Now.Year);

            if (weeklyStats == null)
            {
                weeklyStats = new UserStatsWeekly
                {
                    user = usd.user,
                    userId = usd.userId,
                    weekDate = weekStart,
                    weekNumber = weekNumber,
                    year = DateTime.Now.Year,
                    activeDays = 1,
                    KcaloriesBurned = usd.KcaloriesBurned,
                    totalExercisesCompleted = usd.totalExercisesCompleted,
                    totalRepsCompleted = usd.totalReps,
                    totalWorkoutsCompleted = 1,
                    totalSetsCompleted = usd.totalSetsCompleted,
                    totalWeightLifted = usd.totalWeightLifted,
                    workoutCompletionRate = 100,
                };
                weeklyStats.userStatsDaily.Add(usd);
                await _unitOfWork.UserStatWeekly.Create(weeklyStats);
                await _unitOfWork.SaveChangesAsync();
                return new SettersResponse { msg = "Weekly stat created and updated", status = 2 };
            }
            weeklyStats.activeDays++;
            weeklyStats.KcaloriesBurned += usd.KcaloriesBurned;
            weeklyStats.totalExercisesCompleted += usd.totalExercisesCompleted;
            weeklyStats.totalRepsCompleted += usd.totalReps;
            weeklyStats.totalWorkoutsCompleted++;
            weeklyStats.totalSetsCompleted += usd.totalSetsCompleted;
            weeklyStats.totalWeightLifted += usd.totalWeightLifted;
            weeklyStats.workoutCompletionRate = weeklyStats.totalWorkoutsMissed == 0 ? 100 : weeklyStats.totalWorkoutsCompleted / (weeklyStats.totalWorkoutsCompleted + weeklyStats.totalWorkoutsMissed) * 100;
            weeklyStats.userStatsDaily.Add(usd);

            await _unitOfWork.UserStatWeekly.Update(weeklyStats);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { msg = "Weekly stat updated", status = 2 };

        }

        public async Task<SettersResponse> AddMonthlyStats(User user)
        {
            if (user == null)
            {
                _logger.LogError("Empty User given in while trying to retrieve Monthly stats");
                return new SettersResponse { msg = "Empty User Daily Stats given", status = 0 };
            }
            int monthNumber = DateTime.Now.Month;
            DateOnly monthStart = DateOnly.FromDateTime(DateTime.Now.AddDays(-DateTime.Now.Day));
            int monthStartWeek = monthNumber * 4;

            int currentWeek = (ISOWeek.GetWeekOfYear(DateTime.Now) / 12) - 1 + monthStartWeek;
            int year = DateTime.Now.Year;
            List<int> weeklyStats = new List<int>();

            for(int i=monthStartWeek; i<=currentWeek; i++)weeklyStats.Add(i);

            var monthStat = await _unitOfWork.UserStatMonthly.GetUserStatsMonthly(user.Id, monthNumber, year);
            var monthWeeklyStats = await _unitOfWork.UserStatWeekly.GetUserStatsWeeksList(user.Id, weeklyStats, year);
            if(monthWeeklyStats.Count == 0)
            {
                _logger.LogError("Somehow Monthly Stats was called eventhough there are no weekly stats recorder in current week");
                return new SettersResponse { msg = "No WEEKLY stats in this month", status = 0 };
            }
            if (monthStat == null)
            {
                monthStat = new UserStatsMonthly
                {
                    user = user,
                    userId = user.Id,
                    year = year,
                    monthDate = monthStart,
                    monthName = DateTime.Now.ToString("MMMM"),
                    monthNumber = monthNumber,
                };
                foreach(var usw in monthWeeklyStats)
                {
                    monthStat.totalRepsCompleted += usw.totalRepsCompleted;
                    monthStat.totalExercisesCompleted += usw.totalExercisesCompleted;
                    monthStat.totalWorkoutsCompleted += usw.totalWorkoutsCompleted;
                    monthStat.activeDays += usw.activeDays;
                    monthStat.KcaloriesBurned += usw.KcaloriesBurned;
                    monthStat.totalWorkoutsMissed += usw.totalWorkoutsMissed;
                    monthStat.totalWeightLifted += usw.totalWeightLifted;
                    monthStat.totalSetsCompleted += usw.totalSetsCompleted;
                    monthStat.userStatsWeekly.Add(usw);
                }
                monthStat.workoutCompletionRate = monthStat.totalWorkoutsMissed == 0 ? 100 : monthStat.totalWorkoutsCompleted / (monthStat.totalWorkoutsCompleted + monthStat.totalWorkoutsMissed) * 100;
                await _unitOfWork.UserStatMonthly.Create(monthStat);
                await _unitOfWork.SaveChangesAsync();
                return new SettersResponse { msg = "Month stat created and updated", status = 2 };
            }
            foreach (var usw in monthWeeklyStats)
            {
                monthStat.totalRepsCompleted += usw.totalRepsCompleted;
                monthStat.totalExercisesCompleted += usw.totalExercisesCompleted;
                monthStat.totalWorkoutsCompleted += usw.totalWorkoutsCompleted;
                monthStat.activeDays += usw.activeDays;
                monthStat.KcaloriesBurned += usw.KcaloriesBurned;
                monthStat.totalWorkoutsMissed += usw.totalWorkoutsMissed;
                monthStat.totalWeightLifted += usw.totalWeightLifted;
                monthStat.totalSetsCompleted += usw.totalSetsCompleted;
                monthStat.userStatsWeekly.Add(usw);
            }
            monthStat.workoutCompletionRate = monthStat.totalWorkoutsMissed == 0 ? 100 : monthStat.totalWorkoutsCompleted / (monthStat.totalWorkoutsCompleted + monthStat.totalWorkoutsMissed) * 100;
            await _unitOfWork.UserStatMonthly.Update(monthStat);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { msg = "Month stat updated", status = 2 };
        }
        public async Task<SettersResponse> AddMonthlyStats(UserStatsWeekly usw)
        {
            if (usw == null)
            {
                _logger.LogError("Empty User weekly Stats given in while trying to retrieve monthy stats");
                return new SettersResponse { msg = "Empty User Weekly Stats given", status = 0 };
            }
            int monthNumber = DateTime.Now.Month;
            DateOnly monthStart = DateOnly.FromDateTime(DateTime.Now.AddDays(-DateTime.Now.Day));
            int monthStartWeek = monthNumber * 4;
            int currentWeek = (ISOWeek.GetWeekOfYear(DateTime.Now) / 12) - 1 + monthStartWeek;
            int year = DateTime.Now.Year;

            var monthStat = await _unitOfWork.UserStatMonthly.GetUserStatsMonthly(usw.userId, monthNumber, year);
            if (monthStat == null)
            {
                monthStat = new UserStatsMonthly
                {
                    user = usw.user,
                    userId = usw.userId,
                    year = year,
                    monthDate = monthStart,
                    monthName = DateTime.Now.ToString("MMMM"),
                    monthNumber = monthNumber,
                };
                monthStat.totalRepsCompleted += usw.totalRepsCompleted;
                monthStat.totalExercisesCompleted += usw.totalExercisesCompleted;
                monthStat.totalWorkoutsCompleted += usw.totalWorkoutsCompleted;
                monthStat.activeDays += usw.activeDays;
                monthStat.KcaloriesBurned += usw.KcaloriesBurned;
                monthStat.totalWorkoutsMissed += usw.totalWorkoutsMissed;
                monthStat.totalWeightLifted += usw.totalWeightLifted;
                monthStat.totalSetsCompleted += usw.totalSetsCompleted;
                monthStat.userStatsWeekly.Add(usw);
                monthStat.workoutCompletionRate = monthStat.totalWorkoutsMissed == 0 ? 100 : monthStat.totalWorkoutsCompleted / (monthStat.totalWorkoutsCompleted + monthStat.totalWorkoutsMissed) * 100;
                await _unitOfWork.UserStatMonthly.Create(monthStat);
                await _unitOfWork.SaveChangesAsync();
                return new SettersResponse { msg = "Month stat created and updated", status = 2 };
            }
            monthStat.totalRepsCompleted += usw.totalRepsCompleted;
            monthStat.totalExercisesCompleted += usw.totalExercisesCompleted;
            monthStat.totalWorkoutsCompleted += usw.totalWorkoutsCompleted;
            monthStat.activeDays += usw.activeDays;
            monthStat.KcaloriesBurned += usw.KcaloriesBurned;
            monthStat.totalWorkoutsMissed += usw.totalWorkoutsMissed;
            monthStat.totalWeightLifted += usw.totalWeightLifted;
            monthStat.totalSetsCompleted += usw.totalSetsCompleted;
            monthStat.userStatsWeekly.Add(usw);
            monthStat.workoutCompletionRate = monthStat.totalWorkoutsMissed == 0 ? 100 : monthStat.totalWorkoutsCompleted / (monthStat.totalWorkoutsCompleted + monthStat.totalWorkoutsMissed) * 100;
            await _unitOfWork.UserStatMonthly.Update(monthStat);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { msg = "Month stat updated", status = 2 };
        }

        public Task<GettersResponse<UserStatsDailyDTO>> GetDailyStats(Guid userId, DateOnly date)
        {
            throw new NotImplementedException();
        }
        public Task<GettersResponse<UserStatsWeeklyDTO>> GetWeeklyStats(Guid userId, int weekNumber, int year)
        {
            throw new NotImplementedException();
        }

        public Task<GettersResponse<UserStatsMonthlyDTO>> GetMonthlyStats(Guid userId, string monthName, int year)
        {
            throw new NotImplementedException();
        }

        public Task<GettersResponse<UserStatsDTO>> GetOverallStats(Guid userId)
        {
            throw new NotImplementedException();
        }


    }
}
