using Gym_App.Application.Authorization;
using Gym_App.Domain;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.DTOs.Message;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infastructure.Transfer_Classes;

namespace Gym_App.Application.Services
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICachedAuthorizationService _authorizationService;

        public MessageService(IUnitOfWork unitOfWork,ICachedAuthorizationService authorizationService)
        {
            _unitOfWork = unitOfWork;
            _authorizationService = authorizationService;
        }

        //        *********** Setters ***********

        //0 == Error(Bad Request) || 1 == Unauthorized (Forbid) || 2 == Success (Ok)
        public async Task<SettersResponse> AddMessage(Guid senderID, MessageCreationDTO message)
        {
            //checking for DTO validity
            if (message == null || message.SessionID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid DTO" };

            //Getting user from repository
            var user = await _unitOfWork.Users.GetById(senderID);
            //if user not found return 
            if (user == null)
                return new SettersResponse { status = 0, msg = "User not found" };

            //Authorization
            var authResult = await _authorizationService.IsUserAsync(user.Id);
            if (!authResult)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Getting session with users included from repository
            var session = await _unitOfWork.Sessions.GetSessionWithUsers(message.SessionID);
            if (session == null)
                return new SettersResponse { status = 0, msg = "Session not found" };
            
            //Checking if user is part of the session
            if (!session.Users.Any(u => u.Id == user.Id))
                return new SettersResponse { status = 1, msg = "User is not part of this session" };

            //Creating new message
            var newMessage = new Message
            {
                Id = Guid.NewGuid(),
                Sender = user,
                Content = message.Content,
                CreatedAt = DateTime.UtcNow,
                IsRead = message.IsRead,
                Session = session
            };

            //Saving to Database via repository
            await _unitOfWork.Messages.Create(newMessage);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Success" };
        }

        public async Task<SettersResponse> UpdateMessage(Guid messageID, MessageUpdateDTO message)
        {
            //checking for DTO validity
            if (message == null)
                return new SettersResponse { status = 0, msg = "Invalid DTO" };

            //Getting message from repository
            var messageEntity = await _unitOfWork.Messages.GetMessageById(messageID);
            //if message not found return
            if (messageEntity == null)
                return new SettersResponse { status = 0, msg = "Message not found" };

            //Authorization
            var authResult = await _authorizationService.IsUserAsync(messageEntity.Sender.Id);
            if (!authResult)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Updating message
            messageEntity.Content = message.Content;
            messageEntity.IsRead = message.IsRead;

            //Saving to Database via repository
            await _unitOfWork.Messages.Update(messageEntity);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Success" };
        }

        public async Task<SettersResponse> DeleteMessage(Guid messageID)
        {
            //checking for messageID validity
            if (messageID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid messageID" };

            //Getting message from repository
            var messageEntity = await _unitOfWork.Messages.GetMessageById(messageID);
            //if message not found return
            if (messageEntity == null)
                return new SettersResponse { status = 0, msg = "Message not found" };

            //Authorization
            var authResult = await _authorizationService.IsUserAsync(messageEntity.Sender.Id);
            if (!authResult)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Deleting from Database via repository
            await _unitOfWork.Messages.Delete(messageID);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Success" };
        }

        //-----------------------------------------------------------------------

        //        *********** Getters ***********

        public async Task<Guid> GetMessageUserID(Guid messageID)
        {
            var messageEntity = await _unitOfWork.Messages.GetMessageById(messageID);
            return messageEntity?.Sender.Id ?? Guid.Empty;
        }

        public async Task<List<Guid>?> GetSessionUsersIDs(Guid sessionID)
        {
            //checking for sessionID validity
            if (sessionID == Guid.Empty)
                return null;

            //Note: This requires ISessionRepositry implementation
            var Users = await _unitOfWork.Sessions.GetSessionUsers(sessionID);
            if (Users == null)
                return null;

            var UserIDs = Users.Select(u => u.Id).ToList();
            //Authorization
            var authResult = await _authorizationService.IsInListAsync(UserIDs);
            if (!authResult)
                return null;

            return UserIDs;
        }

        public async Task<GettersResponse<MessageMiniViewDTO>> GetSessionMessages(
            Guid sessionID,
            string startDate,
            string endDate,
            int page,
            string sortColumn,
            string OrderBy,
            string searchTerm,
            int pageSize)
        {
            //Note: This requires ISessionRepositry implementation
            var users = await _unitOfWork.Sessions.GetSessionUsers(sessionID);
            if (users == null || !users.Any())
                return new GettersResponse<MessageMiniViewDTO>
                {
                    status = 0,
                    msg = "Session not found"
                };

            var UserIDs = users.Select(u => u.Id).ToList();
            var authResult = await _authorizationService.IsInListAsync(UserIDs);
            if (!authResult)
                return new GettersResponse<MessageMiniViewDTO>
                {
                    status = 1,
                    msg = "Unauthorized"
                };

            //Getting messages from repository
            var messageQuery = _unitOfWork.Messages.GetSessionMessagesQueryable(sessionID);


            if (DateTime.TryParse(startDate, out DateTime parsedStartDate) && DateTime.TryParse(endDate, out DateTime parsedEndDate))
                messageQuery = _unitOfWork.Messages.FilterDate(parsedStartDate, parsedEndDate, messageQuery);
            //filtering by search term
            if (!string.IsNullOrEmpty(searchTerm))
                messageQuery = _unitOfWork.Messages.Search(searchTerm, messageQuery);

            //ordering by a given column
            if (!string.IsNullOrEmpty(sortColumn))
                messageQuery = _unitOfWork.Messages.FilterSortColumn(sortColumn, OrderBy, messageQuery);    
            //Projecting the resultant message queries to messageDTO
            var messageResponse = messageQuery.Select(m => new MessageMiniViewDTO
            {
                SenderID = m.Sender.Id,
                MessageID = m.Id,
                Content = m.Content,
                IsRead = m.IsRead,
                Timestamp = m.CreatedAt
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

        public async Task<GettersResponse<MessageViewDTO>> GetMessagesByFilter(
            string startDate,
            string endDate,
            int page,
            string sortColumn,
            string OrderBy,
            string searchTerm,
            int pageSize)
        {
            //Getting messages from repository
            var messageQuery = _unitOfWork.Messages.GetAllMessagesQueryable();

            if (messageQuery == null || !messageQuery.Any())
                return new GettersResponse<MessageViewDTO>
                {
                    status = 0,
                    msg = "No messages in Database"
                };

            if (DateTime.TryParse(startDate, out DateTime parsedStartDate) && DateTime.TryParse(endDate, out DateTime parsedEndDate))
                messageQuery = _unitOfWork.Messages.FilterDate(parsedStartDate, parsedEndDate, messageQuery);

            //filtering by search term
            if (!string.IsNullOrEmpty(searchTerm))
                messageQuery = _unitOfWork.Messages.Search(searchTerm, messageQuery);

            //ordering by a given column
            if (!string.IsNullOrEmpty(sortColumn))
                messageQuery = _unitOfWork.Messages.FilterSortColumn(sortColumn, OrderBy, messageQuery);
            //Projecting the resultant message queries to messageDTO
            var messageResponse = messageQuery.Select(m => new MessageViewDTO
            {
                SenderID = m.Sender.Id,
                SessionID = m.Session.Id,
                MessageID = m.Id,
                Name = m.Sender.Name,
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

        public async Task<GettersResponse<MessageViewDTO>> GetMessages(int page, int pageSize)
        {
            //Getting messages from repository
            var messagesQuery = _unitOfWork.Messages.GetAllMessagesQueryable()
                .Select(m => new MessageViewDTO
                {
                    SenderID = m.Sender.Id,
                    SessionID = m.Session.Id,
                    MessageID = m.Id,
                    Name = m.Sender.Name,
                    Content = m.Content,
                    IsRead = m.IsRead,
                    Timestamp = m.CreatedAt
                });

            if (messagesQuery == null || !messagesQuery.Any())
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
