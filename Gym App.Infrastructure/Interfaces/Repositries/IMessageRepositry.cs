using Gym_App.Domain;

namespace Gym_App.Infastructure.Interfaces.Repositries
{
    public interface IMessageRepositry : IBaseRepositry<Message>
    {
        // Retrieval methods
        Task<Message> GetMessageById(Guid messageId);
        Task<IEnumerable<Message>> GetSessionMessages(Guid sessionId);
        IQueryable<Message> GetSessionMessagesQueryable(Guid sessionId);
        IQueryable<Message> GetAllMessagesQueryable();
        
        // Unread messages
        Task<IEnumerable<Message>> GetUnreadMessages(Guid receiverId);
        Task<int> GetUnreadMessageCount(Guid receiverId);
        Task<bool> HasUnreadMessages(Guid userId);
        
        // Status management
        Task MarkAsRead(Guid messageId);
        Task MarkMultipleAsRead(IEnumerable<Guid> messageIds);
        Task MarkConversationAsRead(Guid senderId, Guid receiverId);
        
        // Existence checks
        Task<bool> MessageExists(Guid messageId);
    }
}
