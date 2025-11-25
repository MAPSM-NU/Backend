using Gym_App.Domain.DTOs;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Service.Controllers
{
    [Authorize(Policy = "NormalUsage")]
    [Route("[controller]")]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        //[Authorize(Policy = "ElevatedPower")]//Only Admins can create notifications 
        [HttpPost("CreateNotification")]
        public async Task<IActionResult> CreateNotification([FromBody] NotificationDTO notification)
        {
            
            var result = await _notificationService.CreateNotification(User,notification);
            if (result == 3)
                return Ok(new { message = "Notification created succesfully" });
            else if (result == 2)
                return Forbid();
            else if (result == 1)
                return BadRequest(new { message = "User not found." });
            else
                return BadRequest(new { message = "Faulty DTO" });
        }
        [HttpDelete("DeleteNotification")]
        public async Task<IActionResult> DeleteNotification([FromQuery]Guid notificationID)//As in deleting the whole notif. there should be one were we delete the notif from the user's list
        {
            var result = await _notificationService.DeleteNotification(User, notificationID);
            if (result == 3)
                return Ok(new { message = "Notification Deleted succesfully" });
            else if (result == 2)
                return Forbid();
            else if (result == 1)
                return BadRequest(new { message = "Notification not found." });
            else
                return BadRequest(new { message = "Invalid NotificationID" });
        }
        [HttpDelete("DeleteAllUsersNotifications")]//Note to self. Dont put unnecassary space in the route. Will result in error
        public async Task<IActionResult> DeleteAllNotifications([FromQuery] Guid userID)
        {
            var result = await _notificationService.DeleteAllNotifications(User, userID);
            if (result == 3)
                return Ok(new { message = "Notification Deleted succesfully" });
            else if (result == 2)
                return Forbid();
            else if (result == 1)
                return BadRequest(new { message = "User not found." });
            else
                return BadRequest(new { message = "Invalid NotificationID" });
        }
        [HttpGet("GetUsersNotifications")]
        public async Task<IActionResult> GetNotifications([FromQuery]Guid userID, string startDate, string endDate, string sortColumn, string OrderBy, string searchTerm, int page, int pageSize)
        {
            var notifications = await _notificationService.GetNotifications(User,userID,startDate,endDate,page,sortColumn,OrderBy,searchTerm,pageSize);
            if (notifications == null) 
                return BadRequest(new { message = "Unauthorized access,User has no notifications or User doesn't exist" });
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
