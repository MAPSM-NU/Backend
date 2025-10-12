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
        public async Task<int> AddMessage(MessageDTO message)//0 == user not found || 1 == Session not found || 2 == succesxful
        {
            var user = await (from u in _db.Users
                        where u.UserID == message.SenderID
                        select u).FirstOrDefaultAsync();
            if (user == null) return await Task.FromResult(0);
            var session = await (from s in _db.Sessions
                           where s.SessionID == message.SessionID
                           select s).FirstOrDefaultAsync();
            if (session == null) return await Task.FromResult(1);
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
            return await Task.FromResult(2);
        }
        
        public async Task<int> DeleteMessages(MessageDTO message)
        {
           var Message = await (from u in _db.Messages
                          where u.MessageID == message.MessageID
                          select u).FirstOrDefaultAsync();
                if (message == null) return await Task.FromResult(0);
                _db.Messages.Remove(Message);
                await _db.SaveChangesAsync();
                return await Task.FromResult(1);
        }
        public async Task<int> UpdateMessage(MessageDTO message)
        {
            var Message = await (from u in _db.Messages
                           where u.MessageID == message.MessageID
                           select u).FirstOrDefaultAsync();
            if (Message == null) return await Task.FromResult(0);
            Message.Content = message.Content;
            Message.IsRead = message.IsRead;
            await _db.SaveChangesAsync();
            return await Task.FromResult(1);
        }
        public async Task<List<MessageDTO>> GetSessionMessages(Guid sessionID)
        {
            var messages = await (from m in _db.Messages.Include(m => m.Session)
                           where m.Session.SessionID == sessionID
                           select new MessageDTO
                           {
                               SenderID = m.Sender.UserID,
                               SessionID = m.Session.SessionID,
                               MessageID = m.MessageID,
                               Content = m.Content,
                               IsRead = m.IsRead,
                               Timestamp = m.Timestamp
                           }).ToListAsync();
            return await Task.FromResult(messages);
        }
        public async Task<List<MessageDTO>> GetMessages()
        {
            var messages = await (from m in _db.Messages.Include(m => m.Sender).Include(m => m.Session)
                           select new MessageDTO
                           {
                               SenderID = m.Sender.UserID,
                               SessionID = m.Session.SessionID,
                               MessageID = m.MessageID,
                               Content = m.Content,
                               IsRead = m.IsRead,
                               Timestamp = m.Timestamp
                           }).ToListAsync();
            return await Task.FromResult(messages);
        }
    }
}
