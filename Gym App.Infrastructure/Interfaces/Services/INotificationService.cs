using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.DTOs.Notification;
using Gym_App.Infastructure.Transfer_Classes;
using System.Security.Claims;
using System.Threading;

namespace Gym_App.Infastructure.Interfaces.Services
{
    public interface INotificationService
    {
        Task<SettersResponse> SendNotificationAsync(NotificationViewDTO notification, CancellationToken cancellationToken = default);
        Task<SettersResponse> CreateNotification(Guid userID, NotificationCreationDTO notification, CancellationToken cancellationToken = default);
        Task<SettersResponse> DeleteNotification(Guid NotificationID, CancellationToken cancellationToken = default);
        Task<SettersResponse> MarkAsRead(Guid NotificationID, CancellationToken cancellationToken = default);
        Task<SettersResponse> MarkAllAsRead(Guid UserID, CancellationToken cancellationToken = default);
        Task<SettersResponse> DeleteAllNotifications(Guid UserID, CancellationToken cancellationToken = default);
        Task<Guid> GetNotificationUserID(Guid NotificationID, CancellationToken cancellationToken = default);
        Task<GettersResponse<NotificationMiniViewDTO>> GetNotifications(Guid UserID, string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize, CancellationToken cancellationToken = default);
        Task<GettersResponse<NotificationViewDTO>> GetAllNotifications(int page, int pageSize, CancellationToken cancellationToken = default);
    }
}
