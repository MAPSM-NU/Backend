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
        private readonly IAuthorizationService _authenticationService;
        public NotificationController(INotificationService notificationService,IAuthorizationService authenticationService)
        {
            _notificationService = notificationService;
            _authenticationService = authenticationService;
        }
        [Authorize(Policy = "ElevatedPower")]//Only Admins can create notifications 
        [HttpPost("CreateNotification")]
        public async Task<IActionResult> CreateNotification([FromBody] NotificationDTO notification)
        {
            
            var result = await _notificationService.CreateNotification(notification);
            if (result == 0) 
                return BadRequest(new { message = "Given user was not found." });
            return 
                Ok(new { message = "Notification created successfully." });
        }
        [HttpDelete("DeleteNotification")]
        public async Task<IActionResult> DeleteNotification([FromQuery]Guid notificationID)
        {
            //Authorization
            if (notificationID == Guid.Empty)
                return BadRequest(new { message = "NotificationID cannot be empty." });
            var userID = await _notificationService.GetNotificationUserID(notificationID);
            if(userID == Guid.Empty)
                return BadRequest(new { message = "Notification not found." });
            var authResult = await _authenticationService.AuthorizeAsync(User,userID,"SameUserPolicy");
            if(!authResult.Succeeded)
                return Forbid();
            //Talking to Database
            var result = await _notificationService.DeleteNotification(notificationID);
            if (result == 0)
                return BadRequest(new { message = "Notification not found." });
            return Ok(new { message = "Notification deleted successfully." });
        }
        [HttpDelete("DeleteAllUsersNotifications")]//Note to self. Dont put unnecassary space in the route. Will result in error
        public async Task<IActionResult> DeleteAllNotifications([FromQuery] Guid userID)
        {
            //Authorization
            if (userID == Guid.Empty)
                return BadRequest(new { message = "Notification not found." });
            var authResult = await _authenticationService.AuthorizeAsync(User, userID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return Forbid();
            //Talking to Database
            var result = await _notificationService.DeleteAllNotifications(userID);
            if (result == 0) 
                return BadRequest(new { message = "User not found." });
            return Ok(new{message = "All notifications deleted successfully."});
        }
        [HttpGet("GetUsersNotifications")]
        public async Task<IActionResult> GetNotifications([FromQuery]Guid userID, string startDate, string endDate, string sortColumn, string OrderBy, string searchTerm, int page, int pageSize)
        {
            //Authorization
            if (userID == Guid.Empty)
                return BadRequest(new { message = "Notification not found." });
            var authResult = await _authenticationService.AuthorizeAsync(User, userID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return Forbid();
            //Talking to Database
            var notifications = await _notificationService.GetNotifications(userID,startDate,endDate,page,sortColumn,OrderBy,searchTerm,pageSize);
            if (notifications == null) 
                return BadRequest(new { message = "User not found or no notifications." });
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
