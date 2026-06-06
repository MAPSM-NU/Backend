using Gym_App.Domain;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using System.Threading;

namespace Gym_App.Infastructure.Interfaces.Services
{
    public interface ITokenHandler
    {
        public Task<string> CreateAccessToken(Guid userID, string name, string email, string role, CancellationToken cancellationToken = default);
        public Task<string> CreateRefreshToken(Guid UserID, CancellationToken cancellationToken = default);
        public Task<string?> RefreshingToken(Guid UserID, CancellationToken cancellationToken = default);
        public Task<ResponseToken?> ValidateRefreshToken(string token, CancellationToken cancellationToken = default);
        public Task<PagedList<RefreshTokens>> GetAllRefreshTokens(int page, int pageSize, CancellationToken cancellationToken = default);
    }
}
