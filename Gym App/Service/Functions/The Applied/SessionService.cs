using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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
        {//need to check if given users already have past session together?
            if (session == null) return 0;
            var Session = new Session
            {
                SessionID = Guid.NewGuid(),
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
        {
            if (sessionMessages == null || sessionMessages.Messages == null || sessionMessages.SessionID == Guid.Empty) 
                return 0;
            var Session = await (from s in _db.Sessions.Include(s => s.Messages)
                                 where s.SessionID == sessionMessages.SessionID
                                 select s).FirstOrDefaultAsync();
            if (Session == null) 
                return 1;
            List<Message> messagesToAdd;
            if (Session.Messages == null)
            {
                messagesToAdd = await (from m in _db.Messages
                                    where sessionMessages.Messages.Contains(m.MessageID)
                                    select m).ToListAsync();
                Session.Messages = new List<Message>();
                return 3;
            }
            else
            {
                var existingMessagesIDs = new HashSet<Guid>(Session.Messages.Select(m => m.MessageID));
                var messagesIDsToAdd = sessionMessages.Messages?.Where(id => !existingMessagesIDs.Contains(id)).ToList();
                if (messagesIDsToAdd == null || messagesIDsToAdd.Count == 0) return 2;
                messagesToAdd = await (from m in _db.Messages
                                where messagesIDsToAdd.Contains(m.MessageID)
                                select m).ToListAsync();
            }

            if(messagesToAdd == null || messagesToAdd.Count == 0) 
                return 2;

            foreach (var messageID in messagesToAdd)
            {
                Session.Messages.Add(messageID);
            }
            _db.Sessions.Update(Session);
            await _db.SaveChangesAsync();
            return 3;
        }
        public async Task<int> DeleteMessages(SessionMessagesDTO sessionMessages)//0 == Faulty DTO || 1 == session not found || 2 == session has no messages || 3 == no messages to delete || 4 == success
        { 
            if(sessionMessages == null || sessionMessages.Messages == null) 
                return 0;

            var Session = await (from s in _db.Sessions.Include(s => s.Messages)
                                 where s.SessionID == sessionMessages.SessionID
                                 select s).FirstOrDefaultAsync();
            if (Session == null) 
                return 1;

            List<Message> messagesToRemove;

            if (Session.Messages == null || Session.Messages.Count == 0)
            {
                return 2;
            }
            else
            {
                var existingMessagesIDs = new HashSet<Guid>(Session.Messages.Select(m => m.MessageID));
                var messagesIDsToRemove = sessionMessages.Messages?.Where(id => existingMessagesIDs.Contains(id)).ToList();
                if (messagesIDsToRemove == null || messagesIDsToRemove.Count == 0) return 3;
                messagesToRemove = await (from m in _db.Messages
                                   where messagesIDsToRemove.Contains(m.MessageID)
                                   select m).ToListAsync();
            }

            if(messagesToRemove == null || messagesToRemove.Count == 0) 
                return 3;

            foreach (var message in messagesToRemove)
            {
                Session.Messages.Remove(message);
            }
            _db.Sessions.Update(Session);
            await _db.SaveChangesAsync();
            return 4;
        }
        public async Task<List<Guid>?> GetSessionUsersIDs(Guid sessionID)
        {
            var Users = await (from s in _db.Sessions
                               where s.SessionID == sessionID
                               select s.Users).FirstOrDefaultAsync();
            if (Users == null) return null;
            var UserIDs = (from u in Users
                           select u.UserID).ToList();

            return UserIDs;
        }
        public async Task<PagedList<MessageDTO>?> GetSessionMessages(Guid sessionID, string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize)
        {
            if (page == 0) page = 1;
            if (pageSize == 0) pageSize = 10;
            var messageQuery = from s in _db.Sessions
                               from m in _db.Messages
                               where s.SessionID == sessionID && s.Messages.Contains(m)
                               select m;
            DateTime validStartDate, validEndDate;
            if(DateTime.TryParse(startDate,out validStartDate))
            {
                messageQuery = messageQuery.Where(m=>m.Timestamp > validStartDate);
            }
            if(DateTime.TryParse(endDate,out validEndDate))
            {
                messageQuery = messageQuery.Where(m=>m.Timestamp < validEndDate);
            }
            if (!string.IsNullOrEmpty(sortColumn))//order by custom column 
            {
                Expression<Func<Message, Object>> keySelector = sortColumn.ToLower() switch // throws error when sortColumn is null
                {
                    "message" or "content" => Message => Message.Content,
                    "time" or "t" or "timestamp" => Message => Message.Timestamp,
                    _ => Message => Message.MessageID,
                };
                if (!string.IsNullOrEmpty(OrderBy)) messageQuery = messageQuery.OrderBy(keySelector);
                else messageQuery = messageQuery.OrderByDescending(keySelector);
            }
            else
            {
                messageQuery = messageQuery.OrderByDescending(m => m.Timestamp);// or just order by recent messages
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
            var messages = await PagedList<MessageDTO>.CreateAsync(messageResponse, page, pageSize);
            return messages;
        }

        public async Task<PagedList<UserDTO>?> GetUsersOfSession(Guid sessionID, int page, int pageSize)//The sessions tree in itself needs a big change man fr
        {
            if (page == 0) page = 1;
            if (pageSize == 0) pageSize = 10;
            var sessionQuery = from s in _db.Sessions
                               from u in s.Users
                               where s.SessionID == sessionID
                               select new UserDTO
                               {
                                   UserID = u.UserID,
                                   Name = u.Name,
                                   Email = u.Email,
                                   Password = u.Password,
                                   UserType = u.UserType
                               };
            if (sessionQuery == null) return null;
            var sessionUsers = await PagedList<UserDTO>.CreateAsync(sessionQuery,page,pageSize);
            return sessionUsers;
        }
        public async Task<PagedList<SessionDTO>>? GetAllSessions(int page,int pageSize)
        {
            if (page == 0) page = 1;
            if (pageSize == 0) pageSize = 10;
            var sessionsQuery = from s in _db.Sessions
                               select new SessionDTO
                               {
                                   SessionID = s.SessionID,
                                   StartTime = s.StartTime,
                                   UserIDs = s.Users.Select(u => u.UserID).ToList(),
                               };
            var sessions = await PagedList<SessionDTO>.CreateAsync(sessionsQuery, page, pageSize);
            return sessions;
        }
    }
}
