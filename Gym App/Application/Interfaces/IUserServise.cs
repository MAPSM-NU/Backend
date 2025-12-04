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
        Task<ResponseToken> SigninUser(string email, string password);
        Task<int> UpdateUser(UserUpdateDTO user);
        Task<int> ChangeUserType(UserChangeTypeDTO user);
        Task<UserViewDTO?> GetUserByID(Guid userID);
        Task<PagedList<UserSmallViewDTO>?> GetUsersByFilter(string startDate,string endDate,int page, string sortColumn, string OrderBy, string searchTerm, int pageSize = 5);
        Task<PagedList<UserViewDTO>?> GetAllUsers(int page, int pageSize);
        Task<bool> DeleteUser(Guid userID);
        //public Task<string> ModifyUser(Trainee user);
        //still under construction

    }
}
