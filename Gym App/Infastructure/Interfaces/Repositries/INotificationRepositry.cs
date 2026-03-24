using Gym_App.Domain;

namespace Gym_App.Infastructure.Interfaces.Repositries
{
    public interface INotificationRepositry : IBaseRepositry<Notification>
    {
        // Retrieval methods
        Task<Notification> GetNotificationById(Guid notificationId);
        Task<IEnumerable<Notification>> GetUserNotifications(Guid userId);
        Task<IEnumerable<Notification>> GetRecentNotifications(Guid userId, int count = 10);
        
        // Count operations
        Task<int> GetUserNotificationCount(Guid userId);
        
        // Existence checks
        Task<bool> isNotificationExist(Guid notificationId);
        Task<bool> isUserHasNotifications(Guid userId);
        
        // Queryable for filtering
        IQueryable<Notification> GetUserNotificationsQueryable(Guid userId);
        
        // Bulk operations
        Task<bool> DeleteUserNotifications(Guid userId);
        Task DeleteNotificationsOlderThan(DateTime date);
    }
}
