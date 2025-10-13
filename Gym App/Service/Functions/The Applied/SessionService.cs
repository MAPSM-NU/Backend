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
            if (session == null) return 0;
            var Session = new Session
            {
                SessionID = new Guid(),
                StartTime = DateTime.Now,
                SessionType = " "//for now leave like this
            };
            foreach (var ID in session.UserIDs) 
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.UserID == ID);
                if (user==null) return 1;
                else
                {
                    Session.Users.Add(user);
                }
            } 
            _db.Sessions.Add(Session);
            await _db.SaveChangesAsync();
            return 2;
        }
        public async Task<int> DeleteSession(Guid sessionID)
        {
            var session = await (from s in _db.Sessions
                                 where s.SessionID == sessionID
                                 select s).FirstOrDefaultAsync();
            if (session == null) return 0;
            _db.Sessions.Remove(session);
            await _db.SaveChangesAsync();
            return 1;
        }

        public async Task<int> AddMessages(SessionMessagesDTO sessionMessages)//0 == Faulty DTO || 1 == session not found || 2 == no new messages added || 3 == success
        {                                                                     //Probably could be optimized more
            if (sessionMessages == null) return 0;
            bool AddedAny = false;
            var Session = await(from s in _db.Sessions.Include(s => s.Messages)
                          where s.SessionID == sessionMessages.SessionID
                          select s).FirstOrDefaultAsync();
            if (Session == null) return 1;
            foreach (var messageID in sessionMessages.Messages ?? [])
            {
                var message = await (from m in _db.Messages
                                     where m.MessageID == messageID
                                     select m).FirstOrDefaultAsync();
                if (message != null && !Session.Messages.Any(m => m.MessageID == messageID))
                {
                    Session.Messages.Add(message);
                    AddedAny = true;
                }
            }
            if (!AddedAny) return 2;
            _db.Sessions.Update(Session);
            await _db.SaveChangesAsync();
            return 3;
        }
        public async Task<int> DeleteMessages(SessionMessagesDTO sessionMessages)//0 == Faulty DTO || 1 == session not found || 2 == no messages deleted || 3 == success
        {                                                                        //Probably could be optimized more
            if (sessionMessages == null) return 0;
            bool DeletedAny = false;
            var Session = await (from s in _db.Sessions.Include(s => s.Messages)
                                 where s.SessionID == sessionMessages.SessionID
                                 select s).FirstOrDefaultAsync();
            if (Session == null) return 1;
            foreach (var messageID in sessionMessages.Messages ?? [])
            {
                var message = _db.Messages.Find(messageID);
                if (message != null && Session.Messages.Any(m => m.MessageID == messageID))
                {
                    Session.Messages.Remove(message);
                    DeletedAny = true;
                }
            }
            if (!DeletedAny) return 2;
            _db.Sessions.Update(Session);
            await _db.SaveChangesAsync();
            return 3;
        }
        public async Task<List<MessageDTO>?> GetSessionMessages(Guid sessionID)
        {
            var sessionMessages = await (from s in _db.Sessions
                                  from m in _db.Messages
                                  where s.SessionID == sessionID && s.Messages.Contains(m)
                                  select new MessageDTO
                                    {
                                        SenderID = m.Sender.UserID,
                                        SessionID = s.SessionID,
                                        MessageID = m.MessageID,
                                        Content = m.Content,
                                        Timestamp = m.Timestamp,
                                        IsRead = m.IsRead
                                    }).ToListAsync();
            return sessionMessages;
        }

        public async Task<List<UserDTO>?> GetUsersOfSession(Guid sessionID)//The sessions tree in itself needs a big change man fr
        {
            var sessionUsers = await (from s in _db.Sessions
                                      from u in s.Users
                                      where s.SessionID == sessionID
                                      select new UserDTO
                                      {
                                          UserID = u.UserID,
                                          Name = u.Name,
                                          Email = u.Email,
                                          Password = u.Password,
                                          UserType = u.UserType
                                      }).ToListAsync();
            if (sessionUsers == null) return null;
            return sessionUsers;
        }
        public async Task<List<SessionDTO>>? GetAllSessions()
        {
            var sessions = await (from s in _db.Sessions
                                  select new SessionDTO
                                  {
                                      SessionID = s.SessionID,
                                      StartTime = s.StartTime,
                                      UserIDs = s.Users.Select(u => u.UserID).ToList(),
                                  }).ToListAsync();
            return sessions;
        }
    }
}
