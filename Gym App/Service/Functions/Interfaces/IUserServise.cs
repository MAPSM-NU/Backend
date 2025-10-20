using DocumentFormat.OpenXml.Bibliography;
using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;

namespace Gym_App.Service.Functions.Interfaces
{
    public interface IUserServise
    {
        public Task<ResponseToken> SignUpUser(UserDTO user);
        public Task<ResponseToken> LoginUser(UserDTO user);
        public Task<int> UpdateUser(UserUpdateDTO user);
        public Task<int> ChangeUserType(UserTypeDTO user);
        public Task<UserDTO?> GetUserByID(Guid userID);
        public Task<PagedList<UserDTO>> GetUsersByFilter(int page, string sortColumn, string OrderBy, string searchTerm, int pageSize = 5);
        public Task<PagedList<UserDTO>?> GetAllUsers(int page, int pageSize);
        public Task<bool> DeleteUser(Guid userID);
        //public Task<string> ModifyUser(Trainee user);
        //still under construction

    }
}
