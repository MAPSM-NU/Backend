using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using System.Security.Claims;

namespace Gym_App.Service.Functions.Interfaces
{
    public interface INotificationService 
    {
        Task<int> SendNotificationAsync(NotificationDTO notification);// Not implemented yet
        Task<int> CreateNotification(ClaimsPrincipal User, NotificationDTO notification);
        Task<int> DeleteNotification(ClaimsPrincipal User, Guid NotificationID);
        Task<int> MarkAsRead(Guid NotificationID);// Not implemented yet
        Task<int> MarkAllAsRead(Guid UserID);// Not implemented yet
        Task<int> DeleteAllNotifications(ClaimsPrincipal User, Guid UserID);
        Task<Guid> GetNotificationUserID( Guid NotificationID);
        Task<PagedList<NotificationDTO>> GetNotifications(ClaimsPrincipal User, Guid UserID,string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm,int pageSize);
        //Task<PagedList<NotificationDTO>> GetNotificationsByFilter(int page, string sortColumn, string OrderBy, string searchTerm, int pageSize);
        Task<PagedList<NotificationDTO>> GetAllNotifications(int page, int pageSize);

    }
}
