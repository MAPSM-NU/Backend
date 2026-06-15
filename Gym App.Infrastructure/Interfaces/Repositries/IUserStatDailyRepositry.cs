using Gym_App.Core;
using Gym_App.Infastructure.Interfaces.Repositries;

namespace Gym_App.Infrastructure.Interfaces.Repositries
{
    public interface IUserStatDailyRepositry : IBaseRepositry<UserStatsDaily>
    {
        public Task<UserStatsDaily> GetUserStatsDaily(Guid userId, DateOnly date);
        public Task<List<UserStatsDaily>> GetUserStatsDaysList(Guid userId, DateOnly startDate, DateOnly endDate);
    }
}
