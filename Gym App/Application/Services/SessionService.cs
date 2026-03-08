using DocumentFormat.OpenXml.InkML;
using Gym_App.Domain;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.DTOs.Message;
using Gym_App.Infastructure.DTOs.Session;
using Gym_App.Infastructure.DTOs.UserDTOs;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infastructure.Transfer_Classes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Gym_App.Application.Services
{
    public class SessionService : ISessionService
    {
        private readonly DbBase _db;
        private readonly IAuthorizationService _authorizationService;

        //        *********** Setters ***********

        //0 == Error(Bad Request) || 1 ==Unauthorized (Forbid) || 2 == Success (Ok)
        public SessionService(DbBase db,IAuthorizationService authorizationService)
        {
            _db = db;
            _authorizationService = authorizationService;
        }
        public async Task<SettersResponse> CreateSession(ClaimsPrincipal User, Guid user1,Guid user2)
        {//need to check if given users already have past session together?

            //checking the validity of the DTO
            if (user1 == Guid.Empty || user2 == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid user IDs" };

            List<Guid> userIDs = new List<Guid> { user1, user2 };

            //Authorization check
            var authResult = await _authorizationService.AuthorizeAsync(User, userIDs, "ListUserPolicy");
            if(!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Finding the users from the Database
            List<User> users = new List<User>();
            foreach (var ID in userIDs) 
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == ID);
                if ( user == null ) 
                    return new SettersResponse { status = 0, msg = "User not found" };
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
            await _db.Sessions.AddAsync(Session);
            await _db.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Session created successfully" };
        }
        public async Task<SettersResponse> DeleteSession(ClaimsPrincipal User, Guid sessionID)
        {
            //checking the validity of the given Guid
            if(sessionID == Guid.Empty) 
                return new SettersResponse { status = 0, msg = "Invalid session ID" };

            //Getting the session from the Database
            var session = await (from s in _db.Sessions.Include(s=>s.Users)
                                 where s.SessionID == sessionID
                                 select s).FirstOrDefaultAsync();

            //Return if session was not found
            if (session == null) 
                return new SettersResponse { status = 0, msg = "Session not found" };

            //Creating list of UserIDs for authorization
            List<Guid> UserIDs = new List<Guid>();
            foreach (var UsersID in session.Users) UserIDs.Add(UsersID.Id);

            //Authorization check
            var authResult = await _authorizationService.AuthorizeAsync(User, UserIDs, "ListUserPolicy");
            if(!authResult.Succeeded) 
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Removing from Database
            _db.Sessions.Remove(session);
            await _db.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Session deleted successfully" };
        }
        public async Task<SettersResponse> AddMessages(ClaimsPrincipal User, Guid sessionID, SessionMessagesDTO sessionMessages)
        {
            //checking the validity of the DTO
            if (sessionMessages == null || sessionID == Guid.Empty) 
                return new SettersResponse { status = 0, msg = "Invalid session ID or messages" };

            //Getting the session from the Database
            var Session = await (from s in _db.Sessions.Include(s => s.Messages).Include(s => s.Users)
                                 where s.SessionID == sessionID
                                 select s).FirstOrDefaultAsync();

            //Return if session was not found
            if (Session == null) 
                return new SettersResponse { status = 0, msg = "Session not found" };

            //Creating list of UserIDs for authorization
            List<Guid> UserIDs = new List<Guid>();
            foreach(var UsersID in Session.Users) UserIDs.Add(UsersID.Id);

            //Authorization check
            var authResult = await _authorizationService.AuthorizeAsync(User, UserIDs, "ListUserPolicy");
            if(!authResult.Succeeded) 
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //checking the messages to add
            List<Message> messagesToAdd;
            if (Session.Messages == null)
            {
                //If session has no messages, add all given messages if their IDs point to acctual messages in the database
                messagesToAdd = await (from m in _db.Messages
                                    where sessionMessages.messagesID.Contains(m.MessageID)
                                    select m).ToListAsync();
                Session.Messages = new List<Message>();
            }
            else
            {
                //If session has messages, only add the new ones if their IDs point to acctual messages in the database
                var existingMessagesIDs = new HashSet<Guid>(Session.Messages.Select(m => m.MessageID));
                var messagesIDsToAdd = sessionMessages.messagesID?.Where(id => !existingMessagesIDs.Contains(id)).ToList();
                if (messagesIDsToAdd == null || messagesIDsToAdd.Count == 0)
                    return new SettersResponse { status = 0, msg = "No new messages to add" };
                messagesToAdd = await (from m in _db.Messages
                                where messagesIDsToAdd.Contains(m.MessageID)
                                select m).ToListAsync();
            }

            //return if no messages where found in the Database
            if (messagesToAdd == null || messagesToAdd.Count == 0) 
                return new SettersResponse { status = 0, msg = "No messages found in the database" };

            //Adding the messages to the session
            foreach (var messageID in messagesToAdd)
            {
                Session.Messages.Add(messageID);
            }

            //Saving to Database
            _db.Sessions.Update(Session);
            await _db.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Messages added successfully" };
        }
        public async Task<SettersResponse> DeleteMessages(ClaimsPrincipal User , Guid sessionID, SessionMessagesDTO sessionMessages)
        {
            //checking the validity of the DTO
            if (sessionMessages == null|| sessionID == Guid.Empty) 
                return new SettersResponse { status = 0, msg = "Invalid session ID or messages" };

            //Getting the session from the Database
            var Session = await (from s in _db.Sessions.Include(s => s.Messages).Include(s => s.Users)
                                 where s.SessionID == sessionID
                                 select s).FirstOrDefaultAsync();

            //Return if session was not found
            if (Session == null) 
                return new SettersResponse { status = 0, msg = "Session not found" };

            //Creating list of UserIDs for authorization
            List<Guid> UserIDs = new List<Guid>();
            foreach (var UsersID in Session.Users) UserIDs.Add(UsersID.Id);

            //Authorization check
            var authResult = await _authorizationService.AuthorizeAsync(User, UserIDs, "ListUserPolicy");
            if(!authResult.Succeeded) 
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Getting Sender ID
            var senderID = GettingSenderID(User);
            if(senderID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid sender ID" };

            //checking the messages to remove
            List<Message> messagesToRemove;
            if (Session.Messages == null || Session.Messages.Count == 0)
            {
                //If session has no messages, return
                return new SettersResponse { status = 0, msg = "No messages found in the session" };
            }
            else
            {
                //If session has messages, only remove the ones that exist in the session
                var existingMessagesIDs = new HashSet<Guid>(Session.Messages.Select(m => m.MessageID));
                var messagesIDsToRemove = sessionMessages.messagesID.Where(id => existingMessagesIDs.Contains(id)).ToList();
                if (messagesIDsToRemove == null || messagesIDsToRemove.Count == 0)
                    return new SettersResponse { status = 0, msg = "No messages found in the database" };
                messagesToRemove = await (from m in _db.Messages.Include(m=>m.Sender)
                                          where messagesIDsToRemove.Contains(m.MessageID)
                                          select m).ToListAsync();
            }

            //If there are no messages to remove, return
            if (messagesToRemove == null || messagesToRemove.Count == 0) 
                return new SettersResponse { status = 0, msg = "No messages found to remove" };

            //Removing the messages from the session
            foreach (var message in messagesToRemove)
            {
                //If the user is trying to delete a message that is not his return forbidden
                if (message.Sender.Id != senderID)
                    return new SettersResponse { status = 1, msg = "Unauthorized" };

                Session.Messages.Remove(message);
            }

            //Saving to Database
            _db.Sessions.Update(Session);
            await _db.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Messages removed successfully" };
        }

        //-----------------------------------------------------------------------


        //        *********** Extra Validation Function ***********

        public async Task<bool> isMessageBelongUser(Guid messageID, Guid userID)
        {
            var message = await _db.Messages.Include(m => m.Sender).FirstOrDefaultAsync(m => m.MessageID == messageID);
            if (message == null) 
                return false;
            return message.Sender.Id == userID;
        }
        public Guid GettingSenderID(ClaimsPrincipal User)
        {
            var userID = User.FindFirst(ClaimTypes.NameIdentifier)
                        ?? User.FindFirst(JwtRegisteredClaimNames.Sub)
                        ?? User.FindFirst("sub")
                        ?? User.FindFirst("userId")
                        ?? User.FindFirst("id");

            if (Guid.TryParse(userID!.Value, out var validID))
                return validID;
            else
                return Guid.Empty;
        }

        //-----------------------------------------------------------------------

        //        *********** Getters ***********

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
                           select u.Id).ToList();

            //Authorization check
            var authResult = await _authorizationService.AuthorizeAsync(User, UserIDs, "ListUserPolicy");
            if(!authResult.Succeeded) 
                return null;

            return UserIDs;
        }
        public async Task<GettersResponse<MessageViewDTO>> GetSessionMessages(ClaimsPrincipal User, Guid sessionID, string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize)
        {
            //Getting the messages of the session from the Database
            var Session = await (from s in _db.Sessions.Include(s => s.Users)
                                 where s.SessionID == sessionID
                                 select s).FirstOrDefaultAsync();
            //Return null if session not found
            if (Session == null)
                return new GettersResponse<MessageViewDTO>
                {
                    status = 0,
                    msg = "Session not found"
                };

            //Creating list of UserIDs for authorization
            List<Guid> UserIDs = new List<Guid>();
            foreach (var UsersID in Session.Users) UserIDs.Add(UsersID.Id);

            //Authorization check
            var authResult = await _authorizationService.AuthorizeAsync(User, UserIDs, "ListUserPolicy");
            if (!authResult.Succeeded)
                return new GettersResponse<MessageViewDTO>
                {
                    status = 1,
                    msg = "Unauthorized"
                };

            //Getting messages from Database
            IQueryable<Message> messageQuery = from m in _db.Messages
                                               where m.Session.SessionID == sessionID
                                               select m;

            //Filtering by search Term
            if (!string.IsNullOrEmpty(searchTerm)) messageQuery = messageQuery.Where(e => e.Content.Contains(searchTerm));

            //filter by start and end date
            DateTime validStartDate, validEndDate;
            if (DateTime.TryParse(startDate, out validStartDate))
            {
                messageQuery = messageQuery.Where(m => m.Timestamp > validStartDate);
            }
            if (DateTime.TryParse(endDate, out validEndDate))
            {
                messageQuery = messageQuery.Where(m => m.Timestamp < validEndDate);
            }
            //Order by given column
            if (!string.IsNullOrEmpty(sortColumn))
            {
                Expression<Func<Message, object>> keySelector = sortColumn.ToLower() switch // throws error when sortColumn is null
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
                                        .Select(m => new MessageViewDTO
                                        {
                                            SenderID = m.Sender.Id,
                                            SessionID = m.Session.SessionID,
                                            MessageID = m.MessageID,
                                            Content = m.Content,
                                            IsRead = m.IsRead,
                                            Timestamp = m.Timestamp
                                        });

            //Making the result as a paged list
            var messages = await PagedList<MessageViewDTO>.CreateAsync(messageResponse, page, pageSize);
            return new GettersResponse<MessageViewDTO>
            {
                status = 2,
                msg = "Successful",
                Data = messages
            };
        }
        public async Task<GettersResponse<UserViewDTO>> GetUsersOfSession(ClaimsPrincipal User,Guid sessionID, int page, int pageSize)//The sessions tree in itself needs a big change man fr
        {
            var session = await _db.Sessions.Include(s => s.Users).FirstOrDefaultAsync(s => s.SessionID == sessionID);
            if (session == null)
                return new GettersResponse<UserViewDTO>
                {
                    status = 0,
                    msg = "Session not found"
                };

            //Creating list of UserIDs for authorization
            List<Guid> UserIDs = new List<Guid>();
            foreach (var UsersID in session.Users) UserIDs.Add(UsersID.Id);

            //Authroization check
            var auhtResult = await _authorizationService.AuthorizeAsync(User, UserIDs, "ListUserPolicy");
            if (!auhtResult.Succeeded)
                return new GettersResponse<UserViewDTO>
                {
                    status = 1,
                    msg = "Unauthorized"
                };

            //Projecting the users to UserDTO
            var sessionQuery = from s in _db.Sessions
                               from u in s.Users
                               where s.SessionID == sessionID
                               select new UserViewDTO
                               {
                                   Id = u.Id,
                                   Name = u.Name,
                                   Email = u.Email,
                                   UserType = u.UserType
                               };

            //Making the result as a paged list
            var sessionUsers = await PagedList<UserViewDTO>.CreateAsync(sessionQuery, page, pageSize);
            return new GettersResponse<UserViewDTO>
            {
                status = 2,
                msg = "Successful",
                Data = sessionUsers
            };
        }
        public async Task<GettersResponse<SessionViewDTO>> GetAllSessions(int page,int pageSize)
        {
            //Getting all sessions from Database and projecting them to SessionDTO
            var sessionsQuery = from s in _db.Sessions
                               select new SessionViewDTO
                               {
                                   SessionID = s.SessionID,
                                   StartTime = s.StartTime,
                                   UserIDs = s.Users.Select(u => u.Id).ToList(),
                               };

            if (sessionsQuery == null || sessionsQuery.Count() == 0)
                return new GettersResponse<SessionViewDTO>
                {
                    status = 0,
                    msg = "No sessions were found"
                };
            var sessions = await PagedList<SessionViewDTO>.CreateAsync(sessionsQuery, page, pageSize);
            return new GettersResponse<SessionViewDTO>
            {
                status = 2,
                msg = "Successful",
                Data = sessions
            };
        }
    }
}
