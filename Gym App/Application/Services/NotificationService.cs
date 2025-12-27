using Gym_App.Application.Interfaces;
using Gym_App.Domain;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.DTOs.Notification;
using Gym_App.Infastructure.Transfer_Classes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Gym_App.Application.Services
{

    public class NotificationService : INotificationService // Fuck this class. Needs a change again. relationship with users should not be many to many GRAAAAAA
    {
        private readonly DbBase _db;
        private readonly IAuthorizationService _authorizationService;
        public NotificationService(DbBase db, IAuthorizationService authorizationService)
        {
            _db = db;
            _authorizationService = authorizationService;
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
            var user = await (from u in _db.Users
                              where u.UserID == userID
                              select u).FirstOrDefaultAsync();
            //If user not found return 
            if (user == null)
                return new SettersResponse { status = 0, msg = "User not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, user.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Faulty DTO" };

            //Creating Notification
            Notification newNotification = new Notification
            {
                NotificationID = Guid.NewGuid(),
                Date = DateTime.UtcNow,
                Title = notification.Title,
                Content = notification.Content,
                User = user,
            };

            //Saving to the Database
            user.Notifications?.Add(newNotification);
            _db.Notifications.Add(newNotification);
            await _db.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Successful" };

        }
        public async Task<SettersResponse> DeleteNotification(ClaimsPrincipal User, Guid NotificationID)//0 == Invalid NotificationID || 1 == Notification not found || 2 == Unauthorized || 3 == Successful
        {
            //Checking for NotificationID validty
            if (NotificationID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid NotificationID" };

            //Getting notification from Database
            var Notification = await (from n in _db.Notifications.Include(n => n.User)
                                      where n.NotificationID == NotificationID
                                      select n).FirstOrDefaultAsync();
            //If notification not found return
            if (Notification == null)
                return new SettersResponse { status = 0, msg = "Notification not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, Notification.User.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Saving to the Database
            _db.Notifications.Remove(Notification);
            await _db.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Successful" };
        }
        public async Task<SettersResponse> DeleteAllNotifications(ClaimsPrincipal User, Guid UserID)//0 == Invalid UserID || 1 == User not found || 2 == Unauthorized || 3 == Successful
                                                                                                    //This leaves the notification with no user!
        {
            //Checking for UserID validty
            if (UserID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid UserID" };

            //Getting User from Database
            var user = await _db.Users
                .Include(u => u.Notifications)//Include is ,sadly😔, important here
                .FirstOrDefaultAsync(u => u.UserID == UserID);
            //If user not found return
            if (user == null)
                return new SettersResponse { status = 0, msg = "User not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, user.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Faulty DTO" };

            //Saving to the Database
            user.Notifications?.Clear();
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
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
            var userID = await (from n in _db.Notifications
                                where n.NotificationID == NotificationID
                                select n.User.UserID).FirstOrDefaultAsync();
            return userID;
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
            var notificationsQuery = from n in _db.Notifications
                                     where n.User.UserID == UserID
                                     select new NotificationMiniViewDTO
                                     {
                                         NotificationID = n.NotificationID,
                                         Date = n.Date,
                                         Title = n.Title,
                                         Content = n.Content
                                     };
            //if no notifications found return null
            if (notificationsQuery == null || notificationsQuery.Count() == 0)
                return new GettersResponse<NotificationMiniViewDTO>
                {
                    status = 0,
                    msg = "No Notifications"
                };

            //Filtering by start date and end date
            DateTime validStartDate, validEndDate;
            if (DateTime.TryParse(startDate, out validStartDate))
            {
                notificationsQuery = notificationsQuery.Where(n => n.Date > validStartDate);
            }
            if (DateTime.TryParse(endDate, out validEndDate))
            {
                notificationsQuery = notificationsQuery.Where(n => n.Date < validEndDate);
            }

            //Filtering by search term in title or content
            if (!string.IsNullOrEmpty(searchTerm)) notificationsQuery = notificationsQuery.Where(n => n.Content.Contains(searchTerm) || n.Title.Contains(searchTerm));

            //Ordering by a given column
            if (!string.IsNullOrEmpty(sortColumn))//Either sort by custom given inputs
            {
                Expression<Func<NotificationMiniViewDTO, object>> keySelector = sortColumn.ToLower() switch
                {
                    "content" or "c" => Notification => Notification.Content!,// ordering by content
                    "title" or "t" => Notification => Notification.Title, // ordering by title
                    _ => Notification => Notification.NotificationID//failsafe: ordering by NotificationID
                };

                //If no orderby was inputed, then we sort ascending
                if (!string.IsNullOrEmpty(OrderBy)) notificationsQuery = notificationsQuery.OrderBy(keySelector);

                //else if anything was inputed we sort descending
                else notificationsQuery = notificationsQuery.OrderByDescending(keySelector);
            }
            else//Or you can sort from most recent notifications
            {
                notificationsQuery = notificationsQuery.OrderByDescending(n => n.Date);
            }

            //Making Paged list from resultant query
            var notifications = await PagedList<NotificationMiniViewDTO>.CreateAsync(notificationsQuery, page, pageSize);
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
            var notificationsQuery = from n in _db.Notifications
                                     select new NotificationViewDTO
                                     {
                                         NotificationID = n.NotificationID,
                                         UserID = n.User.UserID,
                                         Date = n.Date,
                                         Title = n.Title,
                                         Content = n.Content
                                     };

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
