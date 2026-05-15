using Gym_App.Domain;
using Gym_App.Infastructure.Interfaces.Repositries;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IEnumerable<PersonalRecord>> GetUserPersonalRecordsAsync(Guid userId)
        {
            return await table
                .Where(pr => pr.UserId == userId)
                .Include(pr => pr.Exercise)
                .OrderByDescending(pr => pr.AchievedDate)
                .ToListAsync();
        }

        public async Task<PersonalRecord?> GetUserExercisePRAsync(Guid userId, Guid exerciseId)
        {
            return await table
                .Where(pr => pr.UserId == userId && pr.ExerciseId == exerciseId)
                .OrderByDescending(pr => pr.Weight)
                .ThenByDescending(pr => pr.Reps)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PersonalRecord>> GetUnsentNotificationsAsync()
        {
            return await _context.Set<PersonalRecord>()
                .Where(pr => !pr.NotificationSent)
                .Include(pr => pr.Exercise)
                .Include(pr => pr.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<PersonalRecord>> GetPRsByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            return await table
                .Where(pr => pr.UserId == userId && 
                           pr.AchievedDate >= startDate && 
                           pr.AchievedDate <= endDate)
                .Include(pr => pr.Exercise)
                .OrderByDescending(pr => pr.AchievedDate)
                .ToListAsync();
        }
    }
}
