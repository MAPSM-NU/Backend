using Gym_App.Core;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.Repositries;
using Gym_App.Infrastructure.Interfaces.Repositries;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym_App.Infrastructure.Repositries
{
    public class UserStatMonthlyRepositry : BaseRepositry<UserStatsMonthly> , IUserStatMonthlyRepositry
    {
        private readonly DbBase _context;
        private readonly DbSet<UserStatsMonthly> table;
        public UserStatMonthlyRepositry(DbBase context) : base(context)
        {
            _context = context;
            table = context.Set<UserStatsMonthly>();
        }
        public async Task<UserStatsMonthly> GetUserStatsMonthly(Guid userId, string monthName, int year)
        {
            return await table.FirstOrDefaultAsync(usd => usd.userId == userId && usd.monthName == monthName && usd.year == year);
        }

        public async Task<UserStatsMonthly> GetUserStatsMonthly(Guid userId, int monthNumber, int year)
        {
            return await table.FirstOrDefaultAsync(usd => usd.userId == userId && usd.monthNumber == monthNumber && usd.year == year);
        }

        public async Task<List<UserStatsMonthly>> GteUserStatsMonthsList(Guid userId, List<string> monthNames, int year)
        {
            return await table.Where(usd => usd.userId == userId && monthNames.Contains(usd.monthName) && usd.year == year).ToListAsync();
        }

        public async Task<List<UserStatsMonthly>> GteUserStatsMonthsList(Guid userId, List<int> monthNumbers, int year)
        {
            return await table.Where(usd => usd.userId == userId && monthNumbers.Contains(usd.monthNumber) && usd.year == year).ToListAsync();
        }
    }
}
