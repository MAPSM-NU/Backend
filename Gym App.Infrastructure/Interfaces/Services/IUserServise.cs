using Gym_App.Domain.Entities;
using Gym_App.Infastructure.DTOs.UserDTOs;
using Gym_App.Infastructure.Transfer_Classes;
using Microsoft.AspNetCore.Http;
using System.Threading;

namespace Gym_App.Infastructure.Interfaces.Services
{
    public interface IUserServise
    {
        Task<ResponseToken> CreateAdmin(UserCreationDTO user, CancellationToken cancellationToken = default);
        Task<ResponseToken> SignUpUser(UserCreationDTO user, CancellationToken cancellationToken = default);
        Task<ResponseToken> SigninUser(string email, string password, CancellationToken cancellationToken = default);
        Task<SettersResponse> ForgotPassword(string email, CancellationToken cancellationToken = default);
        Task<SettersResponse> ResetPassword(string email, string otp, string newPassword, CancellationToken cancellationToken = default);
        Task<SettersResponse> ChangePfp(Guid userID, IFormFile pfp, CancellationToken cancellationToken = default);
        Task<SettersResponse> DeletePfp(Guid userId, CancellationToken cancellationToken = default);
        Task<SettersResponse> UpdateUser(UserUpdateDTO user, CancellationToken cancellationToken = default);
        Task<SettersResponse> ChangeUserType(UserChangeTypeDTO user, CancellationToken cancellationToken = default);
        Task<SettersResponse> DeleteUser(Guid userID, CancellationToken cancellationToken = default);
        Task<GettersResponse<UserViewDTO>> GetUserByID(Guid userID, CancellationToken cancellationToken = default);
        Task<GettersResponse<UserMiniViewDTO>> GetMiniUsers(string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize = 5, CancellationToken cancellationToken = default);
        Task<GettersResponse<UserViewDTO>> GetUsers(string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize = 5, CancellationToken cancellationToken = default);
        Task<GettersResponse<UserViewDTO>> GetAllUsers(int page, int pageSize, CancellationToken cancellationToken = default);
    }
}
