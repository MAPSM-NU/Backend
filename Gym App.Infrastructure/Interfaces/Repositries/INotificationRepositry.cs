using Gym_App.Domain;

namespace Gym_App.Infastructure.Interfaces.Repositries
{
    public interface INotificationRepositry : IBaseRepositry<Notification>
    {
        // Retrieval methods
        Task<Notification> GetNotificationById(Guid notificationId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Notification>> GetUserNotifications(Guid userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Notification>> GetRecentNotifications(Guid userId, int count = 10, CancellationToken cancellationToken = default);

        // Count operations
        Task<int> GetUserNotificationCount(Guid userId, CancellationToken cancellationToken = default);

        // Existence checks
        Task<bool> isNotificationExist(Guid notificationId, CancellationToken cancellationToken = default);
        Task<bool> isUserHasNotifications(Guid userId, CancellationToken cancellationToken = default);

        // Queryable for filtering
        IQueryable<Notification> GetUserNotificationsQueryable(Guid userId);

        // Bulk operations
        Task<bool> DeleteUserNotifications(Guid userId, CancellationToken cancellationToken = default);
        Task DeleteNotificationsOlderThan(DateTime date, CancellationToken cancellationToken = default);
    }
}
