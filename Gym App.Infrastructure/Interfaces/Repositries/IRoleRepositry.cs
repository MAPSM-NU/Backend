using Gym_App.Domain;

namespace Gym_App.Infastructure.Interfaces.Repositries
{
    public interface IRoleRepositry : IBaseRepositry<Role>
    {
        public Task<Role> GetRoleById(Guid roleId, CancellationToken cancellationToken = default);
        public Task<Role> GetRoleByName(string roleName, CancellationToken cancellationToken = default);
        public Task<bool> IsRoleExist(Guid roleId, CancellationToken cancellationToken = default);
        public Task<bool> IsRoleNameExist(string roleName, CancellationToken cancellationToken = default);
    }
}
