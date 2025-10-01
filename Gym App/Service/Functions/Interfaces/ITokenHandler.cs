
using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;

namespace Gym_App.Service.Functions.Interfaces
{
    public interface ITokenHandler
    {
        public Task<string> CreateAccessToken(UserDTO u);
        public Task<string> CreateRefreshToken(Guid UserID);
        public Task<string> RefreshingToken(Guid UserID);
        public Task<ResponseToken>? ValidateAccessToken(string token);
        public Task<IQueryable<RefreshTokens>> GetAllRefreshTokens();
    }
}
