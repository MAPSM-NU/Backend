using Gym_App.Application.Interfaces;
using Gym_App.Infastructure.DTOs.Notification;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Api.Controllers
{
    [Authorize(Policy = "NormalUsage")]
    [Route("api/v1/notif")]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        //[Authorize(Policy = "ElevatedPower")]//Only Admins can create notifications 
        [HttpPost("create/{userID}")]
        public async Task<IActionResult> CreateNotification([FromRoute]Guid userID,[FromBody] NotificationCreationDTO notification)
        {

            var result = await _notificationService.CreateNotification(User, userID, notification);
            
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpDelete("delete/{notificationID}")]
        public async Task<IActionResult> DeleteNotification([FromRoute]Guid notificationID)//As in deleting the whole notif. there should be one were we delete the notif from the user's list
        {
            var result = await _notificationService.DeleteNotification(User, notificationID);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpDelete("delete-all/{userID}")]//Note to self. Dont put unnecassary space in the route. Will result in error
        public async Task<IActionResult> DeleteAllNotifications([FromRoute] Guid userID)
        {
            var result = await _notificationService.DeleteAllNotifications(User, userID);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpGet("user-notifs/{userID}")]
        public async Task<IActionResult> GetNotifications([FromRoute]Guid userID,[FromQuery] string startDate, string endDate, string sortColumn, string OrderBy, string searchTerm, int page, int pageSize)
        {
            var notifications = await _notificationService.GetNotifications(User,userID,startDate,endDate,page,sortColumn,OrderBy,searchTerm,pageSize);
            if (notifications == null) 
                return BadRequest(new { message = "Unauthorized access,User has no notifications or User doesn't exist" });
            return Ok(notifications);
        }
        [HttpGet("get")]
        public async Task<IActionResult> GetAllNotifications([FromQuery]int page,int pageSize)
        {
            var notifications = await _notificationService.GetAllNotifications(page,pageSize);
            return Ok(notifications);
        }

    }
}
