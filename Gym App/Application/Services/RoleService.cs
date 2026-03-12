using Gym_App.Domain;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.DTOs.UserDTOs;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infastructure.Transfer_Classes;

namespace Gym_App.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepositry _roleRepositry;
        private readonly IUserRepositry _userRepositry;
        public RoleService(IRoleRepositry roleRepositry, IUserRepositry userRepositry) 
        {
            _roleRepositry = roleRepositry;
            _userRepositry = userRepositry;
        }
        public async Task<SettersResponse> createRole(string roleName)
        {
            await _roleRepositry.Create(new Role 
            { 
                RoleName = roleName,
                CreatedAt = DateTime.Now,
                Id = Guid.NewGuid(),
            });
            return new SettersResponse { msg = "Role created", status = 2 };
        }
        public async Task<SettersResponse> updateRole(Guid roleId, string roleName)
        {
            var role = await _roleRepositry.GetById(roleId);
            if(role == null) 
                return new SettersResponse { msg = "Role not found", status = 0 };
            role.RoleName = roleName;
            role.UpdatedAt = DateTime.Now;
            await _roleRepositry.Update(role);
            return new SettersResponse { msg = "Role updated", status = 2 };
        }

        public async Task<SettersResponse> deleteRole(Guid roleId)
        {
            var role = await _roleRepositry.GetById(roleId);
            if (role == null)
                return new SettersResponse { msg = "Role not found", status = 0 };
            await _roleRepositry.Delete(role);
            return new SettersResponse { msg = "Role deleted", status = 2 };
        }

        public async Task<GettersResponse<Role>> getRoles()
        {
            var roles = _roleRepositry.GetAll();
            var pagedRoles = await PagedList<Role>.CreateAsync(roles, 1, roles.Count());
            return new GettersResponse<Role>
            {
                Data = pagedRoles,
                msg = "Roles retrieved",
                status = 2
            };
        }

        public async Task<GettersResponse<UserMiniViewDTO>> getUsersOfRole(Guid roleId, string roleName)
        {
            bool isRoleExist = await _roleRepositry.IsRoleExist(roleId);
            if (!isRoleExist)
                return new GettersResponse<UserMiniViewDTO> { msg = "Role not found", status = 0 };

            var users = await _userRepositry.GetUsersByRoleAsQueryable(roleId);
            var userDTOs = users.Select(u => new UserMiniViewDTO
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email
            });
            var pagedUsers = await PagedList<UserMiniViewDTO>.CreateAsync(userDTOs, 1, userDTOs.Count());

            return new GettersResponse<UserMiniViewDTO>
            {
                Data = pagedUsers,
                msg = "Users retrieved",
                status = 2
            };
        }

    }
}
