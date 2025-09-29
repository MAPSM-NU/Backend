using Gym_App.Domain.DTOs;

namespace Gym_App.Service.Functions.Interfaces
{
    public interface IUserF
    {
        public Task<bool> AddUser(UserDTO user);
        public Task<bool> LoginUser(UserDTO user);
        public Task<IQueryable<UserDTO>> GetAllUsers();
        //public Task<bool> LogoutUser(UserDTO user);
        //public Task<string> ModifyName(UserDTO user);
        //public Task<string> ModifyPassword(UserDTO user);
        //public Task<string> AddProfilePic(UserDTO user);
        //public Task<string> ModifyBio(UserDTO user);
        //still under construction

    }
}
