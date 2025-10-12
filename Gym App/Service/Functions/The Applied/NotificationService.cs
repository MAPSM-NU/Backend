using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.EntityFrameworkCore;

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
            if (UserID == null) return await Task.FromResult(0);
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
            return await Task.FromResult(1);

        }
        public async Task<int> DeleteNotification(Guid NotificationID)
        {
            var Notification =  await (from n in _db.Notifications
                                where n.NotificationID == NotificationID
                                select n).FirstOrDefaultAsync();
            if (Notification == null) return await Task.FromResult(0);
            _db.Notifications.Remove(Notification);
            await _db.SaveChangesAsync();
            return await Task.FromResult(1);
        }
        public async Task<int> DeleteAllNotifications(Guid UserID)//This leaves the notification with no user!
        {
            var user = await _db.Users
                .Include(u => u.Notifications)//Include is ,sadly😔, important here
                .FirstOrDefaultAsync(u => u.UserID == UserID);

            if (user == null) return await Task.FromResult(0);

            user.Notifications?.Clear();
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return await Task.FromResult(1);
        }

        public async Task<List<NotificationDTO>> GetNotifications(Guid UserID)
        {
             var notifications = await (from n in _db.Notifications
                                        from u in n.User
                                        where u.UserID == UserID
                                        select new NotificationDTO
                                        {
                                            NotificationID = n.NotificationID,
                                            UserID = u.UserID,
                                            Date = n.Date,
                                            Title = n.Title,
                                            Content = n.Content
                                        }
                                    ).ToListAsync();
            if (notifications == null) return null;
            return await Task.FromResult(notifications);
        }
        public async Task<List<NotificationDTO>> GetAllNotifications()
        {
            var Notifications = await (from n in _db.Notifications
                                select new NotificationDTO
                                {
                                    NotificationID = n.NotificationID,
                                    Date = n.Date,
                                    Title = n.Title,
                                    Content = n.Content
                                }).ToListAsync();
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
