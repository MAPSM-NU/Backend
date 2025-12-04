using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.DTOs;
using Gym_App.Infastructure.DTOs.UserDTOs;
using System.Security.Claims;

namespace Gym_App.Application.Interfaces
{
    public interface ISessionService
    {
        Task<int> CreateSession(ClaimsPrincipal User, SessionDTO session);
        Task<int> DeleteSession(ClaimsPrincipal User, Guid sessionID);
        Task<int> AddMessages(ClaimsPrincipal User, SessionMessagesDTO sessionMessages);
        Task<int> DeleteMessages(ClaimsPrincipal User, SessionMessagesDTO sessionMessages);
        Task<List<Guid>?> GetSessionUsersIDs(ClaimsPrincipal User, Guid sessionID);
        Task<PagedList<MessageDTO>?> GetSessionMessages(ClaimsPrincipal User, Guid sessionID, string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize);
        Task<PagedList<UserViewDTO>?> GetUsersOfSession(ClaimsPrincipal User, Guid sessionID,int page,int pageSize);
        Task<PagedList<SessionDTO>>? GetAllSessions(int page,int pagSize);
    }
}
