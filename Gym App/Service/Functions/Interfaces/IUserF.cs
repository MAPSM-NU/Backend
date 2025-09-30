using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Entities.Users;

namespace Gym_App.Service.Functions.Interfaces
{
    public interface IUserF
    {
        public Task<Response> SignUpUser(UserDTO user);
        public Task<Response> LoginUser(UserDTO user);
        public Task<IQueryable<UserDTO>> GetAllUsers();
        public Task<bool> DeleteUser(Guid userID);
        //public Task<string> ModifyUser(Trainee user);
        //still under construction

    }
}
