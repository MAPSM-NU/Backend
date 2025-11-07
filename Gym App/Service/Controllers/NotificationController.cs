using Gym_App.Domain.DTOs;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Service.Controllers
{
    [Route("[controller]")]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        [HttpPost("CreateNotification")]
        public async Task<IActionResult> CreateNotification([FromBody] NotificationDTO notification)
        {
            var result = await _notificationService.CreateNotification(notification);
            if (result == 0) return BadRequest(new { message = "Given user was not found." });
            return Ok(new { message = "Notification created successfully." });
        }
        [HttpDelete("DeleteNotification")]
        public async Task<IActionResult> DeleteNotification([FromQuery]Guid NotificationID)
        {
            var result = await _notificationService.DeleteNotification(NotificationID);
            if (result == 0) return BadRequest(new { message = "Notification not found." });
            return Ok(new { message = "Notification deleted successfully." });
        }
        [HttpDelete("DeleteAllUsersNotifications")]//Note to self. Dont put unnecassary space in the route. Will result in error
        public async Task<IActionResult> DeleteAllNotifications([FromQuery] Guid UserID)
        {
            var result = await _notificationService.DeleteAllNotifications(UserID);
            if (result == 0) return BadRequest(new { message = "User not found." });
            return Ok(new{message = "All notifications deleted successfully."});
        }
        [HttpGet("GetUsersNotifications")]
        public async Task<IActionResult> GetNotifications([FromQuery]Guid UserID, string startDate, string endDate, string sortColumn, string OrderBy, string searchTerm, int page, int pageSize)
        {
            var notifications = await _notificationService.GetNotifications(UserID,startDate,endDate,page,sortColumn,OrderBy,searchTerm,pageSize);
            if (notifications == null) return BadRequest(new { message = "User not found or no notifications." });
            return Ok(notifications);
        }
        [HttpGet("GetAllNotifications")]
        public async Task<IActionResult> GetAllNotifications([FromQuery]int page,int pageSize)
        {
            var notifications = await _notificationService.GetAllNotifications(page,pageSize);
            return Ok(notifications);
        }

    }
}
