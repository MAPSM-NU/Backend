

using Gym_App.Core;
using Gym_App.Infastructure.Interfaces.Repositries;

namespace Gym_App.Infrastructure.Interfaces.Repositries
{
    public interface IUserStatMonthlyRepositry : IBaseRepositry<UserStatsMonthly>
    {
        public Task<UserStatsMonthly> GetUserStatsMonthly(Guid userId, string monthName, int year);
        public Task<UserStatsMonthly> GetUserStatsMonthly(Guid userId, int monthNumber, int year);
        public Task<List<UserStatsMonthly>> GteUserStatsMonthsList(Guid userId, List<string> monthNames, int year);
        public Task<List<UserStatsMonthly>> GteUserStatsMonthsList(Guid userId, List<int> monthNumbers, int year);
    }
}
