using DocumentFormat.OpenXml.Bibliography;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.DTOs.UserDTOs;

namespace Gym_App.Application.Interfaces
{
    public interface IUserServise
    {
        Task<ResponseToken> CreateAdmin(UserCreationDTO user);   
        Task<ResponseToken> SignUpUser(UserCreationDTO user);
        Task<ResponseToken> LoginUser(UserCreationDTO user);
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
