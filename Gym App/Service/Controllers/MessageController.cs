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
            if (result == 0) return BadRequest(new { message = "Failed to add message" });
            return Ok(new { message = "Message Added Successfully" });
        }
        [HttpDelete("DeleteMessage")]
        public async Task<IActionResult> DeleteMessage([FromBody] MessageDTO message)
        {
            var result = await _messageService.DeleteMessages(message);
            if (result == 0) return BadRequest(new { message = "Failed to delete message" });
            return Ok(new { message = "Message Deleted Successfully" });
        }
        [HttpPut("UpdateMessage")]
        public async Task<IActionResult> UpdateMessage([FromBody] MessageDTO message)
        {
            var result = await _messageService.UpdateMessage(message);
            if (result == 0) return BadRequest(new { message = "Failed to update message" });
            return Ok(new { message = "Message Updated Successfully" });
        }
        [HttpPost("GetSessionMessages")]
        public async Task<IActionResult> GetSessionMessages([FromBody] Guid sessionID)
        {
            var messages = await _messageService.GetSessionMessages(sessionID);
            if (messages == null) return NotFound(new { message = "No messages found for this session" });
            return Ok(messages);
        }
        [HttpGet("GetAllMessages")]
        public async Task<IActionResult> GetAllMessages()
        {
            var messages = await _messageService.GetMessages();
            if (messages == null) return NotFound(new { message = "No messages found" });
            return Ok(messages);
        }
    }
}
