using Gym_App.Domain.Entities;
using Gym_App.Infastructure.DTOs.UserDTOs;
using Gym_App.Infastructure.Transfer_Classes;
using Microsoft.AspNetCore.Http;

namespace Gym_App.Infastructure.Interfaces.Services
{
    public interface IUserServise
    {
        Task<ResponseToken> CreateAdmin(UserCreationDTO user);   
        Task<ResponseToken> SignUpUser(UserCreationDTO user);
        Task<ResponseToken> SigninUser(string email, string password);
        Task<SettersResponse> ForgotPassword(string email);
        Task<SettersResponse> ResetPassword(string email,string otp, string newPassword);
        Task<SettersResponse> ChangePfp(Guid userID, IFormFile pfp);
        Task<SettersResponse> DeletePfp(Guid userId);
        Task<SettersResponse> UpdateUser(UserUpdateDTO user);
        Task<SettersResponse> ChangeUserType(UserChangeTypeDTO user);
        Task<SettersResponse> DeleteUser(Guid userID);
        Task<GettersResponse<UserViewDTO>> GetUserByID(Guid userID);
        Task<GettersResponse<UserMiniViewDTO>> GetMiniUsers(string startDate,string endDate,int page, string sortColumn, string OrderBy, string searchTerm, int pageSize = 5);
        Task<GettersResponse<UserViewDTO>> GetUsers(string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize = 5);
        Task<GettersResponse<UserViewDTO>> GetAllUsers(int page, int pageSize);
        
    }
}
