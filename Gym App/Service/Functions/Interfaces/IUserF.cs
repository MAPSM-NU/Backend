using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities.Users;

namespace Gym_App.Service.Functions.Interfaces
{
    public interface IUserF
    {
        public Task<int> SignUpUser(UserDTO user);
        public Task<int> LoginUser(UserDTO user);
        public Task<IQueryable<UserDTO>> GetAllUsers();
        public Task<bool> DeleteUser(Guid userID);
        //public Task<string> ModifyUser(Trainee user);
        //still under construction

    }
}
