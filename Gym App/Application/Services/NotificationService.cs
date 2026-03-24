using Gym_App.Domain;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.DTOs.Notification;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infastructure.Transfer_Classes;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Gym_App.Application.Services
{

    public class NotificationService : INotificationService // Fuck this class. Needs a change again. relationship with users should not be many to many GRAAAAAA
    {
        private readonly INotificationRepositry _notificationRepositry;
        private readonly IUserRepositry _userRepositry;
        private readonly IAuthorizationService _authorizationService;
        public NotificationService(INotificationRepositry notificationRepositry, IAuthorizationService authorizationService, IUserRepositry userRepositry)
        {
            _notificationRepositry = notificationRepositry;
            _authorizationService = authorizationService;
            _userRepositry = userRepositry;
        }

        //        *********** Setters ***********

        //0 == Error(Bad Request) || 1 == Unauthorized (Forbid) || 2 == Success (Ok)

        public Task<SettersResponse> SendNotificationAsync(NotificationViewDTO notification)
        {
            throw new NotImplementedException();
        }
        public async Task<SettersResponse> CreateNotification(ClaimsPrincipal User, Guid userID, NotificationCreationDTO notification)
        {
            //Checking for DTO validity
            if (notification == null)
                return new SettersResponse { status = 0, msg = "Faulty DTO" };

            //Getting User from Database
            var user = await _userRepositry.GetById(userID);
            //If user not found return 
            if (user == null)
                return new SettersResponse { status = 0, msg = "User not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, user.Id, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Faulty DTO" };

            //Creating Notification
            Notification newNotification = new Notification
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                Title = notification.Title,
                Content = notification.Content,
                User = user,
            };

            //Saving to the Database
            await _notificationRepositry.Create(newNotification);
            return new SettersResponse { status = 2, msg = "Successful" };

        }
        public async Task<SettersResponse> DeleteNotification(ClaimsPrincipal User, Guid NotificationID)//0 == Invalid NotificationID || 1 == Notification not found || 2 == Unauthorized || 3 == Successful
        {
            //Checking for NotificationID validty
            if (NotificationID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid NotificationID" };

            //Getting notification from Database
            var Notification = await _notificationRepositry.GetNotificationById(NotificationID);
            //If notification not found return
            if (Notification == null)
                return new SettersResponse { status = 0, msg = "Notification not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, Notification.User.Id, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Saving to the Database
            _notificationRepositry.Delete(Notification);
            return new SettersResponse { status = 2, msg = "Successful" };
        }
        public async Task<SettersResponse> DeleteAllNotifications(ClaimsPrincipal User, Guid UserID)//0 == Invalid UserID || 1 == User not found || 2 == Unauthorized || 3 == Successful
                                                                                                    //This leaves the notification with no user!
        {
            //Checking for UserID validty
            if (UserID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid UserID" };

            //Getting User from Database
            var user = await _userRepositry.GetById(UserID);
            //If user not found return
            if (user == null)
                return new SettersResponse { status = 0, msg = "User not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, user.Id, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Faulty DTO" };

            var deleted = await _notificationRepositry.DeleteUserNotifications(UserID);
            if(!deleted)
                return new SettersResponse { status = 0, msg = "No Notifications" };
            return new SettersResponse { status = 2, msg = "Successful" };
        }
        public Task<SettersResponse> MarkAllAsRead(Guid UserID)
        {
            throw new NotImplementedException();
        }
        public Task<SettersResponse> MarkAsRead(Guid NotificationID)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------

        //        *********** Getters ***********
        public async Task<Guid> GetNotificationUserID(Guid NotificationID)//not used anymore
        {
            //Getting UserID from Database
            var notif = await _notificationRepositry.GetNotificationById(NotificationID);
            return notif.User.Id;
        }
        public async Task<GettersResponse<NotificationMiniViewDTO>> GetNotifications(ClaimsPrincipal User, Guid UserID, string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize)
        {
            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new GettersResponse<NotificationMiniViewDTO>
                {
                    status = 1,
                    msg = "Unauthorized",
                };

            //Getting notifications from Database and projecting to DTO
            var notificationsQuery = _notificationRepositry.GetUserNotificationsQueryable(UserID);
            //if no notifications found return null
            if (notificationsQuery == null || notificationsQuery.Count() == 0)
                return new GettersResponse<NotificationMiniViewDTO>
                {
                    status = 0,
                    msg = "No Notifications"
                };

            //Filtering by start date and end date
            DateTime validStartDate, validEndDate;
            if (DateTime.TryParse(startDate, out validStartDate) && DateTime.TryParse(endDate, out validEndDate))
                _notificationRepositry.FilterDate(validStartDate, validEndDate, notificationsQuery);

            //Filtering by search term in title or content
            if (!string.IsNullOrEmpty(searchTerm)) notificationsQuery = _notificationRepositry.Search(searchTerm, notificationsQuery);

            //Ordering by a given column
            if (!string.IsNullOrEmpty(sortColumn))//Either sort by custom given inputs
                _notificationRepositry.FilterSortColumn(sortColumn, OrderBy, notificationsQuery);

            var notificationsProjection = notificationsQuery.Select(n => new NotificationMiniViewDTO
            {
                NotificationID = n.Id,
                Date = n.CreatedAt,
                Title = n.Title,
                Content = n.Content
            });

            //Making Paged list from resultant query
            var notifications = await PagedList<NotificationMiniViewDTO>.CreateAsync(notificationsProjection, page, pageSize);
            return new GettersResponse<NotificationMiniViewDTO>
            {
                status = 2,
                msg = "Successful",
                Data = notifications
            };
        }
        public async Task<GettersResponse<NotificationViewDTO>> GetAllNotifications(int page, int pageSize)
        {
            //Getting notifications from Database and projecting to DTO
            var notificationsQuery = _notificationRepositry.GetAll().Select(n => new NotificationViewDTO
            {
                NotificationID = n.Id,
                Date = n.CreatedAt,
                Title = n.Title,
                Content = n.Content,
                UserID = n.User.Id
            });

            if (notificationsQuery == null || notificationsQuery.Count() == 0)
                return new GettersResponse<NotificationViewDTO>
                {
                    status = 0,
                    msg = "No Notifications in Database"
                };
            //Ordering by date descending
            notificationsQuery = notificationsQuery.OrderByDescending(n => n.Date);

            //Making Paged list from resultant query
            var notifications = await PagedList<NotificationViewDTO>.CreateAsync(notificationsQuery,page, pageSize);
            return new GettersResponse<NotificationViewDTO>
            {
                status = 2,
                msg = "Successful",
                Data = notifications
            };
        }

    }
}
