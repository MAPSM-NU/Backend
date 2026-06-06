using Gym_App.Domain;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.Interfaces.Repositries;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace Gym_App.Infastructure.Repositries
{
    public class RoleRepositry : BaseRepositry<Role>, IRoleRepositry
    {
        private readonly DbBase _db;
        private readonly DbSet<Role> table;
        public RoleRepositry(DbBase db) : base(db)
        {
            _db = db;
            table = _db.Set<Role>();
        }
        public async Task<Role> GetRoleById(Guid roleId, CancellationToken cancellationToken = default)
        {
            return await table.FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);
        }

        public async Task<Role> GetRoleByName(string roleName, CancellationToken cancellationToken = default)
        {
            return await table.FirstOrDefaultAsync(r => r.RoleName == roleName, cancellationToken);
        }

        public async Task<bool> IsRoleExist(Guid roleId, CancellationToken cancellationToken = default)
        {
            return await table.AnyAsync(r => r.Id == roleId, cancellationToken);
        }

        public async Task<bool> IsRoleNameExist(string roleName, CancellationToken cancellationToken = default)
        {
            return await table.AnyAsync(r => r.RoleName == roleName, cancellationToken);
        }
    }
}
