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
    public class UserStatDailyRepositry : BaseRepositry<UserStatsDaily> , IUserStatDailyRepositry
    {
        private readonly DbBase _context;
        private readonly DbSet<UserStatsDaily> table;
        public UserStatDailyRepositry(DbBase context) : base(context)
        {
            _context = context;
            table = context.Set<UserStatsDaily>();
        }
        public async Task<UserStatsDaily> GetUserStatsDaily(Guid userId, DateOnly date)
        {
            return await table.FirstOrDefaultAsync(usd => usd.userId == userId && usd.date == date);
        }

        public async Task<List<UserStatsDaily>> GetUserStatsDaysList(Guid userId, DateOnly startDate, DateOnly endDate)
        {
            return await table.Where(usd => usd.userId == userId && usd.date >= startDate && usd.date <= endDate).ToListAsync();
        }
    }
}
