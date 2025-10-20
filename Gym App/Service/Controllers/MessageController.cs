using DocumentFormat.OpenXml.Wordprocessing;
using Gym_App.Domain.DTOs;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Service.Controllers
{
    [Route("[controller]")]
    public class MessageController : Controller
    {
        private readonly IMessageService _messageService;
        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }
        [HttpPost("AddMessage")]
        public async Task<IActionResult> AddMessage([FromBody] MessageDTO message)
        {
            var result = await _messageService.AddMessage(message);
            if (result == 2) return Ok(new { Message = "Message succesfully created" });
            if (result == 1) return BadRequest(new { Message = "Session not found" });
            else return BadRequest(new {Message = "User not found"});
        }
        [HttpDelete("DeleteMessage")]
        public async Task<IActionResult> DeleteMessage([FromBody] MessageDTO message)
        {
            var result = await _messageService.DeleteMessages(message);
            if (result == 0) return BadRequest(new { message = "Couldn't find the message" });
            return Ok(new { message = "Message Deleted Successfully" });
        }
        [HttpPut("UpdateMessage")]
        public async Task<IActionResult> UpdateMessage([FromBody] MessageDTO message)
        {
            var result = await _messageService.UpdateMessage(message);
            if (result == 0) return BadRequest(new { message = "Failed to update message" });
            return Ok(new { message = "Message Updated Successfully" });
        }
        [HttpGet("GetSessionMessages")]
        public async Task<IActionResult> GetSessionMessages([FromQuery] Guid sessionID,int page, int pageSize)
        {
            var messages = await _messageService.GetSessionMessages(sessionID,page,pageSize);
            if (messages == null) return NotFound(new { message = "No messages found for this session" });
            return Ok(messages);
        }
        [HttpGet("GetMessagesByFilter")]
        public async Task<IActionResult> GetMessagesByFilter([FromQuery] string sortColumn, string OrderBy, string SearchTerm, int page, int pageSize)
        {
            var messages = await _messageService.GetMessagesByFilter(page, sortColumn, OrderBy, SearchTerm, pageSize);
            return Ok(messages);
        }
        [HttpGet("GetAllMessages")]
        public async Task<IActionResult> GetAllMessages([FromQuery]int page, int pageSize)
        {
            var messages = await _messageService.GetMessages(page,pageSize);
            return Ok(messages);
        }
    }
}
