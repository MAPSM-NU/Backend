using Gym_App.Domain;

namespace Gym_App.Infastructure.Interfaces.Repositries
{
    public interface ITokenRepositry : IBaseRepositry<RefreshTokens>
    {
        // Retrieval methods
        Task<RefreshTokens> GetRefreshTokenByUserId(Guid userId, CancellationToken cancellationToken = default);
        Task<RefreshTokens> GetRefreshTokenByToken(string token, CancellationToken cancellationToken = default);
        Task<bool> TokenExists(string token, CancellationToken cancellationToken = default);
        Task<bool> UserHasRefreshToken(Guid userId, CancellationToken cancellationToken = default);

        // Update methods
        Task UpdateRefreshToken(Guid userId, string newToken, DateTime newExpiry, CancellationToken cancellationToken = default);
        Task RevokeRefreshToken(Guid userId, CancellationToken cancellationToken = default);

        // Validation
        Task<bool> IsTokenValid(string token, CancellationToken cancellationToken = default);
        Task<bool> IsTokenExpired(string token, CancellationToken cancellationToken = default);
    }
}
