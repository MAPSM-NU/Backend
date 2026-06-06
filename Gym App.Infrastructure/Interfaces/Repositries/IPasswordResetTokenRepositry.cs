using Gym_App.Core;
using Gym_App.Infastructure.Interfaces.Repositries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym_App.Infrastructure.Interfaces.Repositries
{
    public interface IPasswordResetTokenRepositry : IBaseRepositry<PasswordResetToken>
    {
        public Task<bool> isTokenUsed(string email, CancellationToken cancellationToken = default);
        public Task<PasswordResetToken> GetTokenByUserId(Guid userId, CancellationToken cancellationToken = default);
        public Task<PasswordResetToken> GetTokenByUserEmail(string email, CancellationToken cancellationToken = default);
    }
}
