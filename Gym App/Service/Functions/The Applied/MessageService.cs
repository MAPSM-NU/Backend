using DocumentFormat.OpenXml.Bibliography;
using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gym_App.Service.Functions.The_Applied
{
    public class MessageService : IMessageService
    {
        private readonly DbBase _db;
        public MessageService(DbBase db)
        {
            _db = db;
        }
        public async Task<int> AddMessage(MessageDTO message)
        {
            var user = (from u in _db.Users
                        where u.UserID == message.SenderID
                        select u).FirstOrDefault();
            if (user == null) return 0;
            var session = (from s in _db.Sessions
                           where s.SessionID == message.SessionID
                           select s).FirstOrDefault();
            if (session == null) return 0;
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
            return await _db.SaveChangesAsync();
        }
        
        public async Task<int> DeleteMessages(MessageDTO message)
        {
           var Message = (from u in _db.Messages
                          where u.MessageID == message.MessageID
                          select u).FirstOrDefault();
                if (message == null) return 0;
                _db.Messages.Remove(Message);
                return await _db.SaveChangesAsync();
        }
        public async Task<int> UpdateMessage(MessageDTO message)
        {
            var Message = (from u in _db.Messages
                           where u.MessageID == message.MessageID
                           select u).FirstOrDefault();
            if (Message == null) return 0;
            Message.Content = message.Content;
            Message.IsRead = message.IsRead;
            return await _db.SaveChangesAsync();
        }
        public async Task<IQueryable<Message>> GetSessionMessages(Guid sessionID)
        {
            var messages = from m in _db.Messages.Include(m => m.Session)
                           where m.Session.SessionID == sessionID
                           select m;
            return await Task.FromResult(messages);
        }
        public async Task<IQueryable<MessageDTO>> GetMessages()
        {
            var messages = from m in _db.Messages.Include(m => m.Sender).Include(m => m.Session)
                           select new MessageDTO
                           {
                               SenderID = m.Sender.UserID,
                               SessionID = m.Session.SessionID,
                               MessageID = m.MessageID,
                               Content = m.Content,
                               IsRead = m.IsRead,
                               Timestamp = m.Timestamp
                           };
            return await Task.FromResult(messages);
        }
    }
}
