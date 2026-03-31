using Gym_App.Domain;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.Interfaces.Repositries;
using Microsoft.EntityFrameworkCore;

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

        public async Task<RefreshTokens> GetRefreshTokenByUserId(Guid userId)
        {
            return await table
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Id == userId);
        }

        public async Task<RefreshTokens> GetRefreshTokenByToken(string token)
        {
            return await table
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.RefreshToken == token);
        }

        public async Task<bool> TokenExists(string token)
        {
            return await table.AnyAsync(rt => rt.RefreshToken == token);
        }

        public async Task<bool> UserHasRefreshToken(Guid userId)
        {
            return await table.AnyAsync(rt => rt.Id == userId);
        }

        public async Task UpdateRefreshToken(Guid userId, string newToken, DateTime newExpiry)
        {
            var refreshToken = await table.FirstOrDefaultAsync(rt => rt.Id == userId);
            if (refreshToken != null)
            {
                refreshToken.RefreshToken = newToken;
                refreshToken.Expires = newExpiry;
            }
        }

        public async Task RevokeRefreshToken(Guid userId)
        {
            var refreshToken = await table.FirstOrDefaultAsync(rt => rt.Id == userId);
            if (refreshToken != null)
            {
                await Delete(refreshToken.Id);
            }
        }

        public async Task<bool> IsTokenValid(string token)
        {
            var refreshToken = await table.FirstOrDefaultAsync(rt => rt.RefreshToken == token);
            return refreshToken != null && refreshToken.Expires > DateTime.UtcNow;
        }

        public async Task<bool> IsTokenExpired(string token)
        {
            var refreshToken = await table.FirstOrDefaultAsync(rt => rt.RefreshToken == token);
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
