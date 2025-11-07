
using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;

namespace Gym_App.Service.Functions.Interfaces
{
    public interface ITokenHandler
    {
        public Task<string> CreateAccessToken(UserDTO u);
        public Task<string> CreateRefreshToken(Guid UserID);
        public Task<string> RefreshingToken(Guid UserID);
        public Task<ResponseToken>? ValidateAccessToken(string token);
        public Task<PagedList<RefreshTokens>> GetAllRefreshTokens(int page,int pageSize);
    }
}
