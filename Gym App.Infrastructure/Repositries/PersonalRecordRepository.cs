using Gym_App.Domain;
using Gym_App.Infastructure.Interfaces.Repositries;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace Gym_App.Infastructure.Repositries
{
    public class PersonalRecordRepository : BaseRepositry<PersonalRecord>, IPersonalRecordRepository
    {
        private readonly DbContext _context;
        private readonly DbSet<PersonalRecord> table;

        public PersonalRecordRepository(DbContext context) : base(context)
        {
            _context = context;
            table = _context.Set<PersonalRecord>();
        }

        public async Task<IEnumerable<PersonalRecord>> GetUserPersonalRecordsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await table
                .Where(pr => pr.UserId == userId)
                .Include(pr => pr.Exercise)
                .OrderByDescending(pr => pr.AchievedDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<PersonalRecord?> GetUserExercisePRAsync(Guid userId, Guid exerciseId, CancellationToken cancellationToken = default)
        {
            return await table
                .Where(pr => pr.UserId == userId && pr.ExerciseId == exerciseId)
                .OrderByDescending(pr => pr.Weight)
                .ThenByDescending(pr => pr.Reps)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<PersonalRecord>> GetUnsentNotificationsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<PersonalRecord>()
                .Where(pr => !pr.NotificationSent)
                .Include(pr => pr.Exercise)
                .Include(pr => pr.User)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<PersonalRecord>> GetPRsByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await table
                .Where(pr => pr.UserId == userId &&
                           pr.AchievedDate >= startDate &&
                           pr.AchievedDate <= endDate)
                .Include(pr => pr.Exercise)
                .OrderByDescending(pr => pr.AchievedDate)
                .ToListAsync(cancellationToken);
        }
    }
}
