using Gym_App.Domain;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.Interfaces.Repositries;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace Gym_App.Infastructure.Repositries
{
    public class TokenRepositry : BaseRepositry<RefreshTokens>, ITokenRepositry
    {
        private readonly DbBase _db;
        private readonly DbSet<RefreshTokens> table;

        public TokenRepositry(DbBase db) : base(db)
        {
            _db = db;
            table = _db.Set<RefreshTokens>();
        }

        public async Task<RefreshTokens> GetRefreshTokenByUserId(Guid userId, CancellationToken cancellationToken = default)
        {
            return await table
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.UserID == userId, cancellationToken);
        }

        public async Task<RefreshTokens> GetRefreshTokenByToken(string token, CancellationToken cancellationToken = default)
        {
            return await table
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.RefreshToken == token, cancellationToken);
        }

        public async Task<bool> TokenExists(string token, CancellationToken cancellationToken = default)
        {
            return await table.AnyAsync(rt => rt.RefreshToken == token, cancellationToken);
        }

        public async Task<bool> UserHasRefreshToken(Guid userId, CancellationToken cancellationToken = default)
        {
            return await table.AnyAsync(rt => rt.UserID == userId, cancellationToken);
        }

        public async Task UpdateRefreshToken(Guid userId, string newToken, DateTime newExpiry, CancellationToken cancellationToken = default)
        {
            var refreshToken = await table.FirstOrDefaultAsync(rt => rt.UserID == userId, cancellationToken);
            if (refreshToken != null)
            {
                refreshToken.RefreshToken = newToken;
                refreshToken.Expires = newExpiry;
            }
        }

        public async Task RevokeRefreshToken(Guid userId, CancellationToken cancellationToken = default)
        {
            var refreshToken = await table.FirstOrDefaultAsync(rt => rt.UserID == userId, cancellationToken);
            if (refreshToken != null)
            {
                await Delete(refreshToken.Id, cancellationToken);
            }
        }

        public async Task<bool> IsTokenValid(string token, CancellationToken cancellationToken = default)
        {
            var refreshToken = await table.FirstOrDefaultAsync(rt => rt.RefreshToken == token, cancellationToken);
            return refreshToken != null && refreshToken.Expires > DateTime.UtcNow;
        }

        public async Task<bool> IsTokenExpired(string token, CancellationToken cancellationToken = default)
        {
            var refreshToken = await table.FirstOrDefaultAsync(rt => rt.RefreshToken == token, cancellationToken);
            return refreshToken == null || refreshToken.Expires <= DateTime.UtcNow;
        }

        public override IQueryable<RefreshTokens> FilterSortColumn(string columnName, string sortOrder, IQueryable<RefreshTokens> query)
        {
            return query.OrderByDescending(rt => rt.Expires);
        }

        public override IQueryable<RefreshTokens> Search(string searchTerm, IQueryable<RefreshTokens> query)
        {
            return query.Where(rt => rt.RefreshToken.Contains(searchTerm));
        }
    }
}
