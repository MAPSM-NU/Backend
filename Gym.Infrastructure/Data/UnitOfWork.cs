namespace Gym.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly DbBase _context;
    private IUserRepository? _userRepository;
    private ISessionRepository? _sessionRepository;
    private IScheduleRepository? _scheduleRepository;
    private IWorkoutRepository? _workoutRepository;
    private IMessageRepository? _messageRepository;
    private INotificationRepository? _notificationRepository;

    public UnitOfWork(DbBase context)
    {
        _context = context;
    }

    public IUserRepository Users => _userRepository ??= new UserRepository(_context);
    public ISessionRepository Sessions => _sessionRepository ??= new SessionRepository(_context);
    public IScheduleRepository Schedules => _scheduleRepository ??= new ScheduleRepository(_context);
    public IWorkoutRepository Workouts => _workoutRepository ??= new WorkoutRepository(_context);
    public IMessageRepository Messages => _messageRepository ??= new MessageRepository(_context);
    public INotificationRepository Notifications => _notificationRepository ??= new NotificationRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            // Log and transform to domain exception
            throw new PersistenceException("Failed to save changes", ex);
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.RollbackTransactionAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}