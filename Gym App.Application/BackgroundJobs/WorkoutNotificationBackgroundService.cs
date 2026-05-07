using Gym_App.Application.Services;
using Gym_App.Domain;
using Gym_App.Infastructure.DTOs.Notification;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gym_App.Application.BackgroundJobs
{
    public interface IWorkoutNotificationSink
    {
        /// <summary>
        /// Check for upcoming workouts and send reminder notifications
        /// </summary>
        Task SendUpcomingWorkoutRemindersAsync();

        /// <summary>
        /// Send notification when user starts a workout
        /// </summary>
        Task SendWorkoutStartedNotificationAsync(Guid workoutId, Guid userId);

        /// <summary>
        /// Send notification when user completes a workout
        /// </summary>
        Task SendWorkoutCompletedNotificationAsync(Guid workoutId, Guid userId);

        /// <summary>
        /// Check for new personal records and send notifications
        /// </summary>
        Task SendPersonalRecordNotificationsAsync();

        /// <summary>
        /// Send notification for a new personal record
        /// </summary>
        Task SendPRNotificationAsync(Guid userId, PersonalRecord pr, ExerciseInstance exerciseInstance);

        /// <summary>
        /// Get personalized workout stats for notification
        /// </summary>
        Task<WorkoutNotificationStatsDTO> GetWorkoutStatsAsync(Guid workoutId);
    }
    /// <summary>
    /// Background service that runs periodic tasks for sending workout notifications
    /// </summary>
    public class WorkoutNotificationBackgroundService : BackgroundService , IWorkoutNotificationSink
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WorkoutNotificationBackgroundService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5); // Check every 5 minutes
        private const int REMINDER_MINUTES_BEFORE = 30; // Send reminder 30 mins before

        public WorkoutNotificationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<WorkoutNotificationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        public async Task SendUpcomingWorkoutRemindersAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                var reminderWindow = now.AddMinutes(REMINDER_MINUTES_BEFORE);
                _serviceProvider.GetRequiredService<IUnitOfWork>();
                var _unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();
                _serviceProvider.GetRequiredService<INotificationService>();
                var _notificationService = _serviceProvider.GetRequiredService<INotificationService>();

                // Get all workouts scheduled in the next 30 minutes that haven't been reminded
                var upcomingWorkouts = (_unitOfWork.Workouts.GetAll()).Where(w =>
                    w.Date >= now &&
                    w.Date <= reminderWindow &&
                    !w.ReminderSent &&
                    !w.IsCompleted).ToList();
                foreach (var workout in upcomingWorkouts)
                {
                    try
                    {
                        var notificationDto = new NotificationCreationDTO
                        {
                            Title = $"🏋️ Workout Starting Soon",
                            Content = $"Your {workout.Name} workout starts in {REMINDER_MINUTES_BEFORE} minutes! Get ready to crush it! 💪"
                        };

                        await _notificationService.CreateNotification(workout.User.Id, notificationDto);

                        // Mark reminder as sent
                        workout.ReminderSent = true;
                        _unitOfWork.Workouts.Update(workout);

                        _logger.LogInformation($"Reminder sent for workout {workout.Id} to user {workout.User.Id}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error sending reminder for workout {workout.Id}: {ex.Message}");
                    }
                }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SendUpcomingWorkoutRemindersAsync: {ex.Message}");
            }
        }

        public async Task SendWorkoutStartedNotificationAsync(Guid workoutId, Guid userId)
        {
            _serviceProvider.GetRequiredService<IUnitOfWork>();
            var _unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();
            _serviceProvider.GetRequiredService<INotificationService>();
            var _notificationService = _serviceProvider.GetRequiredService<INotificationService>();
            try
            {
                var workout = await _unitOfWork.Workouts.GetById(workoutId);
                if (workout == null)
                {
                    _logger.LogWarning($"Workout {workoutId} not found");
                    return;
                }

                var notificationDto = new NotificationCreationDTO
                {
                    Title = "💪 Workout Started",
                    Content = $"You've started your {workout.Name} workout. Stay focused and give it your best effort!"
                };

                await _notificationService.CreateNotification(userId, notificationDto);
                _logger.LogInformation($"Workout started notification sent for workout {workoutId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending workout started notification: {ex.Message}");
            }
        }

        public async Task SendWorkoutCompletedNotificationAsync(Guid workoutId, Guid userId)
        {
            _serviceProvider.GetRequiredService<IUnitOfWork>();
            var _unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();
            _serviceProvider.GetRequiredService<INotificationService>();
            var _notificationService = _serviceProvider.GetRequiredService<INotificationService>();
            try
            {
                var workout = await _unitOfWork.Workouts.GetById(workoutId);
                if (workout == null)
                {
                    _logger.LogWarning($"Workout {workoutId} not found");
                    return;
                }

                var stats = await GetWorkoutStatsAsync(workoutId);

                var content = $"Amazing work! You completed {workout.Name}! 🎉\n" +
                    $"Duration: {stats.ActualDurationMinutes} mins | Exercises: {stats.ExercisesCompleted}/{stats.TotalExercises}";

                if (stats.PersonalRecordsAchieved > 0)
                {
                    content += $"\n🏆 {stats.PersonalRecordsAchieved} Personal Record(s) achieved!";
                }

                var notificationDto = new NotificationCreationDTO
                {
                    Title = "🎊 Workout Completed",
                    Content = content
                };

                await _notificationService.CreateNotification(userId, notificationDto);

                workout.NotificationSent = true;
                _unitOfWork.Workouts.Update(workout);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Workout completed notification sent for workout {workoutId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending workout completed notification: {ex.Message}");
            }
        }

        public async Task SendPersonalRecordNotificationsAsync()
        {
            _serviceProvider.GetRequiredService<IUnitOfWork>();
            var _unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();
            _serviceProvider.GetRequiredService<INotificationService>();
            var _notificationService = _serviceProvider.GetRequiredService<INotificationService>();
            try
            {
                // Get all unsent personal records
                var unsentPRs = (_unitOfWork.PersonalRecords.GetAll()).Where(pr =>
                    !pr.NotificationSent).ToList();

                foreach (var pr in unsentPRs)
                {
                    try
                    {
                        var exerciseInstance = pr.WorkoutSet?.ExerciseInstance;
                        if (exerciseInstance != null)
                        {
                            await SendPRNotificationAsync(pr.UserId, pr, exerciseInstance);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error sending PR notification for {pr.Id}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SendPersonalRecordNotificationsAsync: {ex.Message}");
            }
        }

        public async Task SendPRNotificationAsync(Guid userId, PersonalRecord pr, ExerciseInstance exerciseInstance)
        {
            _serviceProvider.GetRequiredService<IUnitOfWork>();
            var _unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();
            _serviceProvider.GetRequiredService<INotificationService>();
            var _notificationService = _serviceProvider.GetRequiredService<INotificationService>();
            try
            {
                var exercise = pr.Exercise;
                var weightStr = pr.Weight > 0 ? $"{pr.Weight}kg" : "Bodyweight";

                var notificationDto = new NotificationCreationDTO
                {
                    Title = "🏆 New Personal Record!",
                    Content = $"Congratulations! You achieved a new PR on {exercise?.Name}!\n" +
                        $"{pr.Reps} reps @ {weightStr}\n" +
                        $"Date: {pr.AchievedDate:MMM dd, yyyy}"
                };

                await _notificationService.CreateNotification(userId, notificationDto);

                pr.NotificationSent = true;
                _unitOfWork.PersonalRecords.Update(pr);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"PR notification sent for exercise {exercise?.Name} to user {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending PR notification: {ex.Message}");
            }
        }

        public async Task<WorkoutNotificationStatsDTO> GetWorkoutStatsAsync(Guid workoutId)
        {
            _serviceProvider.GetRequiredService<IUnitOfWork>();
            var _unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();
            _serviceProvider.GetRequiredService<INotificationService>();
            var _notificationService = _serviceProvider.GetRequiredService<INotificationService>();
            var workout = await _unitOfWork.Workouts.GetById(workoutId);
            if (workout == null)
                return new WorkoutNotificationStatsDTO();

            var exerciseInstances = (_unitOfWork.ExerciseInstance.GetAll()).Where(e => e.WorkoutId == workoutId).ToList();
            var completedExercises = exerciseInstances.Count(e => e.IsCompleted);

            var actualDuration = workout.ActualEndTime.HasValue && workout.ActualStartTime.HasValue
                ? (int)(workout.ActualEndTime.Value - workout.ActualStartTime.Value).TotalMinutes
                : 0;

            // Get PRs from this workout
            var prs = (_unitOfWork.PersonalRecords.GetAll()).Where(pr =>
                pr.WorkoutSet != null &&
                pr.WorkoutSet.ExerciseInstance != null &&
                pr.WorkoutSet.ExerciseInstance.WorkoutId == workoutId).ToList();

            return new WorkoutNotificationStatsDTO
            {
                TotalExercises = exerciseInstances.Count,
                ExercisesCompleted = completedExercises,
                ActualDurationMinutes = actualDuration,
                PersonalRecordsAchieved = prs.Count()
            };
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Workout Notification Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var workoutNotificationService = scope.ServiceProvider
                            .GetRequiredService<IWorkoutNotificationSink>();

                        // Send reminder notifications for upcoming workouts
                        await workoutNotificationService.SendUpcomingWorkoutRemindersAsync();

                        // Check for personal records to notify about
                        await workoutNotificationService.SendPersonalRecordNotificationsAsync();

                        _logger.LogInformation("Workout notification checks completed");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in workout notification background job");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Workout Notification Background Service stopped");
        }
    }
    public class WorkoutNotificationStatsDTO
    {
        public int TotalExercises { get; set; }
        public int ExercisesCompleted { get; set; }
        public int ActualDurationMinutes { get; set; }
        public int PersonalRecordsAchieved { get; set; }
    }
}
