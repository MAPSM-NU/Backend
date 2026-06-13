using Gym_App.Core;
using Gym_App.Infastructure.Interfaces.Repositries;

namespace Gym_App.Infrastructure.Interfaces.Repositries
{
    public interface IUserStatsRepositry : IBaseRepositry<UserStats>
    {
        public Task<bool> IsUserStatsExistByUserId(Guid userId);
        public Task<UserStats> GetUserStatsByUserName(string userName);
        public Task<UserStats> GetUserStatsByUserId(Guid userId);
        public Task<UserStatsDaily> GetUserStatsDaily(Guid userId, DateOnly date);
        public Task<UserStatsWeekly> GetUserStatsWeekly(Guid userId, int weekNumber, int year);
        public Task<UserStatsMonthly> GetUserStatsMonthly(Guid userId, string monthName, int year);
        public Task<UserStatsMonthly> GetUserStatsMonthly(Guid userId, int monthNumber, int year);
        public Task<List<UserStatsDaily>> GetUserStatsDaysList(Guid userId, DateOnly startDate, DateOnly endDate);
        public Task<List<UserStatsWeekly>> GetUserStatsWeeksList(Guid userId, List<int> weekNumbers, int year);
        public Task<List<UserStatsMonthly>> GteUserStatsMonthsList(Guid userId, List<string> monthNames, int year);
        public Task<List<UserStatsMonthly>> GteUserStatsMonthsList(Guid userId, List<int> monthNumbers, int year);
    }
}
