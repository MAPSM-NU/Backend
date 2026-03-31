using Gym_App.Domain;

namespace Gym_App.Infastructure.Interfaces.Repositries
{
    public interface IRoleRepositry : IBaseRepositry<Role>
    {
        public Task<Role> GetRoleById(Guid roleId);
        public Task<Role> GetRoleByName(string roleName);
        public Task<bool> IsRoleExist(Guid roleId);
        public Task<bool> IsRoleNameExist(string roleName);
    }
}
