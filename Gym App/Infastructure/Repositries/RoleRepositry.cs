using Gym_App.Domain;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.Interfaces.Repositries;
using Microsoft.EntityFrameworkCore;

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
        public async Task<Role> GetRoleById(Guid roleId)
        {
            return await table.FirstOrDefaultAsync(r => r.Id == roleId);
        }

        public async Task<Role> GetRoleByName(string roleName)
        {
            return await table.FirstOrDefaultAsync(r => r.RoleName == roleName);
        }

        public async Task<bool> IsRoleExist(Guid roleId)
        {
            return await table.AnyAsync(r => r.Id == roleId);   
        }

        public async Task<bool> IsRoleNameExist(string roleName)
        {
            return await table.AnyAsync(r => r.RoleName == roleName);
        }
    }
}
