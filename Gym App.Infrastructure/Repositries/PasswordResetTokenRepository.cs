using Gym_App.Core;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.Repositries;
using Gym_App.Infrastructure.Interfaces.Repositries;
using Microsoft.EntityFrameworkCore;
using System.Threading;

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
        public async Task<PasswordResetToken> GetTokenByUserEmail(string email, CancellationToken cancellationToken = default)
        {
            return await table.Include(t => t.User).FirstOrDefaultAsync(t => t.Email == email, cancellationToken);
        }

        public async Task<PasswordResetToken> GetTokenByUserId(Guid userId, CancellationToken cancellationToken = default)
        {
            return await table.Include(t => t.User).FirstOrDefaultAsync(t => t.userId == userId, cancellationToken);
        }

        public async Task<bool> isTokenUsed(string email, CancellationToken cancellationToken = default)
        {
            return await table.FirstOrDefaultAsync(t => t.Email == email, cancellationToken) is PasswordResetToken token && token.isUsed;
        }
    }
}
