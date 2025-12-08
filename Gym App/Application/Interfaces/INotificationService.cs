using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.DTOs.Notification;
using System.Security.Claims;

namespace Gym_App.Application.Interfaces
{
    public interface INotificationService 
    {
        Task<int> SendNotificationAsync(NotificationViewDTO notification);// Not implemented yet
        Task<int> CreateNotification(ClaimsPrincipal User,Guid userID, NotificationCreationDTO notification);
        Task<int> DeleteNotification(ClaimsPrincipal User, Guid NotificationID);
        Task<int> MarkAsRead(Guid NotificationID);// Not implemented yet
        Task<int> MarkAllAsRead(Guid UserID);// Not implemented yet
        Task<int> DeleteAllNotifications(ClaimsPrincipal User, Guid UserID);
        Task<Guid> GetNotificationUserID( Guid NotificationID);
        Task<PagedList<NotificationMiniViewDTO>> GetNotifications(ClaimsPrincipal User, Guid UserID,string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm,int pageSize);
        //Task<PagedList<NotificationDTO>> GetNotificationsByFilter(int page, string sortColumn, string OrderBy, string searchTerm, int pageSize);
        Task<PagedList<NotificationViewDTO>> GetAllNotifications(int page, int pageSize);

    }
}
