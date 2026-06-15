using Gym_App.Core;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.Repositries;
using Gym_App.Infrastructure.Interfaces.Repositries;
using Microsoft.EntityFrameworkCore;

namespace Gym_App.Infrastructure.Repositries
{
    public class UserStatWeeklyRepoistry : BaseRepositry<UserStatsWeekly>, IUserStatWeeklyRepositry
    {
        private readonly DbBase _context;
        private readonly DbSet<UserStatsWeekly> table;
        public UserStatWeeklyRepoistry(DbBase context) : base(context)
        {
            _context = context;
            table = context.Set<UserStatsWeekly>();
        }
        public async Task<UserStatsWeekly> GetUserStatsWeekly(Guid userId, int weekNumber, int year)
        {
            return await table.Include(usw => usw.userStatsDaily).FirstOrDefaultAsync(usw => usw.userId == userId && usw.weekNumber == weekNumber && usw.year == year);
        }

        public async Task<List<UserStatsWeekly>> GetUserStatsWeeksList(Guid userId, List<int> weekNumbers, int year)
        {
            return await table.Include(usw => usw.userStatsDaily).Where(usw => usw.userId == userId && weekNumbers.Contains(usw.weekNumber) && usw.year == year).ToListAsync();
        }
    }
}
