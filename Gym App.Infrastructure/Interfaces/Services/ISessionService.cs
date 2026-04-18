using Gym_App.Infastructure.DTOs.Message;
using Gym_App.Infastructure.DTOs.Session;
using Gym_App.Infastructure.DTOs.UserDTOs;
using Gym_App.Infastructure.Transfer_Classes;
using Gym_App.Infrastructure.DTOs.Session;
using System.Security.Claims;

namespace Gym_App.Infastructure.Interfaces.Services
{
    public interface ISessionService
    {
        Task<SettersResponse> CreateSession(SessionUsersDTO Users);
        Task<SettersResponse> DeleteSession(Guid sessionID);
        Task<SettersResponse> AddMessages(Guid sessionID,SessionMessagesDTO sessionMessages);
        Task<SettersResponse> DeleteMessages(Guid sessionID, SessionMessagesDTO sessionMessages);
        Task<List<Guid>?> GetSessionUsersIDs(Guid sessionID);
        Task<GettersResponse<MessageViewDTO>> GetSessionMessages(Guid sessionID, string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize);
        Task<GettersResponse<UserViewDTO>> GetUsersOfSession(Guid sessionID,int page,int pageSize);
        Task<GettersResponse<SessionViewDTO>> GetAllSessions(int page,int pagSize);
    }
}
