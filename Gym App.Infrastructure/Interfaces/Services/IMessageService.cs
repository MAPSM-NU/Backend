using Gym_App.Infastructure.DTOs.Message;
using Gym_App.Infastructure.Transfer_Classes;
using System.Security.Claims;

namespace Gym_App.Infastructure.Interfaces.Services
{
    public interface IMessageService
    {
        public Task<SettersResponse> AddMessage(ClaimsPrincipal User,Guid senderID, MessageCreationDTO sessionMessages);
        public Task<SettersResponse> UpdateMessage(ClaimsPrincipal User,Guid messageID, MessageUpdateDTO message);
        public Task<SettersResponse> DeleteMessage(ClaimsPrincipal User, Guid messageID);
        public Task<Guid> GetMessageUserID(Guid messageID);
        public Task<List<Guid>?> GetSessionUsersIDs(ClaimsPrincipal User, Guid sessionID);
        public Task<GettersResponse<MessageMiniViewDTO>> GetSessionMessages(ClaimsPrincipal User, Guid sessionID, string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize);
        public Task<GettersResponse<MessageViewDTO>> GetMessagesByFilter(string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize);
        public Task<GettersResponse<MessageViewDTO>> GetMessages(int page, int pageSize);
    }
}
