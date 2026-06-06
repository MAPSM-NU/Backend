using Gym_App.Domain;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.Interfaces.Repositries;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Threading;

namespace Gym_App.Infastructure.Repositries
{
    public class ScheduleRepositry : BaseRepositry<Schedule>, IScheduleRepositry
    {
        private readonly DbBase _db;
        private readonly DbSet<Schedule> table;
        public ScheduleRepositry(DbBase db) : base(db)
        {
            _db = db;
            table = _db.Set<Schedule>();
        }
        public async Task<bool> DeleteUserSchedules(Guid userId, CancellationToken cancellationToken = default)
        {
            var schedules = await table.Where(s => s.User.Id == userId).ToListAsync(cancellationToken);
            if (schedules.Count == 0)
                return false;

            table.RemoveRange(schedules);
            return true;
        }

        public async Task<Schedule> GetScheduleById(Guid schedId, CancellationToken cancellationToken = default)
        {
            return await table.Include(s => s.User).Include(s => s.Workouts).Where(s => s.Id == schedId).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<ICollection<Schedule>> GetUserSchedules(Guid userId, CancellationToken cancellationToken = default)
        {
            return await table.Where(s => s.User.Id == userId).ToListAsync(cancellationToken);
        }

        public async Task<int> GetUserSchedulesCount(Guid userId, CancellationToken cancellationToken = default)
        {
            return await table.CountAsync(s => s.User.Id == userId, cancellationToken);
        }

        public IQueryable<Schedule> GetUserSchedulesQueryable(Guid userId)
        {
            return table.Where(s => s.User.Id == userId).AsNoTracking();
        }

        public async Task<bool> isScheduleExist(Guid schedId, CancellationToken cancellationToken = default)
        {
            return await table.AnyAsync(s => s.Id == schedId, cancellationToken);
        }

        public async Task<bool> isUserHasSchedules(Guid userId, CancellationToken cancellationToken = default)
        {
            return await table.AnyAsync(s => s.User.Id == userId, cancellationToken);
        }
        public override IQueryable<Schedule> Search(string searchTerm, IQueryable<Schedule> query)
        {
            return query.Where(s => s.Name.Contains(searchTerm));
        }
        public override IQueryable<Schedule> FilterSortColumn(string columnName, string sortOrder, IQueryable<Schedule> query)
        {
            Expression<Func<Schedule, object>> keySelector = columnName.ToLower() switch
            {
                "date" or "d" => Schedule => Schedule.CreatedAt, // order by date
                "name" or "n" => Schedule => Schedule.Name, // order by name
                "type" or "t" => Schedule => Schedule.Type, // order by type
                _ => Schedule => Schedule.Id //failsafe: order by Id
            };
            var orderLower = (sortOrder ?? string.Empty).ToLowerInvariant();
            bool descending = orderLower == "desc" || orderLower == "descending" || orderLower == "descend" || orderLower == "d";

            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }
    }
}
