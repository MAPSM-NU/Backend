using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gym_App.Service.Functions.The_Applied
{
    
    public class NotificationService : INotificationService
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
            Notification newNotification = new Notification
            {
                NotificationID = Guid.NewGuid(),
                Date = DateTime.UtcNow,
                Title = notification.Title,
                Content = notification.Content,
            };
            var UserID = (from u in _db.Users
                         where u.UserID == notification.UserID
                         select u).FirstOrDefault();
            if (UserID == null) return await Task.FromResult(0);
            UserID.Notifications.Add(newNotification);
            _db.Notifications.Add(newNotification);
            await _db.SaveChangesAsync();
            return await Task.FromResult(1);

        }

        public async Task<int> DeleteAllNotifications(Guid UserID)//This leaves the notification with no user!
        {
            var user = _db.Users
                .Include(u => u.Notifications)
                .FirstOrDefault(u => u.UserID == UserID);

            if (user == null) return await Task.FromResult(0);

            user.Notifications?.Clear();
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return await Task.FromResult(1);
        }

        public async Task<int> DeleteNotification(Guid NotificationID)
        {
            var Notification = (from n in _db.Notifications
                                where n.NotificationID == NotificationID
                                select n).FirstOrDefault();
            if (Notification == null) return await Task.FromResult(0);
            _db.Notifications.Remove(Notification);
            await _db.SaveChangesAsync();
            return await Task.FromResult(1);
        }

        public async Task<IQueryable<Notification>> GetNotifications(Guid UserID)
        {
            var notifications = (from n in _db.Notifications.Include(n => n.User)
                                 where n.User.Any(user => user.UserID == UserID)
                                 select n);
            if (notifications == null) return null;
            return await Task.FromResult(notifications);
        }
        public async Task<IQueryable<Notification>> GetAllNotifications()
        {
            var Notifications = from n in _db.Notifications.Include(N => N.User)
                                select n;
            return await Task.FromResult(Notifications);
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
