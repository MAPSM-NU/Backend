using Gym_App.Domain;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.Interfaces.Repositries;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Gym_App.Infastructure.Repositries
{
    public class MessageRepositry : BaseRepositry<Message>, IMessageRepositry
    {
        private readonly DbBase _db;
        private readonly DbSet<Message> table;

        public MessageRepositry(DbBase db) : base(db)
        {
            _db = db;
            table = _db.Set<Message>();
        }

        public async Task<Message> GetMessageById(Guid messageId)
        {
            return await table
                .Include(m => m.Sender)
                .Include(m => m.Session)
                .FirstOrDefaultAsync(m => m.Id == messageId);
        }

        public async Task<IEnumerable<Message>> GetSessionMessages(Guid sessionId)
        {
            return await table
                .Where(m => m.Session.Id == sessionId)
                .Include(m => m.Sender)
                .Include(m => m.Session)
                .ToListAsync();
        }

        public IQueryable<Message> GetSessionMessagesQueryable(Guid sessionId)
        {
            return table
                .Where(m => m.Session.Id == sessionId)
                .Include(m => m.Sender)
                .Include(m => m.Session);
        }

        public IQueryable<Message> GetAllMessagesQueryable()
        {
            return table
                .Include(m => m.Sender)
                .Include(m => m.Session);
        }

        public async Task<IEnumerable<Message>> GetUnreadMessages(Guid receiverId)
        {
            return await table
                .Where(m => m.Session.Users.Any(u => u.Id == receiverId) && !m.IsRead)
                .Include(m => m.Sender)
                .Include(m => m.Session)
                .ToListAsync();
        }

        public async Task<int> GetUnreadMessageCount(Guid receiverId)
        {
            return await table
                .Where(m => m.Session.Users.Any(u => u.Id == receiverId) && !m.IsRead)
                .CountAsync();
        }

        public async Task<bool> HasUnreadMessages(Guid userId)
        {
            return await table
                .AnyAsync(m => m.Session.Users.Any(u => u.Id == userId) && !m.IsRead);
        }

        public async Task MarkAsRead(Guid messageId)
        {
            var message = await table.FirstOrDefaultAsync(m => m.Id == messageId);
            if (message != null)
            {
                message.IsRead = true;
            }
        }

        public async Task MarkMultipleAsRead(IEnumerable<Guid> messageIds)
        {
            var messages = await table.Where(m => messageIds.Contains(m.Id)).ToListAsync();
            foreach (var message in messages)
            {
                message.IsRead = true;
            }
        }

        public async Task MarkConversationAsRead(Guid senderId, Guid receiverId)
        {
            var messages = await table
                .Where(m => (m.Sender.Id == senderId || m.Sender.Id == receiverId) && !m.IsRead)
                .ToListAsync();
            foreach (var message in messages)
            {
                message.IsRead = true;
            }
        }

        public async Task<bool> MessageExists(Guid messageId)
        {
            return await table.AnyAsync(m => m.Id == messageId);
        }

        public override IQueryable<Message> FilterSortColumn(string columnName, string sortOrder, IQueryable<Message> query)
        {
            Expression<Func<Message, object>> keySelector = columnName.ToLower() switch
            {
                "message" or "content" => m => m.Content,
                "time" or "t" or "timestamp" or "date" or "d" => m => m.CreatedAt,
                _ => m => m.Id,
            };

            var orderLower = (sortOrder ?? string.Empty).ToLowerInvariant();
            bool descending = orderLower == "desc" || orderLower == "descending" || orderLower == "descend" || orderLower == "d";

            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        public override IQueryable<Message> Search(string searchTerm, IQueryable<Message> query)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return query;

            return query.Where(m => m.Content.Contains(searchTerm));
        }
    }
}
