using Gym_App.Domain.DTOs;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        [HttpPost("Create a Notification")]
        public async Task<IActionResult> CreateNotification([FromBody] NotificationDTO notification)
        {
            var result = await _notificationService.CreateNotification(notification);
            if (result == 0) return BadRequest(new { message = "Notification could not be created." });
            return Ok(new { message = "Notification created successfully." });
        }
        [HttpDelete("Delete a Notification")]
        public async Task<IActionResult> DeleteNotification([FromBody]Guid NotificationID)
        {
            var result = await _notificationService.DeleteNotification(NotificationID);
            if (result == 0) return NotFound(new { message = "Notification not found." });
            return Ok(new { message = "Notification deleted successfully." });
        }
        [HttpDelete("Delete All Users Notifications")]//Note to self. Dont put unnecassary space in the route. Will result in error
        public async Task<IActionResult> DeleteAllNotifications([FromBody] Guid UserID)
        {
            var result = await _notificationService.DeleteAllNotifications(UserID);
            if (result == 0) return NotFound(new { message = "User not found." });
            return Ok(new{message = "All notifications deleted successfully."});
        }
        [HttpPost("Get a Users Notifications")]
        public async Task<IActionResult> GetNotifications([FromBody]Guid UserID)
        {
            var notifications = await _notificationService.GetNotifications(UserID);
            if (notifications == null) return NotFound(new { message = "User not found or no notifications." });
            return Ok(notifications);
        }
        [HttpGet("Get All Notifications")]
        public async Task<IActionResult> GetAllNotifications()
        {
            var notifications = await _notificationService.GetAllNotifications();
            return Ok(notifications);
        }

    }
}
