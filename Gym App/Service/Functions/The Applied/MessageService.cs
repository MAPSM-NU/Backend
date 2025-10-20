using DocumentFormat.OpenXml.Bibliography;
using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Gym_App.Service.Functions.The_Applied
{
    public class MessageService : IMessageService
    {
        private readonly DbBase _db;
        public MessageService(DbBase db)
        {
            _db = db;
        }
        public async Task<int> AddMessage(MessageDTO message)//0 == user not found || 1 == Session not found || 2 == succesxful
        {
            var user = await (from u in _db.Users
                        where u.UserID == message.SenderID
                        select u).FirstOrDefaultAsync();
            if (user == null) return 0;
            var session = await (from s in _db.Sessions
                           where s.SessionID == message.SessionID
                           select s).FirstOrDefaultAsync();
            if (session == null) return 1;
            var newMessage = new Message
            {
                MessageID = Guid.NewGuid(),
                Sender = user,
                Session = session,
                Content = message.Content,
                Timestamp = DateTime.UtcNow,
                IsRead = message.IsRead
            };
            await _db.Messages.AddAsync(newMessage);
            await _db.SaveChangesAsync();
            return 2;
        }
        
        public async Task<int> DeleteMessages(MessageDTO message)
        {
           var Message = await (from u in _db.Messages
                          where u.MessageID == message.MessageID
                          select u).FirstOrDefaultAsync();
                if (message == null) return 0;
                _db.Messages.Remove(Message);
                await _db.SaveChangesAsync();
                return 1;
        }
        public async Task<int> UpdateMessage(MessageDTO message)
        {
            var Message = await (from u in _db.Messages
                           where u.MessageID == message.MessageID
                           select u).FirstOrDefaultAsync();
            if (Message == null) return 0;
            Message.Content = message.Content;
            Message.IsRead = message.IsRead;
            await _db.SaveChangesAsync();
            return 1;
        }
        public async Task<PagedList<MessageDTO>> GetSessionMessages(Guid sessionID,int page, int pageSize)
        {
            var messagesQuery = (from m in _db.Messages
                           where m.Session.SessionID == sessionID
                           select new MessageDTO
                           {
                               SenderID = m.Sender.UserID,
                               SessionID = m.Session.SessionID,
                               MessageID = m.MessageID,
                               Content = m.Content,
                               IsRead = m.IsRead,
                               Timestamp = m.Timestamp
                           });
            var messages = await PagedList<MessageDTO>.CreateAsync(messagesQuery, page, pageSize);
            return messages;
        }
        public async Task<PagedList<MessageDTO>> GetMessagesByFilter(int page, string sortColumn, string OrderBy, string searchTerm, int pageSize)
        {
            IQueryable<Message> messageQuery = _db.Messages;
            if (!string.IsNullOrEmpty(searchTerm)) messageQuery = messageQuery.Where(m => m.Content.Contains(searchTerm));//Might add user name belmara bs will wait
            if (!string.IsNullOrEmpty(sortColumn))
            {
                Expression<Func<Message, Object>> keySelector = sortColumn.ToLower() switch // throws error when sortColumn is null
                {
                    "message" or "content" => Message => Message.Content,
                    _ => Message => Message.MessageID,
                };
                if(!string.IsNullOrEmpty(OrderBy))messageQuery.OrderBy(keySelector);
                else messageQuery.OrderByDescending(keySelector);
            }
            var messageResponse = messageQuery
                                    .Select(m => new MessageDTO
                                    {
                                        SenderID = m.Sender.UserID,
                                        SessionID = m.Session.SessionID,
                                        MessageID = m.MessageID,
                                        Content = m.Content,
                                        IsRead = m.IsRead,
                                        Timestamp = m.Timestamp
                                    });
            var messages = await PagedList<MessageDTO>.CreateAsync(messageResponse,page,pageSize);
            return messages;
        }
        public async Task<PagedList<MessageDTO>> GetMessages(int page, int pageSize)
        {
            var messagesQuery = (from m in _db.Messages
                                  select new MessageDTO
                           {
                               SenderID = m.Sender.UserID,
                               SessionID = m.Session.SessionID,
                               MessageID = m.MessageID,
                               Content = m.Content,
                               IsRead = m.IsRead,
                               Timestamp = m.Timestamp
                           });
            var messages = await PagedList<MessageDTO>.CreateAsync(messagesQuery, page, pageSize);
            return messages;
        }

    }
}
