using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Gym_App.Service.Functions.The_Applied
{
    public class SessionService : ISessionService
    {
        private readonly DbBase _db;
        private readonly IAuthorizationService _authorizationService;
        public SessionService(DbBase db,IAuthorizationService authorizationService)
        {
            _db = db;
            _authorizationService = authorizationService;
        }
        public async Task<int> CreateSession(ClaimsPrincipal User, SessionDTO session)// 0 == Faulty DTO || 1 == User not found || 2 == unauthorized || 3 == success
        {//need to check if given users already have past session together?

            //checking the validity of the DTO
            if (session == null) return 0;

            //Authorization check
            var authResult = await _authorizationService.AuthorizeAsync(User, session.UserIDs, "ListUserPolicy");
            if(!authResult.Succeeded)
                return 2;

            //Finding the users from the Database
            List<User> users = new List<User>();
            foreach (var ID in session.UserIDs) 
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.UserID == ID);
                if ( user == null ) 
                    return 1;
                else
                {
                    users.Add(user);
                }
            }

            //Creating the session
            var Session = new Session
            {
                SessionID = Guid.NewGuid(),
                StartTime = DateTime.Now,
                SessionType = " "//for now leave like this
            };

            //Adding the users to the session
            for (int i = 0; i < users.Count; i++)
            {
                Session.Users.Add(users[i]);
            }

            //Saving to Database
            _db.Sessions.Add(Session);
            await _db.SaveChangesAsync();
            return 3;
        }
        public async Task<int> DeleteSession(ClaimsPrincipal User, Guid sessionID)//0 == session not found || 1 == unauthorized || 2 == success
        {
            //checking the validity of the given Guid
            if(sessionID == Guid.Empty) 
                return 0;

            //Getting the session from the Database
            var session = await (from s in _db.Sessions.Include(s=>s.Users)
                                 where s.SessionID == sessionID
                                 select s).FirstOrDefaultAsync();

            //Return if session was not found
            if (session == null) 
                return 0;

            //Creating list of UserIDs for authorization
            List<Guid> UserIDs = new List<Guid>();
            foreach (var UsersID in session.Users) UserIDs.Add(UsersID.UserID);

            //Authorization check
            var authResult = await _authorizationService.AuthorizeAsync(User, UserIDs, "ListUserPolicy");
            if(!authResult.Succeeded) 
                return 1;

            //Removing from Database
            _db.Sessions.Remove(session);
            await _db.SaveChangesAsync();
            return 2;
        }
        public async Task<int> AddMessages(ClaimsPrincipal User, SessionMessagesDTO sessionMessages)//0 == Faulty DTO || 1 == session not found || 2 == unauthorized || 3 == no new messages to add
                                                                                                    //|| 4 == no messages found || 5 == success
        {
            //checking the validity of the DTO
            if (sessionMessages == null || sessionMessages.Messages == null || sessionMessages.SessionID == Guid.Empty) 
                return 0;

            //Getting the session from the Database
            var Session = await (from s in _db.Sessions.Include(s => s.Messages).Include(s => s.Users)
                                 where s.SessionID == sessionMessages.SessionID
                                 select s).FirstOrDefaultAsync();
            Console.WriteLine($"Users count: {Session?.Users?.Count ?? -1}");

            //Return if session was not found
            if (Session == null) 
                return 1;

            //Creating list of UserIDs for authorization
            List<Guid> UserIDs = new List<Guid>();
            foreach(var UsersID in Session.Users) UserIDs.Add(UsersID.UserID);

            //Authorization check
            var authResult = await _authorizationService.AuthorizeAsync(User, UserIDs, "ListUserPolicy");
            if(!authResult.Succeeded) 
                return 2;

            //checking the messages to add
            List<Message> messagesToAdd;
            if (Session.Messages == null)
            {
                //If session has no messages, add all given messages if their IDs point to acctual messages in the database
                messagesToAdd = await (from m in _db.Messages
                                    where sessionMessages.Messages.Contains(m.MessageID)
                                    select m).ToListAsync();
                Session.Messages = new List<Message>();
            }
            else
            {
                //If session has messages, only add the new ones if their IDs point to acctual messages in the database
                var existingMessagesIDs = new HashSet<Guid>(Session.Messages.Select(m => m.MessageID));
                var messagesIDsToAdd = sessionMessages.Messages?.Where(id => !existingMessagesIDs.Contains(id)).ToList();
                if (messagesIDsToAdd == null || messagesIDsToAdd.Count == 0)
                    return 3;
                messagesToAdd = await (from m in _db.Messages
                                where messagesIDsToAdd.Contains(m.MessageID)
                                select m).ToListAsync();
            }

            //return if no messages where found in the Database
            if (messagesToAdd == null || messagesToAdd.Count == 0) 
                return 4;

            //Adding the messages to the session
            foreach (var messageID in messagesToAdd)
            {
                Session.Messages.Add(messageID);
            }

            //Saving to Database
            _db.Sessions.Update(Session);
            await _db.SaveChangesAsync();
            return 5;
        }
        public async Task<int> DeleteMessages(ClaimsPrincipal User , SessionMessagesDTO sessionMessages)//0 == Faulty DTO || 1 == session not found || 2 == unauthorized || 3 == no messages in session
                                                                                                        //|| 4 == no messages to remove found || 5 == no messages found || 6 == success
        {
            //checking the validity of the DTO
            if (sessionMessages == null || sessionMessages.Messages == null) 
                return 0;

            //Getting the session from the Database
            var Session = await (from s in _db.Sessions.Include(s => s.Messages).Include(s => s.Users)
                                 where s.SessionID == sessionMessages.SessionID
                                 select s).FirstOrDefaultAsync();

            //Return if session was not found
            if (Session == null) 
                return 1;

            //Creating list of UserIDs for authorization
            List<Guid> UserIDs = new List<Guid>();
            foreach (var UsersID in Session.Users) UserIDs.Add(UsersID.UserID);

            //Authorization check
            var authResult = await _authorizationService.AuthorizeAsync(User, UserIDs, "ListUserPolicy");
            if(!authResult.Succeeded) 
                return 2;
            
            //checking the messages to remove
            List<Message> messagesToRemove;
            if (Session.Messages == null || Session.Messages.Count == 0)
            {
                //If session has no messages, return
                return 3;
            }
            else
            {
                //If session has messages, only remove the ones that exist in the session
                var existingMessagesIDs = new HashSet<Guid>(Session.Messages.Select(m => m.MessageID));
                var messagesIDsToRemove = sessionMessages.Messages?.Where(id => existingMessagesIDs.Contains(id)).ToList();
                if (messagesIDsToRemove == null || messagesIDsToRemove.Count == 0)
                    return 4;
                messagesToRemove = await (from m in _db.Messages
                                   where messagesIDsToRemove.Contains(m.MessageID)
                                   select m).ToListAsync();
            }

            //If there are no messages to remove, return
            if (messagesToRemove == null || messagesToRemove.Count == 0) 
                return 5;

            //Removing the messages from the session
            foreach (var message in messagesToRemove)
            {
                Session.Messages.Remove(message);
            }

            //Saving to Database
            _db.Sessions.Update(Session);
            await _db.SaveChangesAsync();
            return 6;
        }
        public async Task<List<Guid>?> GetSessionUsersIDs(ClaimsPrincipal User,Guid sessionID)
        {
            //Getting the users of the session from the Database
            var Users = await (from s in _db.Sessions
                               where s.SessionID == sessionID
                               select s.Users).FirstOrDefaultAsync();

            //Return null if session not found
            if (Users == null) return null;

            //Creating list of UserIDs for authorization
            var UserIDs = (from u in Users
                           select u.UserID).ToList();

            //Authorization check
            var authResult = await _authorizationService.AuthorizeAsync(User, UserIDs, "ListUserPolicy");
            if(!authResult.Succeeded) 
                return null;

            return UserIDs;
        }
        public async Task<PagedList<MessageDTO>?> GetSessionMessages(ClaimsPrincipal User, Guid sessionID, string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize)
        {
            //if page or pageSize are 0, set default values
            if (page == 0) page = 1;
            if (pageSize == 0) pageSize = 10;

            //Getting the messages of the session from the Database
            var Session = await (from s in _db.Sessions.Include(s => s.Users)
                                from m in _db.Messages
                                where s.SessionID == sessionID && s.Messages.Contains(m)
                                select s).FirstOrDefaultAsync();
            //Return null if session not found
            if (Session == null) return null;

            //Creating list of UserIDs for authorization
            List<Guid> UserIDs = new List<Guid>();
            foreach (var UsersID in Session.Users) UserIDs.Add(UsersID.UserID);

            //Authorization check
            var authResult = await _authorizationService.AuthorizeAsync(User, UserIDs, "ListUserPolicy");
            if(!authResult.Succeeded) 
                return null;

            //Getting messages from Database
            IQueryable<Message> messageQuery = from m in _db.Messages
                                               where m.Session.SessionID == sessionID
                                               select m;

            //Filtering by search Term
            if (!string.IsNullOrEmpty(searchTerm)) messageQuery = messageQuery.Where(e => e.Content.Contains(searchTerm));

            //filter by start and end date
            DateTime validStartDate, validEndDate;
            if(DateTime.TryParse(startDate,out validStartDate))
            {
                messageQuery = messageQuery.Where(m=>m.Timestamp > validStartDate);
            }
            if(DateTime.TryParse(endDate,out validEndDate))
            {
                messageQuery = messageQuery.Where(m=>m.Timestamp < validEndDate);
            }
            //Order by given column
            if (!string.IsNullOrEmpty(sortColumn))
            {
                Expression<Func<Message, Object>> keySelector = sortColumn.ToLower() switch // throws error when sortColumn is null
                {
                    "message" or "content" => Message => Message.Content, // order by messages or content
                    "time" or "t" or "timestamp" => Message => Message.Timestamp, // order by time or timestamp
                    _ => Message => Message.MessageID, //failsafe: order by ID
                };
                //If no orderby was inputed, then we sort ascending
                if (!string.IsNullOrEmpty(OrderBy)) messageQuery = messageQuery.OrderBy(keySelector);

                //else if anything was inputted we sort descending
                else messageQuery = messageQuery.OrderByDescending(keySelector);
            }
            else
            {
                // or just order by recent messages
                messageQuery = messageQuery.OrderByDescending(m => m.Timestamp);
            }
            ////Projecting the resultant message queries to messageDTO
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

            //Making the result as a paged list
            var messages = await PagedList<MessageDTO>.CreateAsync(messageResponse, page, pageSize);
            return messages;
        }
        public async Task<PagedList<UserDTO>?> GetUsersOfSession(ClaimsPrincipal User,Guid sessionID, int page, int pageSize)//The sessions tree in itself needs a big change man fr
        {
            //if page or pageSize are 0, set default values
            if (page == 0) page = 1;
            if (pageSize == 0) pageSize = 10;
            
            var session = await _db.Sessions.Include(s=>s.Users).FirstOrDefaultAsync(s => s.SessionID == sessionID);
            if (session == null) 
                return null;

            //Creating list of UserIDs for authorization
            List<Guid> UserIDs = new List<Guid>();
            foreach (var UsersID in session.Users) UserIDs.Add(UsersID.UserID);

            //Authroization check
            var auhtResult = await _authorizationService.AuthorizeAsync(User, UserIDs, "ListUserPolicy");
            if(!auhtResult.Succeeded) 
                return null;

            //Projecting the users to UserDTO
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

            //Making the result as a paged list
            var sessionUsers = await PagedList<UserDTO>.CreateAsync(sessionQuery,page,pageSize);
            return sessionUsers;
        }
        public async Task<PagedList<SessionDTO>>? GetAllSessions(int page,int pageSize)
        {
            //if page or pageSize are 0, set default values
            if (page == 0) page = 1;
            if (pageSize == 0) pageSize = 10;

            //Getting all sessions from Database and projecting them to SessionDTO
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
