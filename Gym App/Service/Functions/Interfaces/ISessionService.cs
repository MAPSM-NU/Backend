using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;

namespace Gym_App.Service.Functions.Interfaces
{
    public interface ISessionService
    {
        Task<int> CreateSession(SessionDTO session);
        Task<int> DeleteSession(Guid sessionID);
        Task<int> AddMessages(SessionMessagesDTO sessionMessages);
        Task<int> DeleteMessages(SessionMessagesDTO sessionMessages);
        Task<List<Guid>?> GetSessionUsersIDs(Guid sessionID);
        Task<PagedList<MessageDTO>?> GetSessionMessages(Guid sessionID, string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize);
        Task<PagedList<UserDTO>?> GetUsersOfSession(Guid sessionID,int page,int pageSize);
        Task<PagedList<SessionDTO>>? GetAllSessions(int page,int pagSize);
    }
}
