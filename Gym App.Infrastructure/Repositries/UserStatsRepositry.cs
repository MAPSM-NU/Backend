using Gym_App.Core;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.Repositries;
using Gym_App.Infrastructure.Interfaces.Repositries;
using Microsoft.EntityFrameworkCore;

namespace Gym_App.Infrastructure.Repositries
{
    internal class UserStatsRepositry : BaseRepositry<UserStats>, IUserStatsRepositry
    {
        private readonly DbBase _context;
        private readonly DbSet<UserStats> table;
        private readonly DbSet<UserStatsDaily> tableDaily;
        private readonly DbSet<UserStatsWeekly> tableWeekly;
        private readonly DbSet<UserStatsMonthly> tableMonthly;
        public UserStatsRepositry(DbBase context) : base(context)
        {
            _context = context;
            table = _context.Set<UserStats>();
            tableDaily = _context.Set<UserStatsDaily>();
            tableWeekly = _context.Set<UserStatsWeekly>();
            tableMonthly = _context.Set<UserStatsMonthly>();
        }
        public async Task<UserStats> GetUserStatsByUserId(Guid userId)
        {
            return await table.Include(u => u.user).FirstOrDefaultAsync(u => u.userId == userId)!;
        }

        public async Task<UserStats> GetUserStatsByUserName(string userName)
        {
            return await table.Include(u => u.user).FirstOrDefaultAsync(u => u.user.Name == userName)!;
        }

        public async Task<bool> IsUserStatsExistByUserId(Guid userId)
        {
            return await table.AnyAsync(u => u.userId == userId);
        }
    }
}
