using Gym_App.Core;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.Repositries;
using Gym_App.Infrastructure.Interfaces.Repositries;
using Microsoft.EntityFrameworkCore;

namespace Gym_App.Infrastructure.Repositries
{
    public class PasswordResetTokenRepository : BaseRepositry<PasswordResetToken>, IPasswordResetTokenRepositry
    {
        private readonly DbBase _db;
        private readonly DbSet<PasswordResetToken> table;
        public PasswordResetTokenRepository(DbBase db) : base(db)
        {
            _db = db;
            table = _db.Set<PasswordResetToken>();
        }
        public async Task<PasswordResetToken> GetTokenByUserEmail(string email)
        {
            return await table.Include(t=>t.User).FirstOrDefaultAsync(t => t.Email == email);
        }

        public async Task<PasswordResetToken> GetTokenByUserId(Guid userId)
        {
            return await table.Include(t=>t.User).FirstOrDefaultAsync(t => t.userId == userId);
        }

        public async Task<bool> isTokenUsed(string email)
        {
            return await table.FirstOrDefaultAsync(t=>t.Email == email) is PasswordResetToken token && token.isUsed;    
        }
    }
}
