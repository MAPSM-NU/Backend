using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.DTOs.Notification;
using Gym_App.Infastructure.Transfer_Classes;
using System.Security.Claims;

namespace Gym_App.Application.Interfaces
{
    public interface INotificationService 
    {
        Task<SettersResponse> SendNotificationAsync(NotificationViewDTO notification);// Not implemented yet
        Task<SettersResponse> CreateNotification(ClaimsPrincipal User,Guid userID, NotificationCreationDTO notification);
        Task<SettersResponse> DeleteNotification(ClaimsPrincipal User, Guid NotificationID);
        Task<SettersResponse> MarkAsRead(Guid NotificationID);// Not implemented yet
        Task<SettersResponse> MarkAllAsRead(Guid UserID);// Not implemented yet
        Task<SettersResponse> DeleteAllNotifications(ClaimsPrincipal User, Guid UserID);
        Task<Guid> GetNotificationUserID( Guid NotificationID);
        Task<GettersResponse<NotificationMiniViewDTO>> GetNotifications(ClaimsPrincipal User, Guid UserID,string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm,int pageSize);
        //Task<PagedList<NotificationDTO>> GetNotificationsByFilter(int page, string sortColumn, string OrderBy, string searchTerm, int pageSize);
        Task<GettersResponse<NotificationViewDTO>> GetAllNotifications(int page, int pageSize);

    }
}
