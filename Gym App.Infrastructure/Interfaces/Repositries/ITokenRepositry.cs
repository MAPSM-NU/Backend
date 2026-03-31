using Gym_App.Domain;

namespace Gym_App.Infastructure.Interfaces.Repositries
{
    public interface ITokenRepositry : IBaseRepositry<RefreshTokens>
    {
        // Retrieval methods
        Task<RefreshTokens> GetRefreshTokenByUserId(Guid userId);
        Task<RefreshTokens> GetRefreshTokenByToken(string token);
        Task<bool> TokenExists(string token);
        Task<bool> UserHasRefreshToken(Guid userId);
        
        // Update methods
        Task UpdateRefreshToken(Guid userId, string newToken, DateTime newExpiry);
        Task RevokeRefreshToken(Guid userId);
        
        // Validation
        Task<bool> IsTokenValid(string token);
        Task<bool> IsTokenExpired(string token);
    }
}
