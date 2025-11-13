using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;

namespace Gym_App.Service.Functions.Interfaces
{
    public interface IMessageService
    {
        public Task<int> AddMessage(MessageDTO sessionMessages);
        public Task<int> DeleteMessage(Guid messageID);
        public Task<int> UpdateMessage(MessageDTO message);
        public Task<Guid> GetMessageUserID(Guid messageID);
        public Task<List<Guid>> GetSessionUsersIDs(Guid sessionID);
        public Task<PagedList<MessageDTO>> GetSessionMessages(Guid sessionID, string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize);
        public Task<PagedList<MessageDTO>> GetMessagesByFilter(string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize);
        public Task<PagedList<MessageDTO>> GetMessages(int page, int pageSize);
    }
}
