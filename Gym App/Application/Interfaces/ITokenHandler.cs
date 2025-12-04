using Gym_App.Domain;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.DTOs.UserDTOs;

namespace Gym_App.Application.Interfaces
{
    public interface ITokenHandler
    {
        public Task<string> CreateAccessToken(Guid userID, string name, string email, string role);
        public Task<string> CreateRefreshToken(Guid UserID);
        public Task<string?> RefreshingToken(Guid UserID);
        public Task<ResponseToken?> ValidateAccessToken(string token);
        public Task<PagedList<RefreshTokens>> GetAllRefreshTokens(int page,int pageSize);
    }
}
