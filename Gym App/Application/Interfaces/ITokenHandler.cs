using Gym_App.Domain;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.DTOs;

namespace Gym_App.Application.Interfaces
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
