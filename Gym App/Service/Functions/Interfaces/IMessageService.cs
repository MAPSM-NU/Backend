using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using System.Security.Claims;

namespace Gym_App.Service.Functions.Interfaces
{
    public interface IMessageService
    {
        public Task<int> AddMessage(ClaimsPrincipal User, MessageDTO sessionMessages);
        public Task<int> DeleteMessage(ClaimsPrincipal User, Guid messageID);
        public Task<int> UpdateMessage(ClaimsPrincipal User, MessageDTO message);
        public Task<Guid> GetMessageUserID(Guid messageID);
        public Task<List<Guid>?> GetSessionUsersIDs(ClaimsPrincipal User, Guid sessionID);
        public Task<PagedList<MessageDTO>?> GetSessionMessages(ClaimsPrincipal User, Guid sessionID, string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize);
        public Task<PagedList<MessageDTO>> GetMessagesByFilter(string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize);
        public Task<PagedList<MessageDTO>> GetMessages(int page, int pageSize);
    }
}
