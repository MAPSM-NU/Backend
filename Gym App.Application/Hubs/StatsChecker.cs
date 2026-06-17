
using Gym_App.Domain;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infrastructure.Interfaces.Services;
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
        private readonly ILogger<StatsChecker> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(12); // Check every 12 hours
        public StatsChecker(IServiceScopeFactory serviceProvider, ILogger<StatsChecker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task CheckMissedWorkout()
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var stats = scope.ServiceProvider.GetRequiredService<IUserStatsService>();
            try
            {
                bool anychange = false;
                var yesterdayWorkouts = await unitOfWork.Workouts.GetAll().Include(w => w.User).Where(w =>
                        w.ScheduledStartTime <= DateTime.Now.AddDays(-1)&&
                        !w.IsCompleted &&
                        !w.isMissed
                    ).ToListAsync();
                foreach (var workout in yesterdayWorkouts)
                {
                    var result = await stats.CheckMissedWorkout(workout);
                    if (result.status != 2)
                        _logger.LogError($"Error while checking for missed workouts : {result.msg}");
                    else
                    {
                        anychange = true;
                        _logger.LogInformation($"missed workout protocol successfully documented: {result.msg}");
                        workout.isMissed = true;
                        await unitOfWork.Workouts.Update(workout);
                    }
                }
                if(anychange)
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
