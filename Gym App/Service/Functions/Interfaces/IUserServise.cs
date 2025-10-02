using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Entities.Users;

namespace Gym_App.Service.Functions.Interfaces
{
    public interface IUserServise
    {
        public Task<ResponseToken> SignUpUser(UserDTO user);
        public Task<ResponseToken> LoginUser(UserDTO user);
        public Task<IQueryable<User>> GetAllUsers();
        public Task<bool> DeleteUser(Guid userID);
        //public Task<string> ModifyUser(Trainee user);
        //still under construction

    }
}
