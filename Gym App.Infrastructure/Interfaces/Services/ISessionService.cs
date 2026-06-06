using Gym_App.Infastructure.DTOs.Message;
using Gym_App.Infastructure.DTOs.Session;
using Gym_App.Infastructure.DTOs.UserDTOs;
using Gym_App.Infastructure.Transfer_Classes;
using Gym_App.Infrastructure.DTOs.Session;
using System.Security.Claims;
using System.Threading;

namespace Gym_App.Infastructure.Interfaces.Services
{
    public interface ISessionService
    {
        Task<SettersResponse> CreateSession(SessionUsersDTO Users, CancellationToken cancellationToken = default);
        Task<SettersResponse> DeleteSession(Guid sessionID, CancellationToken cancellationToken = default);
        Task<SettersResponse> AddMessages(Guid sessionID, SessionMessagesDTO sessionMessages, CancellationToken cancellationToken = default);
        Task<SettersResponse> DeleteMessages(Guid sessionID, SessionMessagesDTO sessionMessages, CancellationToken cancellationToken = default);
        Task<List<Guid>?> GetSessionUsersIDs(Guid sessionID, CancellationToken cancellationToken = default);
        Task<GettersResponse<SessionViewDTO>> GetSession(Guid sessionID, int page = 1, int pageSize = 5, CancellationToken cancellationToken = default);
        Task<GettersResponse<MessageViewDTO>> GetSessionMessages(Guid sessionID, string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize, CancellationToken cancellationToken = default);
        Task<GettersResponse<UserViewDTO>> GetUsersOfSession(Guid sessionID, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<GettersResponse<SessionViewDTO>> GetAllSessions(int page, int pagSize, CancellationToken cancellationToken = default);
    }
}
