using Gym_App.Core;
using Gym_App.Domain;
using Gym_App.Infastructure.Transfer_Classes;
using Gym_App.Infrastructure.DTOs.UserStats;

namespace Gym_App.Infrastructure.Interfaces.Services
{
    public interface IUserStatsService
    {
        public Task<SettersResponse> AddDailyStats(Workout workout);
        public Task<SettersResponse> AddWeeklyStats(User user);
        public Task<SettersResponse> AddWeeklyStats(UserStatsDaily usd);
        public Task<SettersResponse> AddMonthlyStats(User user);
        public Task<SettersResponse> AddMonthlyStats(UserStatsWeekly usw);
        public Task<GettersResponse<UserStatsDTO>> GetOverallStats(Guid userId);
        public Task<GettersResponse<UserStatsDailyDTO>> GetDailyStats(Guid userId, DateOnly date);
        public Task<GettersResponse<UserStatsWeeklyDTO>> GetWeeklyStats(Guid userId, int weekNumber, int year);
        public Task<GettersResponse<UserStatsMonthlyDTO>> GetMonthlyStats(Guid userId, string monthName, int year);
    }
}
