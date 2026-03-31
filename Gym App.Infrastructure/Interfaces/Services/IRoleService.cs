using Gym_App.Domain;
using Gym_App.Infastructure.DTOs.UserDTOs;
using Gym_App.Infastructure.Transfer_Classes;

namespace Gym_App.Infastructure.Interfaces.Services
{
    public interface IRoleService
    {
        public Task<SettersResponse> createRole(string roleName);
        public Task<SettersResponse> updateRole(Guid roleId, string roleName);
        public Task<SettersResponse> deleteRole(Guid roleId);
        public Task<GettersResponse<UserMiniViewDTO>> getUsersOfRole(Guid roleId,string roleName);
        public Task<GettersResponse<Role>> getRoles();

    }
}
