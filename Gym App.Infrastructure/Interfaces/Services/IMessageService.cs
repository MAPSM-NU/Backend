using Gym_App.Infastructure.DTOs.Message;
using Gym_App.Infastructure.Transfer_Classes;
using System.Threading;

namespace Gym_App.Infastructure.Interfaces.Services
{
    public interface IMessageService
    {
        public Task<SettersResponse> AddMessage(Guid senderID, MessageCreationDTO sessionMessages, CancellationToken cancellationToken = default);
        public Task<SettersResponse> UpdateMessage(Guid messageID, MessageUpdateDTO message, CancellationToken cancellationToken = default);
        public Task<SettersResponse> DeleteMessage(Guid messageID, CancellationToken cancellationToken = default);
        public Task<Guid> GetMessageUserID(Guid messageID, CancellationToken cancellationToken = default);
        public Task<List<Guid>?> GetSessionUsersIDs(Guid sessionID, CancellationToken cancellationToken = default);
        public Task<GettersResponse<MessageMiniViewDTO>> GetSessionMessages(Guid sessionID, string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize, CancellationToken cancellationToken = default);
        public Task<GettersResponse<MessageViewDTO>> GetMessagesByFilter(string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize, CancellationToken cancellationToken = default);
        public Task<GettersResponse<MessageViewDTO>> GetMessages(int page, int pageSize, CancellationToken cancellationToken = default);
    }
}
