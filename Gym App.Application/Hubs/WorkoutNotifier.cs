using Gym_App.Application.Services;
using Gym_App.Domain;
using Gym_App.Infastructure.DTOs.Notification;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Gym_App.Application.Hubs
{
    public interface IWorkoutNotificationSink
    {
        Task SendUpcomingWorkoutRemindersAsync();
        Task SendWorkoutStartedNotificationAsync(Guid workoutId, Guid userId);
        Task SendWorkoutCompletedNotificationAsync(Guid workoutId, Guid userId);
        Task SendPersonalRecordNotificationsAsync();
        Task SendPRNotificationAsync(Guid userId, PersonalRecord pr, ExerciseInstance exerciseInstance);
        ValueTask PushAsync(PersonalRecord pr);
        Task<WorkoutNotificationStatsDTO> GetWorkoutStatsAsync(Guid workoutId);
    }
    public class WorkoutNotifier : BackgroundService , IWorkoutNotificationSink
    {
        private readonly string[] startOfWorkout = { "Your workout is starting soon", "Get ready for your workout", "something is getting closer and it is not our abysmal ending as a race. It is your workout!!",
            "Time to lock in, your workout is nearing ", "If it was a bad day, it's time to release steam and if it wasn't, it is still time to release steam you absolute unit",
            "Don't get it twisted, nothing is permenant so it is time to get up and do the work; workout nearing in","Time to pr the whole day, just reminding you of your workout","workout time, get ready",
            "It's Time","IT'S TIME","hey again, just reminding you of your workout","Progress takes time and discipline, you got it"};
        private readonly string[] wordsOfEncouregment = { "you got it", "yeah you are almost there", "stay focused","nothing means much as how you treat yourself, you got it","YOU GOT IT",
            "The road to Rome starts with a single step, you got it","The weakness you percieve in yourself is a hologram you made for yourself, unbind yourself. You got it",
            "Every step you take is a step towards your percieved happiness, keep moving","The weights are feathers in your hand, you got it","keep moving forward","Hopefully you pr today"};
        private readonly string[] workoutStarted = { "You workout has started", "Get to moving and pushing those weights", "Have a nice workout!","Workout is starting","will you hit a pr today mi lord?",
            "Get to movinggggg","Get your playlist ready, the workout has started" };
        private readonly string[] workoutEnded = { "Hopefully you appreciate this workout as much as I do","Well done today fella, see you later","Great workout",
            "What a clean workout, or maybe not I am just an automated response, so you get to decide","Large leap of progress indeed","Great work out there","You slayed" };
        private readonly Random random = new Random();
        private readonly IServiceScopeFactory _serviceProvider;
        private readonly ILogger<WorkoutNotifier> _logger;
        private readonly Channel<PersonalRecord> _channel;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(1); // Check every 5 minutes
        private const int REMINDER_MINUTES_BEFORE = 30; // Send reminder 30 mins before

        public WorkoutNotifier(
            IServiceScopeFactory serviceProvider,
            ILogger<WorkoutNotifier> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _channel = Channel.CreateUnbounded<PersonalRecord>();
        }
        public ValueTask PushAsync(PersonalRecord pr) => _channel.Writer.WriteAsync(pr);
        public async Task SendUpcomingWorkoutRemindersAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                var reminderWindow = now.AddMinutes(REMINDER_MINUTES_BEFORE);
                await using var scope = _serviceProvider.CreateAsyncScope();
                var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var _notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                // Get all workouts scheduled in the next 30 minutes that haven't been reminded
                var upcomingWorkouts = _unitOfWork.Workouts.GetAll().Where(w =>
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
                            Title = startOfWorkout[random.Next(startOfWorkout.Length)],
                            Content = $"Your {workout.Name} workout starts in {REMINDER_MINUTES_BEFORE} minutes! {wordsOfEncouregment[random.Next(wordsOfEncouregment.Length)]}"
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
            await using var scope = _serviceProvider.CreateAsyncScope();
            var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var _notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
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
                    Title = workoutStarted[random.Next(workoutStarted.Length)],
                    Content = $"You've started your {workout.Name} workout. {wordsOfEncouregment[random.Next(wordsOfEncouregment.Length)]}"
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
            await using var scope = _serviceProvider.CreateAsyncScope();
            var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var _notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            try
            {
                var workout = await _unitOfWork.Workouts.GetById(workoutId);
                if (workout == null)
                {
                    _logger.LogWarning($"Workout {workoutId} not found");
                    return;
                }

                var stats = await GetWorkoutStatsAsync(workoutId);

                var content = $"{workoutEnded[random.Next(workoutEnded.Length)]}\n You completed {workout.Name}! 🎉\n" +
                    $"Duration: {stats.ActualDurationMinutes} mins | Exercises: {stats.ExercisesCompleted}/{stats.TotalExercises}";

                if (stats.PersonalRecordsAchieved > 0)
                {
                    content += $"\n {stats.PersonalRecordsAchieved} Personal Record(s) achieved!";
                }

                var notificationDto = new NotificationCreationDTO
                {
                    Title = workoutEnded[random.Next(workoutEnded.Length)],
                    Content = content
                };

                await _notificationService.CreateNotification(userId, notificationDto);

                workout.NotificationSent = true;
                await _unitOfWork.Workouts.Update(workout);
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
            
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var _notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                await foreach (var pr in _channel.Reader.ReadAllAsync())
                {
                    await _notificationService.CreateNotification(pr.UserId, new NotificationCreationDTO
                    {
                        Title = "New Personal Record!",
                        Content = $"You achieved a new PR on {pr.Exercise?.Name}!\n" +
                            $"{pr.Reps} reps @ {(pr.Weight > 0 ? $"{pr.Weight}kg" : "Bodyweight")}\n" +
                            $"Date: {pr.AchievedDate:MMM dd, yyyy}"
                    });
                    pr.NotificationSent = true;
                    _unitOfWork.PersonalRecords.Update(pr);
                    _logger.LogInformation($"Personal record notifications sent to {pr.UserId} on {pr.AchievedDate:MMM dd, yyyy} for exercise {pr.Exercise?.Name}");
                }
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SendPersonalRecordNotificationsAsync: {ex.Message}");
            }
        }

        public async Task SendPRNotificationAsync(Guid userId, PersonalRecord pr, ExerciseInstance exerciseInstance)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var _notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            try
            {
                var exercise = pr.Exercise;
                var weightStr = pr.Weight > 0 ? $"{pr.Weight}kg" : "Bodyweight";

                var notificationDto = new NotificationCreationDTO
                {
                    Title = "New Personal Record!",
                    Content = $"{wordsOfEncouregment[random.Next(workoutEnded.Length)]} You achieved a new PR on {exercise?.Name}!\n" +
                        $"{pr.Reps} reps @ {weightStr}\n" +
                        $"Date: {pr.AchievedDate:MMM dd, yyyy}"
                };

                await _notificationService.CreateNotification(userId, notificationDto);

                pr.NotificationSent = true;
                await _unitOfWork.PersonalRecords.Update(pr);
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
            await using var scope = _serviceProvider.CreateAsyncScope();
            var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var _notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var workout = await _unitOfWork.Workouts.GetById(workoutId);
            if (workout == null)
                return new WorkoutNotificationStatsDTO();

            var exerciseInstances = _unitOfWork.ExerciseInstance.GetAll().Where(e => e.WorkoutId == workoutId).ToList();
            var completedExercises = exerciseInstances.Count(e => e.IsCompleted);

            var actualDuration = workout.ActualEndTime.HasValue && workout.ActualStartTime.HasValue
                ? (int)(workout.ActualEndTime.Value - workout.ActualStartTime.Value).TotalMinutes
                : 0;

            // Get PRs from this workout
            var prs = _unitOfWork.PersonalRecords.GetAll().Where(pr =>
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
            _logger.LogInformation($"Workout Notifier started at {DateTime.Now}");
            var timer = new PeriodicTimer(_interval);
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
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
