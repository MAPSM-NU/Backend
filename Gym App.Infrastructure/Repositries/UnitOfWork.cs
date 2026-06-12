using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infrastructure.Interfaces.Repositries;
using Gym_App.Infrastructure.Repositries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
namespace Gym_App.Infastructure.Repositries;

public class UnitOfWork : IUnitOfWork
{
    private readonly DbBase _context;
    private IDbContextTransaction? _transaction;

    // Lazy-loaded repositories
    private IUserRepositry? _userRepository;
    private ISessionRepositry? _sessionRepository;
    private IScheduleRepositry? _scheduleRepository;
    private IWorkoutRepositry? _workoutRepository;
    private IMessageRepositry? _messageRepository;
    private INotificationRepositry? _notificationRepository;
    private IRoleRepositry? _roleRepository;
    private ITokenRepositry? _tokenRepository;
    private IExerciseRepositry? _exerciseRepositry;
    private IMuscleRepositry? _muscleRepositry;
    private IFeedbackRepositry? _feedbackRepositry;
    private IPersonalRecordRepository? _personalRecordRepository;
    private IWorkoutSetRepository? _workoutSetRepository;
    private IExerciseInstanceRepository? _exerciseInstanceRepository;
    private IPasswordResetTokenRepositry? _passwordResetToken;
    private IUserStatsRepositry? _userStatsRepositry;

    public UnitOfWork(DbBase context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // Lazy-initialize repositories (only create when first accessed)
    public IUserRepositry Users
        => _userRepository ??= new UserRepositry(_context);

    public ISessionRepositry Sessions
        => _sessionRepository ??= new SessionRepositry(_context);

    public IScheduleRepositry Schedules
        => _scheduleRepository ??= new ScheduleRepositry(_context);

    public IWorkoutRepositry Workouts
        => _workoutRepository ??= new WorkoutRepositry(_context);

    public IMessageRepositry Messages
        => _messageRepository ??= new MessageRepositry(_context);

    public INotificationRepositry Notifications
        => _notificationRepository ??= new NotifiacationRepositry(_context);

    public IRoleRepositry Roles
        => _roleRepository ??= new RoleRepositry(_context);
    
    public ITokenRepositry Tokens
        => _tokenRepository ??= new TokenRepositry(_context);

    public IExerciseRepositry Exercises
        => _exerciseRepositry ??= new ExerciseRepositry(_context);

    public IMuscleRepositry Muscles
        => _muscleRepositry ??= new MuscleRepositry(_context);

    public IFeedbackRepositry Feedbacks
        => _feedbackRepositry ??= new FeedbackRepositry(_context);
    public IPersonalRecordRepository PersonalRecords
        => _personalRecordRepository ??= new PersonalRecordRepository(_context);
    public IWorkoutSetRepository WorkoutSet
        => _workoutSetRepository ??= new WorkoutSetRepository(_context);
    public IExerciseInstanceRepository ExerciseInstance
        => _exerciseInstanceRepository ??= new ExerciseInstanceRepository(_context);
    public IPasswordResetTokenRepositry PasswordResetToken
        => _passwordResetToken ??= new PasswordResetTokenRepository(_context);
    public IUserStatsRepositry UserStats
        => _userStatsRepositry ??= new UserStatsRepositry(_context);

    /// <summary>
    /// Saves all changes in a single database transaction
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            // Log the error (you can use ILogger here)
            Console.WriteLine($"Database error: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Rolls back all pending changes
    /// </summary>
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Discard all tracked changes
            foreach (var entry in _context.ChangeTracker.Entries().ToList())
            {
                entry.State = EntityState.Detached;
            }

            if (_transaction is not null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Rollback error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Begins a database transaction (for complex multi-operation scenarios)
    /// </summary>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// Commits the current transaction
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);

            if (_transaction is not null)
            {
                await _transaction.CommitAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        catch
        {
            await RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Cleanup resources
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
        }

        await _context.DisposeAsync();
    }
}
