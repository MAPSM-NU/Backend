using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.DTOs.UserDTOs;
using Gym_App.Infastructure.Transfer_Classes;

namespace Gym_App.Application.Interfaces
{
    public interface IUserServise
    {
        Task<ResponseToken> CreateAdmin(UserCreationDTO user);   
        Task<ResponseToken> SignUpUser(UserCreationDTO user);
        Task<ResponseToken> SigninUser(string email, string password);
        Task<SettersResponse> UpdateUser(UserUpdateDTO user);
        Task<SettersResponse> ChangeUserType(UserChangeTypeDTO user);
        Task<SettersResponse> DeleteUser(Guid userID);
        Task<UserViewDTO?> GetUserByID(Guid userID);
        Task<PagedList<UserMiniViewDTO>?> GetMiniUsers(string startDate,string endDate,int page, string sortColumn, string OrderBy, string searchTerm, int pageSize = 5);
        Task<PagedList<UserViewDTO>?> GetUsers(string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize = 5);
        Task<PagedList<UserViewDTO>?> GetAllUsers(int page, int pageSize);

    }
}
