using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;

namespace Gym_App.Service.Functions.Interfaces
{
    public interface IMessageService
    {
        public Task<int> AddMessage(MessageDTO sessionMessages);
        public Task<int> DeleteMessages(MessageDTO sessionMessages);
        public Task<int> UpdateMessage(MessageDTO message);
        public Task<PagedList<MessageDTO>> GetSessionMessages(Guid sessionID,int page,int pageSize);
        public Task<PagedList<MessageDTO>> GetMessagesByFilter(int page, string sortColumn, string OrderBy, string searchTerm, int pageSize);
        public Task<PagedList<MessageDTO>> GetMessages(int page, int pageSize);
    }
}
