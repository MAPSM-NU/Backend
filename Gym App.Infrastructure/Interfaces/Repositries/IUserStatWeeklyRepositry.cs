using Gym_App.Core;
using Gym_App.Infastructure.Interfaces.Repositries;

namespace Gym_App.Infrastructure.Interfaces.Repositries
{
    public interface IUserStatWeeklyRepositry : IBaseRepositry<UserStatsWeekly>
    {
        public Task<UserStatsWeekly> GetUserStatsWeekly(Guid userId, int weekNumber, int year);
        public Task<List<UserStatsWeekly>> GetUserStatsWeeksList(Guid userId, List<int> weekNumbers, int year);
    }
}
