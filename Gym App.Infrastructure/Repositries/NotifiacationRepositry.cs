using Gym_App.Domain;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.DTOs.Notification;
using Gym_App.Infastructure.Interfaces.Repositries;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Threading;

namespace Gym_App.Infastructure.Repositries
{
    public class NotifiacationRepositry : BaseRepositry<Notification>, INotificationRepositry
    {
        private readonly DbBase _db;
        private readonly DbSet<Notification> table;
        public NotifiacationRepositry(DbBase db) : base(db)
        {
            _db = db;
            table = _db.Set<Notification>();
        }
        public async Task DeleteNotificationsOlderThan(DateTime date, CancellationToken cancellationToken = default)
        {
            var notifications = table.Where(n => n.CreatedAt < date);
            table.RemoveRange(notifications);
            await Task.CompletedTask;
        }

        public async Task<bool> DeleteUserNotifications(Guid userId, CancellationToken cancellationToken = default)
        {
            var notifications = await table.Where(n => n.User.Id == userId).ToListAsync(cancellationToken);
            if (notifications.Count == 0) return false;
            table.RemoveRange(notifications);
            return true;
        }

        public async Task<Notification> GetNotificationById(Guid notificationId, CancellationToken cancellationToken = default)
        {
            return await table!.Include(n => n.User).FirstOrDefaultAsync(n => n.Id == notificationId, cancellationToken);
        }

        public async Task<IEnumerable<Notification>> GetRecentNotifications(Guid userId, int count = 10, CancellationToken cancellationToken = default)
        {
            var notifications = await table.Where(n => n.User.Id == userId)
                                        .OrderByDescending(n => n.CreatedAt)
                                        .Take(count)
                                        .ToListAsync(cancellationToken);
            return notifications;
        }

        public async Task<int> GetUserNotificationCount(Guid userId, CancellationToken cancellationToken = default)
        {
            return await table.CountAsync(n => n.User.Id == userId, cancellationToken);

        }

        public async Task<IEnumerable<Notification>> GetUserNotifications(Guid userId, CancellationToken cancellationToken = default)
        {
            return await table.Where(n => n.User.Id == userId).ToListAsync(cancellationToken);
        }

        public IQueryable<Notification> GetUserNotificationsQueryable(Guid userId)
        {
            return table.Where(n => n.User.Id == userId).AsQueryable();
        }

        public async Task<bool> isNotificationExist(Guid notificationId, CancellationToken cancellationToken = default)
        {
            return await table.AnyAsync(n => n.Id == notificationId, cancellationToken);
        }

        public async Task<bool> isUserHasNotifications(Guid userId, CancellationToken cancellationToken = default)
        {
            return await table.AnyAsync(n => n.User.Id == userId, cancellationToken);
        }
        public override IQueryable<Notification> Search(string searchTerm, IQueryable<Notification> query)
        {
            return query.Where(n => n.Title.Contains(searchTerm) || n.Content!.Contains(searchTerm));
        }
        public override IQueryable<Notification> FilterSortColumn(string columnName, string sortOrder, IQueryable<Notification> query)
        {
            Expression<Func<Notification, object>> keySelector = columnName.ToLower() switch
            {
                "content" or "c" => Notification => Notification.Content!,// ordering by content
                "title" or "t" => Notification => Notification.Title, // ordering by title
                _ => Notification => Notification.Id//failsafe: ordering by NotificationID
            };

            //If no orderby was inputed, then we sort ascending
            if (!string.IsNullOrEmpty(sortOrder)) query = query.OrderBy(keySelector);
            //else if anything was inputed we sort descending
            else query = query.OrderByDescending(keySelector);
            return query;
        }
    }
}
