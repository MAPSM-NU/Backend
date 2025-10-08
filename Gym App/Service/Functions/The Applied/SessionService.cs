using DocumentFormat.OpenXml.Drawing;
using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gym_App.Service.Functions.The_Applied
{
    public class SessionService : ISessionService
    {
        private readonly DbBase _db;
        public SessionService(DbBase db)
        {
            _db = db;
        }


        public async Task<int> CreateSession(SessionDTO session)
        {
            var Session = new Session
            {
                SessionID = new Guid(),
                StartTime = DateTime.Now,
                SessionType = " "//for now leave like this
            };
            foreach (var ID in session.UserIDs) 
            {
                var user = _db.Users.FirstOrDefault(u => u.UserID == ID);
                if (user==null) return await Task.FromResult(0);
                else
                {
                    Session.Users.Add(user);
                }
            } 
            _db.Sessions.Add(Session);
            return await _db.SaveChangesAsync(); // Gonna try this here. The return is how many rows were affected
        }
        public async Task<int> DeleteSession(Guid sessionID)
        {
            var session = _db.Sessions.Find(sessionID);
            if (session == null) return await Task.FromResult(0);
            _db.Sessions.Remove(session);
            return await _db.SaveChangesAsync();
        }

        public async Task<int> AddMessages(SessionMessagesDTO sessionMessages)
        {
            var Session = (from s in _db.Sessions.Include(s => s.Messages)
                          select s).FirstOrDefault();
            if (Session == null) return await Task.FromResult(0);
            foreach (var messageID in sessionMessages.Messages ?? [])
            {
                var message = _db.Messages.Find(messageID);
                if (message != null && !Session.Messages.Any(m => m.MessageID == messageID))
                {
                    Session.Messages.Add(message);
                }
            }
            return await _db.SaveChangesAsync();
        }
        public async Task<int> DeleteMessages(SessionMessagesDTO sessionMessages)
        {
            var Session = (from s in _db.Sessions.Include(s => s.Messages)
                           select s).FirstOrDefault();
            if (Session == null) return await Task.FromResult(0);
            foreach (var messageID in sessionMessages.Messages ?? [])
            {
                var message = _db.Messages.Find(messageID);
                if (message != null && Session.Messages.Any(m => m.MessageID == messageID))
                {
                    Session.Messages.Remove(message);
                }
            }
            return await _db.SaveChangesAsync();
        }
        public async Task<IQueryable<MessageDTO>>? GetSessionMessages(Guid sessionID)
        {
            var Session = (from s in _db.Sessions.Include(s => s.Messages)
                           select s).FirstOrDefault();
            if (Session == null) return null;
            var messages = Session.Messages.Select(m => new MessageDTO
            {
                MessageID = m.MessageID,
                Content = m.Content,
                Timestamp = m.Timestamp,
                SenderID = m.Sender.UserID
            }).AsQueryable();
            return await Task.FromResult(messages);
        }

        public async Task<ICollection<User>>? GetUsersOfSession(Guid sessionID)//The sessions tree in itself needs a big change man fr
        {
            var Session = _db.Sessions.Include(s=>s.Users).FirstOrDefault(s => s.SessionID == sessionID);
            if(Session == null) return null;
            return await Task.FromResult(Session.Users);
        }
        public async Task<IQueryable<Session>>? GetAllSessions()
        {
            var sessions = _db.Sessions.Include(s=>s.Users).AsQueryable();
            return await Task.FromResult(sessions);
        }
    }
}
