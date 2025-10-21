using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Gym_App.Service.Functions.The_Applied
{
    
    public class NotificationService : INotificationService // Fuck this class. Needs a change again. relationship with users should not be many to many GRAAAAAA
    {
        private readonly DbBase _db;
        public NotificationService(DbBase db)
        {
            _db = db;
        }
        public async Task<int> SendNotificationAsync(NotificationDTO notification)
        {
            throw new NotImplementedException();
        }
        public async Task<int> CreateNotification(NotificationDTO notification)
        {
            var UserID = await (from u in _db.Users
                         where u.UserID == notification.UserID
                         select u).FirstOrDefaultAsync();
            if (UserID == null) return 0;
            Notification newNotification = new Notification
            {
                NotificationID = Guid.NewGuid(),
                Date = DateTime.UtcNow,
                Title = notification.Title,
                Content = notification.Content,
            };
            UserID.Notifications?.Add(newNotification);
            _db.Notifications.Add(newNotification);
            await _db.SaveChangesAsync();
            return 1;

        }
        public async Task<int> DeleteNotification(Guid NotificationID)
        {
            var Notification =  await (from n in _db.Notifications
                                where n.NotificationID == NotificationID
                                select n).FirstOrDefaultAsync();
            if (Notification == null) return 0;
            _db.Notifications.Remove(Notification);
            await _db.SaveChangesAsync();
            return 1;
        }
        public async Task<int> DeleteAllNotifications(Guid UserID)//This leaves the notification with no user!
        {
            var user = await _db.Users
                .Include(u => u.Notifications)//Include is ,sadly😔, important here
                .FirstOrDefaultAsync(u => u.UserID == UserID);

            if (user == null) return 0;

            user.Notifications?.Clear();
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return 1;
        }

        public async Task<PagedList<NotificationDTO>> GetNotifications(Guid UserID, string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize)
        {
            if (page == 0) page = 1;
            if (pageSize == 0) pageSize = 10;
            var notificationsQuery = (from n in _db.Notifications
                                 where n.User.UserID == UserID
                                 select new NotificationDTO
                                 {
                                    NotificationID = n.NotificationID,
                                    UserID = UserID,
                                    Date = n.Date,
                                    Title = n.Title,
                                    Content = n.Content
                                 });
            if (notificationsQuery == null) return null;
            DateTime validStartDate, validEndDate;
            if(DateTime.TryParse(startDate,out validStartDate))
            {
                notificationsQuery = notificationsQuery.Where(n=>n.Date >  validStartDate);
            }
            if (DateTime.TryParse(endDate,out validEndDate))
            {
                notificationsQuery = notificationsQuery.Where(n=>n.Date < validEndDate);
            }
            if (!string.IsNullOrEmpty(searchTerm)) notificationsQuery = notificationsQuery.Where(n => n.Content.Contains(searchTerm) || n.Title.Contains(searchTerm));
            if (!string.IsNullOrEmpty(sortColumn))//Either sort by custom given inputs
            {
                Expression<Func<NotificationDTO, Object>> keySelector = sortColumn.ToLower() switch
                {
                    "content" or "c" => Notification => Notification.Content,
                    "title" or "t" => Notification => Notification.Title,
                    _ => Notification => Notification.NotificationID
                };
                if (!string.IsNullOrEmpty(OrderBy)) notificationsQuery = notificationsQuery.OrderBy(keySelector);
                else notificationsQuery = notificationsQuery.OrderByDescending(keySelector);
            }
            else//Or you can sort from most recent notifications
            {
                notificationsQuery = notificationsQuery.OrderByDescending(n => n.Date);
            }
            var notifications = await PagedList<NotificationDTO>.CreateAsync(notificationsQuery, page, pageSize);
            return notifications;
        }

        public async Task<PagedList<NotificationDTO>> GetAllNotifications(int page, int pageSize)
        {
            if (page == 0) page = 1;
            if(pageSize == 0) pageSize = 10;
            var notificationsQuery =(from n in _db.Notifications
                                select new NotificationDTO
                                {
                                    NotificationID = n.NotificationID,
                                    UserID = n.User.UserID,
                                    Date = n.Date,
                                    Title = n.Title,
                                    Content = n.Content
                                });
            notificationsQuery = notificationsQuery.OrderByDescending(n => n.Date);
            var notifications = await PagedList<NotificationDTO>.CreateAsync(notificationsQuery,page, pageSize);
            return notifications;
        }
        public Task<int> MarkAllAsRead(Guid UserID)
        {
            throw new NotImplementedException();
        }

        public Task<int> MarkAsRead(Guid NotificationID)
        {
            throw new NotImplementedException();
        }

    }
}
