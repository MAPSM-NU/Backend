using Gym_App.Domain;

namespace Gym_App.Infastructure.Interfaces.Repositries
{
    public interface IMessageRepositry : IBaseRepositry<Message>
    {
        // Retrieval methods
        Task<Message> GetMessageById(Guid messageId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Message>> GetSessionMessages(Guid sessionId, CancellationToken cancellationToken = default);
        IQueryable<Message> GetSessionMessagesQueryable(Guid sessionId);
        IQueryable<Message> GetAllMessagesQueryable();

        // Unread messages
        Task<IEnumerable<Message>> GetUnreadMessages(Guid receiverId, CancellationToken cancellationToken = default);
        Task<int> GetUnreadMessageCount(Guid receiverId, CancellationToken cancellationToken = default);
        Task<bool> HasUnreadMessages(Guid userId, CancellationToken cancellationToken = default);

        // Status management
        Task MarkAsRead(Guid messageId, CancellationToken cancellationToken = default);
        Task MarkMultipleAsRead(IEnumerable<Guid> messageIds, CancellationToken cancellationToken = default);
        Task MarkConversationAsRead(Guid senderId, Guid receiverId, CancellationToken cancellationToken = default);

        // Existence checks
        Task<bool> MessageExists(Guid messageId, CancellationToken cancellationToken = default);
    }
}
