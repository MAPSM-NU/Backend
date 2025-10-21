using DocumentFormat.OpenXml.Bibliography;
using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;

namespace Gym_App.Service.Functions.Interfaces
{
    public interface IUserServise
    {
        Task<ResponseToken> SignUpUser(UserDTO user);
        Task<ResponseToken> LoginUser(UserDTO user);
        Task<int> UpdateUser(UserUpdateDTO user);
        Task<int> ChangeUserType(UserTypeDTO user);
        Task<UserDTO?> GetUserByID(Guid userID);
        Task<PagedList<UserDTO>?> GetUsersByFilter(string startDate,string endDate,int page, string sortColumn, string OrderBy, string searchTerm, int pageSize = 5);
        Task<PagedList<UserDTO>?> GetAllUsers(int page, int pageSize);
        Task<bool> DeleteUser(Guid userID);
        //public Task<string> ModifyUser(Trainee user);
        //still under construction

    }
}
