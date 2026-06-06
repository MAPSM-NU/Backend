using Gym_App.Domain;
using Gym_App.Infastructure.DTOs.UserDTOs;
using Gym_App.Infastructure.Transfer_Classes;
using System.Threading;

namespace Gym_App.Infastructure.Interfaces.Services
{
    public interface IRoleService
    {
        public Task<SettersResponse> createRole(string roleName, CancellationToken cancellationToken = default);
        public Task<SettersResponse> updateRole(Guid roleId, string roleName, CancellationToken cancellationToken = default);
        public Task<SettersResponse> deleteRole(Guid roleId, CancellationToken cancellationToken = default);
        public Task<GettersResponse<UserMiniViewDTO>> getUsersOfRole(Guid roleId, string roleName, CancellationToken cancellationToken = default);
        public Task<GettersResponse<Role>> getRoles(CancellationToken cancellationToken = default);
    }
}
