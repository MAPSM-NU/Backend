using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;

namespace Gym_App.Service.Functions.Interfaces
{
    public interface IMessageService
    {
        public Task<int> AddMessage(MessageDTO sessionMessages);
        public Task<int> DeleteMessages(MessageDTO sessionMessages);
        public Task<int> UpdateMessage(MessageDTO message);
        public Task<List<MessageDTO>> GetSessionMessages(Guid sessionID);
        public Task<List<MessageDTO>> GetMessages();
    }
}
