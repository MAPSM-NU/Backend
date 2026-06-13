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

        public async Task<UserStatsDaily> GetUserStatsDaily(Guid userId, DateOnly date)
        {
            return await tableDaily.FirstOrDefaultAsync(usd => usd.userId == userId && usd.date == date);
        }

        public async Task<List<UserStatsDaily>> GetUserStatsDaysList(Guid userId, DateOnly startDate, DateOnly endDate)
        {
            return await tableDaily.Where(usd=> usd.userId == userId && usd.date >= startDate && usd.date <= endDate).ToListAsync();
        }
        public async Task<UserStatsWeekly> GetUserStatsWeekly(Guid userId, int weekNumber, int year)
        {
            return await tableWeekly.FirstOrDefaultAsync(usw => usw.userId == userId && usw.weekNumber == weekNumber && usw.year == year);
        }

        public async Task<List<UserStatsWeekly>> GetUserStatsWeeksList(Guid userId, List<int> weekNumbers, int year)
        {
            return await tableWeekly.Where(usw => usw.userId == userId && weekNumbers.Contains(usw.weekNumber) && usw.year == year).ToListAsync();
        }
        public async Task<UserStatsMonthly> GetUserStatsMonthly(Guid userId, string monthName, int year)
        {
            return await tableMonthly.FirstOrDefaultAsync(usd => usd.userId == userId && usd.monthName == monthName && usd.year == year);
        }

        public async Task<UserStatsMonthly> GetUserStatsMonthly(Guid userId, int monthNumber, int year)
        {
            return await tableMonthly.FirstOrDefaultAsync(usd => usd.userId == userId && usd.monthNumber == monthNumber && usd.year == year);
        }

        public async Task<List<UserStatsMonthly>> GteUserStatsMonthsList(Guid userId, List<string> monthNames, int year)
        {
            return await tableMonthly.Where(usd => usd.userId == userId && monthNames.Contains(usd.monthName) &&  usd.year == year).ToListAsync();
        }

        public async Task<List<UserStatsMonthly>> GteUserStatsMonthsList(Guid userId, List<int> monthNumbers, int year)
        {
            return await tableMonthly.Where(usd => usd.userId == userId && monthNumbers.Contains(usd.monthNumber) && usd.year == year).ToListAsync();
        }

        public async Task<bool> IsUserStatsExistByUserId(Guid userId)
        {
            return await table.AnyAsync(u => u.userId == userId);
        }
    }
}
