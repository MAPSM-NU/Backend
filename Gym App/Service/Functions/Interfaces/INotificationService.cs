using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;

namespace Gym_App.Service.Functions.Interfaces
{
    public interface INotificationService 
    {
        Task<int> SendNotificationAsync(NotificationDTO notification);// Not implemented yet
        Task<int> CreateNotification(NotificationDTO notification);
        Task<int> DeleteNotification(Guid NotificationID);
        Task<int> MarkAsRead(Guid NotificationID);// Not implemented yet
        Task<int> MarkAllAsRead(Guid UserID);// Not implemented yet
        Task<int> DeleteAllNotifications(Guid UserID);
        Task<IQueryable<Notification>> GetNotifications(Guid UserID);
        Task<IQueryable<Notification>> GetAllNotifications();

    }
}
