using Gym_App.Core;
using Gym_App.Infastructure.Interfaces.Repositries;

namespace Gym_App.Infrastructure.Interfaces.Repositries
{
    public interface IUserStatsRepositry : IBaseRepositry<UserStats>
    {
        public Task<bool> IsUserStatsExistByUserId(Guid userId);
        public Task<UserStats> GetUserStatsByUserName(string userName);
        public Task<UserStats> GetUserStatsByUserId(string userId);
    }
}
