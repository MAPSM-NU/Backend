using Gym_App.Application.Authorization;
using Gym_App.Core;
using Gym_App.Domain;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Transfer_Classes;
using Gym_App.Infrastructure.DTOs.UserStats;
using Gym_App.Infrastructure.Interfaces.Services;
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
                foreach (var exercise in workout.ExerciseInstances!)
                {
                    dailyStat.totalExercisesCompleted++;
                    foreach (var set in exercise.Sets!)
                    {
                        dailyStat.totalSetsCompleted++;
                        dailyStat.totalReps += (int)set.ActualReps!;
                        dailyStat.totalWeightLifted += (double)set.ActualWeight!;
                        dailyStat.KcaloriesBurned += set.KCaloriesBurned;
                    }
                }
                dailyStat.totalWorkoutCompleted++;
                await _unitOfWork.UserStatDaily.Create(dailyStat);
                await _unitOfWork.SaveChangesAsync();
                return new SettersResponse { msg = "Daily stat created and done", status = 2 };
            }

            
            foreach(var exercise in workout.ExerciseInstances!)
            {
                dailyStat.totalExercisesCompleted++;
                foreach(var set in exercise.Sets!)
                {
                    dailyStat.totalSetsCompleted++;
                    dailyStat.totalReps += (int)set.ActualReps!;
                    dailyStat.KcaloriesBurned += set.KCaloriesBurned;
                }
            }
            dailyStat.totalWorkoutCompleted++;

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
                    weeklyStats.totalWorkoutsCompleted += item.totalWorkoutCompleted;
                    weeklyStats.totalSetsCompleted += item.totalSetsCompleted;
                    weeklyStats.totalWeightLifted += item.totalWeightLifted;
                    weeklyStats.workoutCompletionRate = weeklyStats.totalWorkoutsMissed == 0 ? 100 : weeklyStats.totalWorkoutsCompleted / (weeklyStats.totalWorkoutsCompleted + weeklyStats.totalWorkoutsMissed) * 100;
                    weeklyStats.userStatsDaily.Add(item);
                }
                await _unitOfWork.UserStatWeekly.Create(weeklyStats);
                await _unitOfWork.SaveChangesAsync();
                return new SettersResponse { msg = "Weekly stat created and updated", status = 2 };
            }
            clearWeeklyStat(weeklyStats); // clearing so items updated don't aggregate the values they already have with their newer values
            foreach (var item in weeklyDailyStats)
            {
                weeklyStats.KcaloriesBurned += item.KcaloriesBurned;
                weeklyStats.totalExercisesCompleted += item.totalExercisesCompleted;
                weeklyStats.totalRepsCompleted += item.totalReps;
                weeklyStats.totalWorkoutsCompleted += item.totalWorkoutCompleted;
                weeklyStats.totalSetsCompleted += item.totalSetsCompleted;
                weeklyStats.totalWeightLifted += item.totalWeightLifted;
                weeklyStats.workoutCompletionRate = weeklyStats.totalWorkoutsMissed == 0 ? 100 : weeklyStats.totalWorkoutsCompleted / (weeklyStats.totalWorkoutsCompleted + weeklyStats.totalWorkoutsMissed) * 100;
                if (!weeklyStats.userStatsDaily.Contains(item)) 
                { 
                    weeklyStats.userStatsDaily.Add(item);
                    weeklyStats.activeDays++;
                }
            }
            await _unitOfWork.UserStatWeekly.Update(weeklyStats);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { msg = "Weekly stat updated", status = 2 };
        }
        public async Task<SettersResponse> AddWeeklyStats(UserStatsDaily usd)
        {
            //needs slight rework
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
                    totalWorkoutsCompleted = usd.totalWorkoutCompleted,
                    totalSetsCompleted = usd.totalSetsCompleted,
                    totalWeightLifted = usd.totalWeightLifted,
                    workoutCompletionRate = 100,
                };
                weeklyStats.userStatsDaily.Add(usd);
                await _unitOfWork.UserStatWeekly.Create(weeklyStats);
                await _unitOfWork.SaveChangesAsync();
                return new SettersResponse { msg = "Weekly stat created and updated", status = 2 };
            }
            if (!weeklyStats.userStatsDaily.Contains(usd))
            {
                weeklyStats.activeDays++;
                weeklyStats.userStatsDaily.Add(usd);
            }
            weeklyStats.KcaloriesBurned += usd.KcaloriesBurned;
            weeklyStats.totalExercisesCompleted += usd.totalExercisesCompleted;
            weeklyStats.totalRepsCompleted += usd.totalReps;
            weeklyStats.totalWorkoutsCompleted += usd.totalWorkoutCompleted;
            weeklyStats.totalSetsCompleted += usd.totalSetsCompleted;
            weeklyStats.totalWeightLifted += usd.totalWeightLifted;
            weeklyStats.workoutCompletionRate = weeklyStats.totalWorkoutsMissed == 0 ? 100 : weeklyStats.totalWorkoutsCompleted / (weeklyStats.totalWorkoutsCompleted + weeklyStats.totalWorkoutsMissed) * 100;

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
            clearMonthlyStat(monthStat);
            foreach (var usw in monthWeeklyStats)
            {
                monthStat.totalRepsCompleted += usw.totalRepsCompleted;
                monthStat.totalExercisesCompleted += usw.totalExercisesCompleted;
                monthStat.totalWorkoutsCompleted += usw.totalWorkoutsCompleted;
                monthStat.KcaloriesBurned += usw.KcaloriesBurned;
                monthStat.totalWorkoutsMissed += usw.totalWorkoutsMissed;
                monthStat.totalWeightLifted += usw.totalWeightLifted;
                monthStat.totalSetsCompleted += usw.totalSetsCompleted;
                if (!monthStat.userStatsWeekly.Contains(usw))
                {
                    monthStat.userStatsWeekly.Add(usw);
                    monthStat.activeDays += usw.activeDays;
                }
            }
            monthStat.workoutCompletionRate = monthStat.totalWorkoutsMissed == 0 ? 100 : monthStat.totalWorkoutsCompleted / (monthStat.totalWorkoutsCompleted + monthStat.totalWorkoutsMissed) * 100;
            await _unitOfWork.UserStatMonthly.Update(monthStat);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { msg = "Month stat updated", status = 2 };
        }
        public async Task<SettersResponse> AddMonthlyStats(UserStatsWeekly usw)
        {
            //needs refactoring cause it is not working right
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
            monthStat.KcaloriesBurned += usw.KcaloriesBurned;
            monthStat.totalWorkoutsMissed += usw.totalWorkoutsMissed;
            monthStat.totalWeightLifted += usw.totalWeightLifted;
            monthStat.totalSetsCompleted += usw.totalSetsCompleted;
            monthStat.workoutCompletionRate = monthStat.totalWorkoutsMissed == 0 ? 100 : monthStat.totalWorkoutsCompleted / (monthStat.totalWorkoutsCompleted + monthStat.totalWorkoutsMissed) * 100;
            if (!monthStat.userStatsWeekly.Contains(usw))
            {
                monthStat.userStatsWeekly.Add(usw);
                monthStat.activeDays += usw.activeDays;
            }
            await _unitOfWork.UserStatMonthly.Update(monthStat);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { msg = "Month stat updated", status = 2 };
        }
        public async Task<SettersResponse> AddAllTimeStats(Workout workout)
        {
            if(workout == null || workout.User == null)
            {
                _logger.LogError("Empty workout data while tryng to update all time stats");
                return new SettersResponse { msg = "Empty Data", status = 0 };
            }
            bool created = false;
            //stats update
            var userStats = await _unitOfWork.UserStats.GetUserStatsByUserId(workout.User.Id);
            if(userStats == null)
            {
                userStats = new UserStats
                {
                    user = workout.User,
                    userId = workout.User.Id,
                };
                created = true;
            }
            var exercisesBeforeUpdate = userStats.totalExercisesCompleted;
            foreach (var exerciseInstance in workout.ExerciseInstances)
            {
                if (exerciseInstance.IsCompleted)
                {
                    userStats.totalExercisesCompleted++;
                }
            }
            var exercisesDone = exercisesBeforeUpdate == userStats.totalExercisesCompleted ? 0 : userStats.totalExercisesCompleted - exercisesBeforeUpdate;

            var exercisesInWorkout = workout.ExerciseInstances.Count;
            if (exercisesInWorkout <= 0 || exercisesDone == 0 || ((double)exercisesDone / exercisesInWorkout) < 0.5)
            {//Check if exercises are little to finish the the workout or the user done less than 50% of the exercises
                _logger.LogInformation($"User {workout.User.Id} missed this workout as they completed only {exercisesDone} out of {exercisesInWorkout} exercises");
                return new SettersResponse { status = 0, msg = "Either you didn't finish atleast 50% of the exercises in the workout or there were less than 3" };
            }
            
            var startTime = workout.ActualStartTime;
            var endTime = workout.ActualEndTime;
            TimeSpan hoursInExercise = workout.ActualStartTime != null && workout.ActualEndTime != null
                ? (workout.ActualEndTime.Value - workout.ActualStartTime.Value)
                : TimeSpan.Zero;
            userStats.totalHours += hoursInExercise.TotalHours;

            userStats.totalWorkoutsCompleted++;
            userStats.workoutStreak++;
            if (userStats.workoutStreak > userStats.longestStreak)
            {
                _logger.LogInformation($"New longest workout streak achieved: {userStats.workoutStreak} for user {workout.User.Id}");
                userStats.longestStreak = userStats.workoutStreak;
            }
            userStats.workoutCompletionRate = userStats.totalWorkoutsMissed == 0 ? 100 :
                (double)userStats.totalWorkoutsCompleted / (userStats.totalWorkoutsCompleted + userStats.totalWorkoutsMissed) * 100;
            if(created)
                await _unitOfWork.UserStats.Create(userStats);
            else
                await _unitOfWork.UserStats.Update(userStats);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { msg = "Done", status = 2 };
        }
        public async Task<GettersResponse<UserStatsDailyDTO>> GetDailyStats(Guid userId, DateOnly date)
        {
            if (userId == Guid.Empty)
            {
                _logger.LogError("Tried to access daily stats with empty userId");
                return new GettersResponse<UserStatsDailyDTO> { msg = "empty UserID", status = 0};
            }

            var authResult = await _authorizationService.IsUserAsync(userId);
            if(!authResult)
                return new GettersResponse<UserStatsDailyDTO> { msg = "Unauthorized", status = 1 };

            var usd = await _unitOfWork.UserStatDaily.GetUserStatsDaily(userId, date);
            if(usd == null)
            {
                _logger.LogError($"User: {userId} tried to access daily stat by the date of {date}, but it does not exist");
                return new GettersResponse<UserStatsDailyDTO> { msg = "Not found", status = 0 };
            }
            var usdDto = new UserStatsDailyDTO
            {
                userId = userId,
                totalSetsCompleted = usd.totalSetsCompleted,
                date = usd.date,
                dayOfWeek = usd.dayOfWeek,
                KcaloriesBurned = usd.KcaloriesBurned,
                totalExercisesCompleted = usd.totalExercisesCompleted,
                totalHours = usd.totalHours,
                totalReps = usd.totalReps,
                year = usd.year
            };

            return new GettersResponse<UserStatsDailyDTO> { msg = "Here is the data", status = 2, Value = usdDto };
        }
        public async Task<GettersResponse<UserStatsWeeklyDTO>> GetWeeklyStats(Guid userId, int weekNumber, int year)
        {
            if (userId == Guid.Empty)
            {
                _logger.LogError("Tried to access weekly stats with empty userId");
                return new GettersResponse<UserStatsWeeklyDTO> { msg = "empty UserID", status = 0 };
            }

            var authResult = await _authorizationService.IsUserAsync(userId);
            if (!authResult)
                return new GettersResponse<UserStatsWeeklyDTO> { msg = "Unauthorized", status = 1 };

            var usw = await _unitOfWork.UserStatWeekly.GetUserStatsWeekly(userId, weekNumber, year);
            if (usw == null)
            {
                _logger.LogError($"User: {userId} tried to access weekly stat by the number of {weekNumber} in year {year}, but it does not exist");
                return new GettersResponse<UserStatsWeeklyDTO> { msg = "Not found", status = 0 };
            }
            var uswDto = new UserStatsWeeklyDTO
            {
                userId = userId,
                totalSetsCompleted = usw.totalSetsCompleted,
                workoutStreak = usw.workoutStreak,
                activeDays = usw.activeDays,
                KcaloriesBurned = usw.KcaloriesBurned,
                personalRecordsBroken = usw.personalRecordsBroken,
                totalExercisesCompleted = usw.totalExercisesCompleted,
                totalHours = usw.totalHours,
                totalRepsCompleted = usw.totalRepsCompleted,
                totalWeightLifted = usw.totalWeightLifted,
                totalWorkoutsCompleted = usw.totalWorkoutsCompleted,
                totalWorkoutsMissed = usw.totalWorkoutsMissed,
                weekDate = usw.weekDate,
                weeklyGoalAchieved = usw.weeklyGoalAchieved,
                weekNumber = usw.weekNumber,
                workoutCompletionRate = usw.workoutCompletionRate,
                year = usw.year
            };
            return new GettersResponse<UserStatsWeeklyDTO> { msg = "Here is the data", status = 2, Value = uswDto };
        }

        public async Task<GettersResponse<UserStatsMonthlyDTO>> GetMonthlyStats(Guid userId, string monthName, int year)
        {
            if (userId == Guid.Empty)
            {
                _logger.LogError("Tried to access weekly stats with empty userId");
                return new GettersResponse<UserStatsMonthlyDTO> { msg = "empty UserID", status = 0 };
            }

            var authResult = await _authorizationService.IsUserAsync(userId);
            if (!authResult)
                return new GettersResponse<UserStatsMonthlyDTO> { msg = "Unauthorized", status = 1 };

            var usw = await _unitOfWork.UserStatMonthly.GetUserStatsMonthly(userId, monthName, year);
            if (usw == null)
            {
                _logger.LogError($"User: {userId} tried to access weekly stat by the number of {monthName} in year {year}, but it does not exist");
                return new GettersResponse<UserStatsMonthlyDTO> { msg = "Not found", status = 0 };
            }
            var uswDto = new UserStatsMonthlyDTO
            {
                userId = userId,
                totalSetsCompleted = usw.totalSetsCompleted,
                workoutStreak = usw.workoutStreak,
                activeDays = usw.activeDays,
                KcaloriesBurned = usw.KcaloriesBurned,
                personalRecordsBroken = usw.personalRecordsBroken,
                totalExercisesCompleted = usw.totalExercisesCompleted,
                totalHours = usw.totalHours,
                totalRepsCompleted = usw.totalRepsCompleted,
                totalWeightLifted = usw.totalWeightLifted,
                totalWorkoutsCompleted = usw.totalWorkoutsCompleted,
                totalWorkoutsMissed = usw.totalWorkoutsMissed,
                longestWorkoutStreak = usw.longestWorkoutStreak,
                monthDate = usw.monthDate,
                monthName = usw.monthName,
                monthNumber = usw.monthNumber,
                weeklyGoalAchieved = usw.weeklyGoalAchieved,
                workoutCompletionRate = usw.workoutCompletionRate,
                year = usw.year
            };
            return new GettersResponse<UserStatsMonthlyDTO> { msg = "Here is the data", status = 2, Value = uswDto };
        }

        public async Task<GettersResponse<UserStatsDTO>> GetOverallStats(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                _logger.LogError("Tried to access all-time stats with empty userId");
                return new GettersResponse<UserStatsDTO> { msg = "empty UserID", status = 0 };
            }

            var authResult = await _authorizationService.IsUserAsync(userId);
            if (!authResult)
                return new GettersResponse<UserStatsDTO> { msg = "Unauthorized", status = 1 };

            var usw = await _unitOfWork.UserStats.GetUserStatsByUserId(userId);
            if(usw == null)
            {
                _logger.LogError($"User: {userId} tried to access all-time stats, but it does not exist");
                return new GettersResponse<UserStatsDTO> { msg = "Not found", status = 0 };
            }
            var uswDto = new UserStatsDTO
            {
                userId = userId,
                longestStreak = usw.longestStreak,
                workoutStreak = usw.workoutStreak,
                workoutCompletionRate = usw.workoutCompletionRate,
                totalExercisesCompleted = usw.totalExercisesCompleted,
                totalHours = usw.totalHours,
                totalWorkoutsCompleted = usw.totalWorkoutsCompleted,
                totalWorkoutsMissed = usw.totalWorkoutsMissed,
            };
            return new GettersResponse<UserStatsDTO> { msg = "Here is the data", status = 2, Value = uswDto };
        }

        private void clearWeeklyStat(UserStatsWeekly usw)
        {
            usw.totalSetsCompleted = 0;
            usw.workoutStreak = 0;
            usw.KcaloriesBurned = 0;
            usw.personalRecordsBroken = 0;
            usw.totalExercisesCompleted = 0;
            usw.totalHours = 0;
            usw.totalRepsCompleted = 0;
            usw.totalWeightLifted = 0;
            usw.totalWorkoutsCompleted = 0;
            usw.totalWorkoutsMissed = 0;
        }
        private void clearMonthlyStat(UserStatsMonthly usw)
        {
            usw.totalSetsCompleted = 0;
            usw.workoutStreak = 0;
            usw.KcaloriesBurned = 0;
            usw.personalRecordsBroken = 0;
            usw.totalExercisesCompleted = 0;
            usw.totalHours = 0;
            usw.totalRepsCompleted = 0;
            usw.totalWeightLifted = 0;
            usw.totalWorkoutsCompleted = 0;
            usw.totalWorkoutsMissed = 0;
        }
    }
}
