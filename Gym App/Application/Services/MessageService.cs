using Gym_App.Domain;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.DTOs.Message;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infastructure.Transfer_Classes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Gym_App.Application.Services
{
    public class MessageService : IMessageService
    {
        private readonly DbBase _db;
        private readonly IAuthorizationService _authorizationService;
        public MessageService(DbBase db, IAuthorizationService authorizationService)
        {
            _db = db;
            _authorizationService = authorizationService;
        }

        //        *********** Setters ***********

        //0 == Error(Bad Request) || 1 == Unauthorized (Forbid) || 2 == Success (Ok)
        public async Task<SettersResponse> AddMessage(ClaimsPrincipal User, Guid senderID, MessageCreationDTO message)
        {
            //checking for DTO validity
            if (message == null)
                return new SettersResponse { status = 0, msg = "Invalid DTO" };

            //Getting user from Database
            var user = await (from u in _db.Users
                              where u.UserID == senderID
                              select u).FirstOrDefaultAsync();
            //if user not found return 
            if (user == null)
                return new SettersResponse { status = 0, msg = "User not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, user.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Getting session from Database
            var session = await (from s in _db.Sessions.Include(s => s.Users)
                                 where s.SessionID == message.SessionID
                                 select s).FirstOrDefaultAsync();
            //if session not found return
            if (session == null)
                return new SettersResponse { status = 0, msg = "Session not found" };
            //Checking if user is part of the session
            if (!session.Users.Contains(user))
                return new SettersResponse { status = 0, msg = "Invalid DTO" };

            //Creating new message
            var newMessage = new Message
            {
                MessageID = Guid.NewGuid(),
                Sender = user,
                Session = session,
                Content = message.Content,
                Timestamp = DateTime.UtcNow,
                IsRead = message.IsRead
            };

            //Saving to Database
            await _db.Messages.AddAsync(newMessage);
            await _db.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Success" };
        }
        public async Task<SettersResponse> UpdateMessage(ClaimsPrincipal User, Guid messageID, MessageUpdateDTO message)
        {
            //checking for DTO validity
            if (message == null)
                return new SettersResponse { status = 0, msg = "Invalid DTO" };

            //Getting message from Database
            var Message = await (from u in _db.Messages.Include(m => m.Sender)
                                 where u.MessageID == messageID
                                 select u).FirstOrDefaultAsync();
            //if message not found return
            if (Message == null)
                return new SettersResponse { status = 0, msg = "Message not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, Message.Sender.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Updating message
            Message.Content = message.Content;
            Message.IsRead = message.IsRead;

            //Saving to Database
            await _db.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Success" };
        }
        public async Task<SettersResponse> DeleteMessage(ClaimsPrincipal User, Guid messageID)
        {
            //checking for messageID validity
            if (messageID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid messageID" };

            var Message = await (from u in _db.Messages.Include(m => m.Sender)
                                 where u.MessageID == messageID
                                 select u).FirstOrDefaultAsync();
            //if message not found return
            if (Message == null)
                return new SettersResponse { status = 0, msg = "Message not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, Message.Sender.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Saving to Database
            _db.Messages.Remove(Message);
            await _db.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Success" };
        }

        //-----------------------------------------------------------------------

        //        *********** Getters ***********

        public async Task<Guid> GetMessageUserID(Guid messageID)//Not used anymore
        {
            var userID = await (from m in _db.Messages
                                where m.MessageID == messageID
                                select m.Sender.UserID).FirstOrDefaultAsync();
            return userID;
        }
        public async Task<List<Guid>?> GetSessionUsersIDs(ClaimsPrincipal User, Guid sessionID)
        {
            //checking for sessionID validity
            if (sessionID == Guid.Empty)
                return null;

            //Getting users from Database
            var Users = await (from s in _db.Sessions
                               where s.SessionID == sessionID
                               select s.Users).FirstOrDefaultAsync();

            //if session not found return
            if (Users == null)
                return null;

            var UserIDs = (from u in Users
                           select u.UserID).ToList();

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, UserIDs, "ListUserPolicy");
            if (!authResult.Succeeded)
                return null;

            return UserIDs;
        }
        public async Task<GettersResponse<MessageMiniViewDTO>> GetSessionMessages(ClaimsPrincipal User, Guid sessionID, string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize)
        {
            var session = await (from s in _db.Sessions.Include(s => s.Users)
                                 where s.SessionID == sessionID
                                 select s).FirstOrDefaultAsync();
            //if session not found return
            if (session == null)
                return new GettersResponse<MessageMiniViewDTO>
                {
                    status = 0,
                    msg = "Session not found"
                };

            var UserIDs = (from u in session.Users
                           select u.UserID).ToList();
            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, UserIDs, "ListUserPolicy");
            if (!authResult.Succeeded)
                return new GettersResponse<MessageMiniViewDTO>
                {
                    status = 1,
                    msg = "Unauthorized"
                };

            //Getting messages from Database
            var messageQuery = from s in _db.Sessions
                               from m in _db.Messages
                               where s.SessionID == sessionID && s.Messages!.Contains(m)
                               select m;

            //filtering by start date and end date
            DateTime validStartDate, validEndDate;
            if (DateTime.TryParse(startDate, out validStartDate))
            {
                messageQuery = messageQuery.Where(m => m.Timestamp > validStartDate);
            }
            if (DateTime.TryParse(endDate, out validEndDate))
            {
                messageQuery = messageQuery.Where(m => m.Timestamp < validEndDate);
            }

            //filtering by search term
            if (!string.IsNullOrEmpty(searchTerm)) messageQuery = messageQuery.Where(m => m.Content.Contains(searchTerm));//Might add user name belmara bs will wait

            //ordering by a given column
            if (!string.IsNullOrEmpty(sortColumn))//order by custom column 
            {
                Expression<Func<Message, object>> keySelector = sortColumn.ToLower() switch // throws error when sortColumn is null
                {
                    "message" or "m" or "c" or "content" => Message => Message.Content, //ordering by message content
                    "time" or "t" or "timestamp" => Message => Message.Timestamp, // ordering by timestamp
                    _ => Message => Message.MessageID, //failsafe: ordering by message ID
                };
                //If no orderby was inputed, then we sort ascending
                if (!string.IsNullOrEmpty(OrderBy)) messageQuery = messageQuery.OrderBy(keySelector);

                //else if anything was inputted we sort descending
                else messageQuery = messageQuery.OrderByDescending(keySelector);
            }
            else
            {
                messageQuery = messageQuery.OrderByDescending(m => m.Timestamp);// or just order by recent messages
            }

            //Projecting the resultant message queries to messageDTO
            var messageResponse = messageQuery
                                    .Select(m => new MessageMiniViewDTO
                                    {
                                        SenderID = m.Sender.UserID,
                                        MessageID = m.MessageID,
                                        Content = m.Content,
                                        IsRead = m.IsRead,
                                        Timestamp = m.Timestamp
                                    });

            //Making the result as a paged list
            var messages = await PagedList<MessageMiniViewDTO>.CreateAsync(messageResponse, page, pageSize);
            return new GettersResponse<MessageMiniViewDTO>
            {
                status = 2,
                msg = "Successful",
                Data = messages
            };
        }
        public async Task<GettersResponse<MessageViewDTO>> GetMessagesByFilter(string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize)//will be accessed by admin only
                                                                                                                                                                                              //so there is no need for authorization
        {

            //Getting messages from Database
            IQueryable<Message> messageQuery = _db.Messages;
            
            if (messageQuery == null || messageQuery.Count() == 0)
                return new GettersResponse<MessageViewDTO>
                {
                    status = 0,
                    msg = "No messages in Database"
                };

            //filtering by start date and end date
            DateTime validStartDate, validEndDate;
            if (DateTime.TryParse(startDate, out validStartDate))
            {
                messageQuery = messageQuery.Where(m => m.Timestamp > validStartDate);
            }
            if (DateTime.TryParse(endDate, out validEndDate))
            {
                messageQuery = messageQuery.Where(m => m.Timestamp < validEndDate);
            }

            //filtering by search term
            if (!string.IsNullOrEmpty(searchTerm)) messageQuery = messageQuery.Where(m => m.Content.Contains(searchTerm));//Might add user name belmara bs will wait

            //ordering by a given column
            if (!string.IsNullOrEmpty(sortColumn))
            {
                Expression<Func<Message, object>> keySelector = sortColumn.ToLower() switch // throws error when sortColumn is null
                {
                    "message" or "content" => Message => Message.Content,//ordering by message content
                    _ => Message => Message.MessageID,//failsafe: ordering by message ID
                };
                //If no orderby was inputed, then we sort ascending
                if (!string.IsNullOrEmpty(OrderBy)) messageQuery = messageQuery.OrderBy(keySelector);

                //else if anything was inputted we sort descending
                else messageQuery = messageQuery.OrderByDescending(keySelector);
            }

            //Projecting the resultant message queries to messageDTO
            var messageResponse = messageQuery
                                    .Select(m => new MessageViewDTO
                                    {
                                        SenderID = m.Sender.UserID,
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
        public async Task<GettersResponse<MessageViewDTO>> GetMessages(int page, int pageSize)
        {

            //Getting messages from Database
            var messagesQuery = from m in _db.Messages
                                select new MessageViewDTO
                                {
                                    SenderID = m.Sender.UserID,
                                    SessionID = m.Session.SessionID,
                                    MessageID = m.MessageID,
                                    Content = m.Content,
                                    IsRead = m.IsRead,
                                    Timestamp = m.Timestamp
                                };

            if (messagesQuery == null || messagesQuery.Count() == 0)
                return new GettersResponse<MessageViewDTO>
                {
                    status = 0,
                    msg = "No messages in Database"
                };

            //Making the result as a paged list
            var messages = await PagedList<MessageViewDTO>.CreateAsync(messagesQuery, page, pageSize);
            return new GettersResponse<MessageViewDTO>
            {
                status = 2,
                msg = "Successful",
                Data = messages
            };
        }

    }
}
