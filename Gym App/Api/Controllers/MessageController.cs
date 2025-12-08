using DocumentFormat.OpenXml.Wordprocessing;
using Gym_App.Application.Interfaces;
using Gym_App.Infastructure.DTOs.Message;
using MailKit.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Api.Controllers
{
    [Authorize(Policy ="NormalUsage")]
    [Route("[controller]")]
    public class MessageController : Controller
    {
        private readonly IMessageService _messageService;
        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }
        [HttpPost("AddMessage")]
        public async Task<IActionResult> AddMessage([FromQuery]Guid userID,[FromBody] MessageCreationDTO message)
        {
            var result = await _messageService.AddMessage(User,userID, message);
            if (result == 5)
                return Ok(new { message = "Message Added Successfully" });
            else if (result == 4)
                return Forbid();
            else if (result == 3)
                return BadRequest(new { message = "Session not found" });
            else if (result == 2)
                return Forbid();
            else if (result == 1)
                return BadRequest(new { message = "User not found" });
            return BadRequest(new { message = "Invalid DTO" });
        }
        [HttpPut("UpdateMessage")]
        public async Task<IActionResult> UpdateMessage([FromQuery] Guid messageID, [FromBody] MessageUpdateDTO message)
        {
            var result = await _messageService.UpdateMessage(User, messageID, message);
            if (result == 3)
                return Ok(new { message = "Message Updated Successfully" });
            else if (result == 2)
                return Forbid();
            else if (result == 1)
                return BadRequest(new { message = "Message not found" });
            return BadRequest(new { message = "Invalid DTO" });
        }
        [HttpDelete("DeleteMessage")]
        public async Task<IActionResult> DeleteMessage([FromQuery] Guid messageID)
        {
            var result = await _messageService.DeleteMessage(User, messageID);
            if(result == 3)
                return Ok(new { message = "Message Deleted Successfully" });
            else if (result == 2)
                return Forbid();
            else if (result == 1)
                return BadRequest(new { message = "Message not found" });
            return BadRequest(new { message = "Invalid messageID" });
        }
        [HttpGet("GetSessionMessages")]
        public async Task<IActionResult> GetSessionMessages([FromQuery] Guid sessionID, string startDate, string endDate, string sortColumn, string OrderBy, string searchTerm, int page, int pageSize)
        {
            var messages = await _messageService.GetSessionMessages(User,sessionID, startDate, endDate, page, sortColumn, OrderBy, searchTerm, pageSize);
            if (messages == null)
                return BadRequest(new { message = "No messages where found or you are not authorized" });
            return Ok(messages);
        }
        //[Authorize(Policy = "ElevatedPower")]
        [HttpGet("GetMessagesByFilter")]
        public async Task<IActionResult> GetMessagesByFilter([FromQuery]string startDate,string endDate, string sortColumn, string OrderBy, string SearchTerm, int page, int pageSize)
        {
            var messages = await _messageService.GetMessagesByFilter(startDate,endDate, page, sortColumn, OrderBy, SearchTerm, pageSize);
            if (messages == null)
                return BadRequest(new { message = "No messages where found or you are not authorized" });
            return Ok(messages);
        }
        //[Authorize(Policy = "ElevatedPower")]
        [HttpGet("GetAllMessages")]
        public async Task<IActionResult> GetAllMessages([FromQuery]int page, int pageSize)
        {
            var messages = await _messageService.GetMessages(page,pageSize);
            return Ok(messages);
        }
    }
}
