using Gym_App.Domain;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.DTOs.Message;
using Gym_App.Infastructure.DTOs.Session;
using Gym_App.Infastructure.DTOs.UserDTOs;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infastructure.Transfer_Classes;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Gym_App.Application.Services
{
    public class SessionService : ISessionService
    {
        private readonly ISessionRepositry _sessionRepositry;
        private readonly IMessageRepositry _messageRepositry;
        private readonly IUserRepositry _userRepositry;
        private readonly IAuthorizationService _authorizationService;

        //        *********** Setters ***********

        //0 == Error(Bad Request) || 1 == Unauthorized (Forbid) || 2 == Success (Ok)
        public SessionService(
            ISessionRepositry sessionRepositry,
            IMessageRepositry messageRepositry,
            IUserRepositry userRepositry,
            IAuthorizationService authorizationService)
        {
            _sessionRepositry = sessionRepositry;
            _messageRepositry = messageRepositry;
            _userRepositry = userRepositry;
            _authorizationService = authorizationService;
        }

        public async Task<SettersResponse> CreateSession(ClaimsPrincipal User, Guid user1, Guid user2)
        {
            //checking the validity of the DTO
            if (user1 == Guid.Empty || user2 == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid user IDs" };

            List<Guid> userIDs = new List<Guid> { user1, user2 };

            //Authorization check
            var authResult = await _authorizationService.AuthorizeAsync(User, userIDs, "ListUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Finding the users from the repository
            List<User> users = new List<User>();
            foreach (var ID in userIDs)
            {
                var user = await _userRepositry.GetById(ID);
                if (user == null)
                    return new SettersResponse { status = 0, msg = "User not found" };
                else
                {
                    users.Add(user);
                }
            }

            //Creating the session
            var Session = new Session
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                SessionType = " ",//for now leave like this
                Users = users,
                Messages = new List<Message>()
            };

            //Saving to Database via repository
            await _sessionRepositry.Create(Session);
            return new SettersResponse { status = 2, msg = "Session created successfully" };
        }

        public async Task<SettersResponse> DeleteSession(ClaimsPrincipal User, Guid sessionID)
        {
            //checking the validity of the given Guid
            if (sessionID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid session ID" };

            //Getting the session with users from repository
            var session = await _sessionRepositry.GetSessionWithUsers(sessionID);

            //Return if session was not found
            if (session == null)
                return new SettersResponse { status = 0, msg = "Session not found" };

            //Creating list of UserIDs for authorization
            List<Guid> UserIDs = session.Users.Select(u => u.Id).ToList();

            //Authorization check
            var authResult = await _authorizationService.AuthorizeAsync(User, UserIDs, "ListUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Removing from Database via repository
            await _sessionRepositry.Delete(sessionID);
            return new SettersResponse { status = 2, msg = "Session deleted successfully" };
        }

        public async Task<SettersResponse> AddMessages(ClaimsPrincipal User, Guid sessionID, SessionMessagesDTO sessionMessages)
        {
            //checking the validity of the DTO
            if (sessionMessages == null || sessionID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid session ID or messages" };

            //Getting the session with users and messages from repository
            var Session = await _sessionRepositry.GetSessionWithUsers(sessionID);

            //Return if session was not found
            if (Session == null)
                return new SettersResponse { status = 0, msg = "Session not found" };

            //Creating list of UserIDs for authorization
            List<Guid> UserIDs = Session.Users.Select(u => u.Id).ToList();

            //Authorization check
            var authResult = await _authorizationService.AuthorizeAsync(User, UserIDs, "ListUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //checking the messages to add
            List<Message> messagesToAdd;
            if (Session.Messages == null || Session.Messages.Count == 0)
            {
                //If session has no messages, add all given messages if their IDs point to actual messages in the database
                var messageIds = sessionMessages.messagesID ?? new List<Guid>();
                messagesToAdd = new List<Message>();
                
                foreach (var messageId in messageIds)
                {
                    var message = await _messageRepositry.GetMessageById(messageId);
                    if (message != null)
                        messagesToAdd.Add(message);
                }
                
                Session.Messages = new List<Message>();
            }
            else
            {
                //If session has messages, only add the new ones if their IDs point to actual messages in the database
                var existingMessagesIDs = new HashSet<Guid>(Session.Messages.Select(m => m.Id));
                var messagesIDsToAdd = sessionMessages.messagesID?.Where(id => !existingMessagesIDs.Contains(id)).ToList();
                
                if (messagesIDsToAdd == null || messagesIDsToAdd.Count == 0)
                    return new SettersResponse { status = 0, msg = "No new messages to add" };
                
                messagesToAdd = new List<Message>();
                foreach (var messageId in messagesIDsToAdd)
                {
                    var message = await _messageRepositry.GetMessageById(messageId);
                    if (message != null)
                        messagesToAdd.Add(message);
                }
            }

            //return if no messages were found in the repository
            if (messagesToAdd == null || messagesToAdd.Count == 0)
                return new SettersResponse { status = 0, msg = "No messages found in the database" };

            //Adding the messages to the session
            foreach (var message in messagesToAdd)
            {
                Session.Messages.Add(message);
            }

            //Saving to Database via repository
            await _sessionRepositry.Update(Session);
            return new SettersResponse { status = 2, msg = "Messages added successfully" };
        }

        public async Task<SettersResponse> DeleteMessages(ClaimsPrincipal User, Guid sessionID, SessionMessagesDTO sessionMessages)
        {
            //checking the validity of the DTO
            if (sessionMessages == null || sessionID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid session ID or messages" };

            //Getting the session with users and messages from repository
            var Session = await _sessionRepositry.GetSessionWithUsers(sessionID);

            //Return if session was not found
            if (Session == null)
                return new SettersResponse { status = 0, msg = "Session not found" };

            //Creating list of UserIDs for authorization
            List<Guid> UserIDs = Session.Users.Select(u => u.Id).ToList();

            //Authorization check
            var authResult = await _authorizationService.AuthorizeAsync(User, UserIDs, "ListUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Getting Sender ID
            var senderID = GettingSenderID(User);
            if (senderID == Guid.Empty)
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
                var existingMessagesIDs = new HashSet<Guid>(Session.Messages.Select(m => m.Id));
                var messagesIDsToRemove = sessionMessages.messagesID.Where(id => existingMessagesIDs.Contains(id)).ToList();
                
                if (messagesIDsToRemove == null || messagesIDsToRemove.Count == 0)
                    return new SettersResponse { status = 0, msg = "No messages found in the database" };
                
                messagesToRemove = new List<Message>();
                foreach (var messageId in messagesIDsToRemove)
                {
                    var message = await _messageRepositry.GetMessageById(messageId);
                    if (message != null)
                        messagesToRemove.Add(message);
                }
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

            //Saving to Database via repository
            await _sessionRepositry.Update(Session);
            return new SettersResponse { status = 2, msg = "Messages removed successfully" };
        }

        //-----------------------------------------------------------------------


        //        *********** Extra Validation Function ***********

        public async Task<bool> isMessageBelongUser(Guid messageID, Guid userID)
        {
            var message = await _messageRepositry.GetMessageById(messageID);
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

        public async Task<List<Guid>?> GetSessionUsersIDs(ClaimsPrincipal User, Guid sessionID)
        {
            //Getting the users of the session from repository
            var Users = await _sessionRepositry.GetSessionUsers(sessionID);

            //Return null if session not found
            if (Users == null || !Users.Any())
                return null;

            //Creating list of UserIDs for authorization
            var UserIDs = Users.Select(u => u.Id).ToList();

            //Authorization check
            var authResult = await _authorizationService.AuthorizeAsync(User, UserIDs, "ListUserPolicy");
            if (!authResult.Succeeded)
                return null;

            return UserIDs;
        }

        public async Task<GettersResponse<MessageViewDTO>> GetSessionMessages(
            ClaimsPrincipal User,
            Guid sessionID,
            string startDate,
            string endDate,
            int page,
            string sortColumn,
            string OrderBy,
            string searchTerm,
            int pageSize)
        {
            //Getting the session with users from repository
            var Session = await _sessionRepositry.GetSessionWithUsers(sessionID);
            
            //Return null if session not found
            if (Session == null)
                return new GettersResponse<MessageViewDTO>
                {
                    status = 0,
                    msg = "Session not found"
                };

            //Creating list of UserIDs for authorization
            List<Guid> UserIDs = Session.Users.Select(u => u.Id).ToList();

            //Authorization check
            var authResult = await _authorizationService.AuthorizeAsync(User, UserIDs, "ListUserPolicy");
            if (!authResult.Succeeded)
                return new GettersResponse<MessageViewDTO>
                {
                    status = 1,
                    msg = "Unauthorized"
                };

            //Getting messages from repository
            var messageQuery = _messageRepositry.GetSessionMessagesQueryable(sessionID);

            //Filtering by search Term
            if (!string.IsNullOrEmpty(searchTerm))
                messageQuery = _messageRepositry.Search(searchTerm, messageQuery);

            //filter by start and end date
            DateTime validStartDate, validEndDate;
            if (DateTime.TryParse(startDate, out validStartDate) && DateTime.TryParse(endDate, out validEndDate))
                messageQuery = _messageRepositry.FilterDate(validStartDate, validEndDate, messageQuery);

            //Order by given column
            if (!string.IsNullOrEmpty(sortColumn))
                messageQuery = _messageRepositry.FilterSortColumn(sortColumn, OrderBy, messageQuery);

            //Projecting the resultant message queries to messageDTO
            var messageResponse = messageQuery
                .Select(m => new MessageViewDTO
                {
                    SenderID = m.Sender.Id,
                    SessionID = m.Session.Id,
                    MessageID = m.Id,
                    Content = m.Content,
                    IsRead = m.IsRead,
                    Timestamp = m.CreatedAt
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

        public async Task<GettersResponse<UserViewDTO>> GetUsersOfSession(ClaimsPrincipal User, Guid sessionID, int page, int pageSize)
        {
            //Getting session with users from repository
            var session = await _sessionRepositry.GetSessionWithUsers(sessionID);
            
            if (session == null)
                return new GettersResponse<UserViewDTO>
                {
                    status = 0,
                    msg = "Session not found"
                };

            //Creating list of UserIDs for authorization
            List<Guid> UserIDs = session.Users.Select(u => u.Id).ToList();

            //Authorization check
            var authResult = await _authorizationService.AuthorizeAsync(User, UserIDs, "ListUserPolicy");
            if (!authResult.Succeeded)
                return new GettersResponse<UserViewDTO>
                {
                    status = 1,
                    msg = "Unauthorized"
                };

            //Projecting the users to UserDTO
            var sessionQuery = session.Users
                .Select(u => new UserViewDTO
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    UserType = u.UserType
                })
                .AsQueryable();

            //Making the result as a paged list
            var sessionUsers = await PagedList<UserViewDTO>.CreateAsync(sessionQuery, page, pageSize);
            return new GettersResponse<UserViewDTO>
            {
                status = 2,
                msg = "Successful",
                Data = sessionUsers
            };
        }

        public async Task<GettersResponse<SessionViewDTO>> GetAllSessions(int page, int pageSize)
        {
            //Getting all sessions from repository
            var sessionsQuery = _sessionRepositry.GetAll()
                .Select(s => new SessionViewDTO
                {
                    SessionID = s.Id,
                    StartTime = s.CreatedAt,
                    UserIDs = s.Users.Select(u => u.Id).ToList(),
                });

            if (sessionsQuery == null || !sessionsQuery.Any())
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
