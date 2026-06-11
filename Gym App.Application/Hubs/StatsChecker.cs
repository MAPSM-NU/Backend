
using Gym_App.Domain;
using Gym_App.Infastructure.Interfaces.Repositries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gym_App.Application.Hubs
{
    public interface IStatsChecker
    {
        Task CheckMissedWorkout();
        Task CheckStats(Guid userId);
    }
    public class StatsChecker : BackgroundService, IStatsChecker
    {
        private readonly IServiceScopeFactory _serviceProvider;
        private readonly ILogger<WorkoutNotifier> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(12); // Check every 12 hours
        public StatsChecker(IServiceScopeFactory serviceProvider, ILogger<WorkoutNotifier> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task CheckMissedWorkout()
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            try
            {

                var yesterdayWorkouts = unitOfWork.Workouts.GetAll().Include(w => w.User).ThenInclude(u=>u.UserStats).Where(w =>
                        w.ScheduledStartTime > DateTime.Today.AddDays(-1) && w.ScheduledStartTime <= DateTime.Today && !w.IsCompleted
                    );
                foreach (var workout in yesterdayWorkouts)
                {
                    workout.User.UserStats.totalWorkoutsMissed++;
                    workout.User.UserStats.workoutStreak = 0;
                    workout.User.UserStats.workoutCompletionRate = workout.User.UserStats.totalWorkoutsCompleted > 0 ? 
                            (double)workout.User.UserStats.totalWorkoutsCompleted / (workout.User.UserStats.totalWorkoutsCompleted + workout.User.UserStats.totalWorkoutsMissed) * 100
                        : 0;
                    await unitOfWork.Workouts.Update(workout);
                    _logger.LogInformation($"Workout {workout.Name} for user {workout.User.Name} was missed. Total missed workouts: {workout.User.UserStats.totalWorkoutsMissed}");
                }

                await unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking missed workouts");
            }
        }

        public Task CheckStats(Guid userId)
        {
            throw new NotImplementedException();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Stats Checker started at {DateTime.Now}");
            var timer = new PeriodicTimer(_interval);
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await CheckMissedWorkout();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in StatsChecker");
                }
            }
        }
    }
}
