using Gym_App.Infrastructure.Interfaces.Repositries;

namespace Gym_App.Infastructure.Interfaces.Repositries;

/// <summary>
/// Represents a unit of work for managing repository operations as a single transaction
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    // Expose all repositories
    IUserRepositry Users { get; }
    ISessionRepositry Sessions { get; }
    IScheduleRepositry Schedules { get; }
    IWorkoutRepositry Workouts { get; }
    IMessageRepositry Messages { get; }
    INotificationRepositry Notifications { get; }
    IRoleRepositry Roles { get; }
    ITokenRepositry Tokens { get; }
    IExerciseRepositry Exercises { get; }
    IMuscleRepositry Muscles { get; }
    IFeedbackRepositry Feedbacks { get; }
    IPersonalRecordRepository PersonalRecords { get; }
    IWorkoutSetRepository WorkoutSet { get; }
    IExerciseInstanceRepository ExerciseInstance { get; }
    IPasswordResetTokenRepositry PasswordResetToken { get; }
    IUserStatsRepositry UserStats { get; } 

    /// <summary>
    /// Saves all changes made to repositories in a single database transaction
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back all pending changes
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a database transaction (optional - for advanced scenarios)
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
}
