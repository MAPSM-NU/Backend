using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;

namespace Gym_App.Service.Functions.Interfaces
{
    public interface ISessionService
    {
        Task<int> CreateSession(SessionDTO session);
        Task<int> DeleteSession(Guid sessionID);
        Task<int> AddMessages(SessionMessagesDTO sessionMessages);
        Task<int> DeleteMessages(SessionMessagesDTO sessionMessages);
        Task<IQueryable<MessageDTO?>>? GetSessionMessages(Guid sessionID);
        Task<ICollection<User>>? GetUsersOfSession(Guid sessionID);
        Task<IQueryable<Session>>? GetAllSessions();
    }
}
