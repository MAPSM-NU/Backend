using Gym_App.Application.Interfaces;
using Gym_App.Domain;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.DTOs.Message;
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
        public MessageService(DbBase db,IAuthorizationService authorizationService)
        {
            _db = db;
            _authorizationService = authorizationService;
        }

        //        *********** Setters ***********

        public async Task<int> AddMessage(ClaimsPrincipal User,Guid senderID, MessageCreationDTO message)//0 == Invalid DTO || 1 == User not found || 2 == Unauthorized ||
                                                                                                         //3 == Session not found || 4 == User not in session || 5 == Success
        {
            //checking for DTO validity
            if (message == null)
                return 0;

            //Getting user from Database
            var user = await (from u in _db.Users
                              where u.UserID == senderID
                              select u).FirstOrDefaultAsync();
            //if user not found return 
            if (user == null)
                return 1;

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, user.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return 2;

            //Getting session from Database
            var session = await (from s in _db.Sessions.Include(s => s.Users)
                                 where s.SessionID == message.SessionID
                                 select s).FirstOrDefaultAsync();
            //if session not found return
            if (session == null)
                return 3;
            //Checking if user is part of the session
            if (!session.Users.Contains(user))
                return 4;

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
            return 5;
        }
        public async Task<int> UpdateMessage(ClaimsPrincipal User,Guid messageID, MessageUpdateDTO message)//0 == Invalid DTO || 1 == Message not found || 2 == Unauthorized || 3 == Success
        {
            //checking for DTO validity
            if (message == null)
                return 0;

            //Getting message from Database
            var Message = await (from u in _db.Messages.Include(m=>m.Sender)
                                 where u.MessageID == messageID
                                 select u).FirstOrDefaultAsync();
            //if message not found return
            if (Message == null) 
                return 1;

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, Message.Sender.UserID, "SameUserPolicy");
            if(!authResult.Succeeded)
                return 2;

            //Updating message
            Message.Content = message.Content;
            Message.IsRead = message.IsRead;

            //Saving to Database
            await _db.SaveChangesAsync();
            return 3;
        }
        public async Task<int> DeleteMessage(ClaimsPrincipal User, Guid messageID)//0 == Invalid messageID || 1 == Message not found || 2 == Unauthorized || 3 == Success
        {
            //checking for messageID validity
            if (messageID == Guid.Empty)
                return 0;

            var Message = await (from u in _db.Messages.Include(m=>m.Sender)
                          where u.MessageID == messageID
                          select u).FirstOrDefaultAsync();
            //if message not found return
            if (Message == null)
                return 1;

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, Message.Sender.UserID, "SameUserPolicy");
            if(!authResult.Succeeded)
                return 2;

            //Saving to Database
            _db.Messages.Remove(Message);
            await _db.SaveChangesAsync();
            return 3;
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
        public async Task<PagedList<MessageMiniViewDTO>?> GetSessionMessages(ClaimsPrincipal User, Guid sessionID, string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize)
        {
            var session = await (from s in _db.Sessions.Include(s => s.Users)
                                 where s.SessionID == sessionID
                                 select s).FirstOrDefaultAsync();
            //if session not found return
            if (session == null)
                return null;
            var UserIDs = (from u in session.Users
                           select u.UserID).ToList();
            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, UserIDs, "ListUserPolicy");
            if (!authResult.Succeeded)
                return null;

            //if page or pageSize are 0 set default values
            if (page == 0) page = 1;
            if (pageSize == 0) pageSize = 10;


            //Getting messages from Database
            var messageQuery = from s in _db.Sessions
                               from m in _db.Messages
                               where s.SessionID == sessionID && s.Messages.Contains(m)
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
            return messages;
        }
        public async Task<PagedList<MessageViewDTO>> GetMessagesByFilter(string startDate,string endDate,int page, string sortColumn, string OrderBy, string searchTerm, int pageSize)//will be accessed by admin only
                                                                                                                                                                                  //so there is no need for authorization
        {
            //if page or pageSize are 0 set default values
            if (page == 0) page = 1;
            if(pageSize == 0)pageSize = 10;

            //Getting messages from Database
            IQueryable<Message> messageQuery = _db.Messages;

            //filtering by start date and end date
            DateTime validStartDate, validEndDate;
            if(DateTime.TryParse(startDate, out validStartDate))
            {
                messageQuery = messageQuery.Where(m => m.Timestamp > validStartDate);
            }
            if (DateTime.TryParse(endDate, out validEndDate))
            {
                messageQuery = messageQuery.Where(m=>m.Timestamp < validEndDate);
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
                if (!string.IsNullOrEmpty(OrderBy))messageQuery = messageQuery.OrderBy(keySelector);

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
            var messages = await PagedList<MessageViewDTO>.CreateAsync(messageResponse,page,pageSize);
            return messages;
        }
        public async Task<PagedList<MessageViewDTO>> GetMessages(int page, int pageSize)
        {
            //if page or pageSize are 0 set default values
            if (page == 0) page = 1;
            if (pageSize == 0) pageSize = 10;

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

            //Making the result as a paged list
            var messages = await PagedList<MessageViewDTO>.CreateAsync(messagesQuery, page, pageSize);
            return messages;
        }

    }
}
