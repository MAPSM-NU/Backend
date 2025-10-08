using Gym_App.Domain.DTOs;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SessionController : Controller
    {
        private readonly ISessionService _sessionService;
        public SessionController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }
        [HttpPost("CreateSession")]
        public async Task<IActionResult> CreateSession([FromBody] SessionDTO session)
        {
            var result = await _sessionService.CreateSession(session);
            if (result == 0) return BadRequest(new { message = "Failed to Create Session" });
            return Ok(new { message = "Session Created Succesfully" });
        }
        [HttpDelete("DeleteSession")]
        public async Task<IActionResult> DeleteSession([FromBody] Guid sessionID)
        {
            var result = await _sessionService.DeleteSession(sessionID);
            if(result == 0) return BadRequest(new { message = "Failed to Delete Session" });
            return Ok(new { message = "Session deleted Succesfully" });
        }
        [HttpPost("AddMessages")]
        public async Task<IActionResult> AddMessages([FromBody] SessionMessagesDTO sessionMessages)
        {
            var result = await _sessionService.AddMessages(sessionMessages);
            if (result == 0) return BadRequest(new { message = "Failed to add messages" });
            return Ok(new { message = "Messages Added Successfully" });
           
        }
        [HttpDelete("DeleteMessages")]
        public async Task<IActionResult> DeleteMessages([FromBody] SessionMessagesDTO sessionMessages)
        {
            var result = await _sessionService.DeleteMessages(sessionMessages);
            if (result == 0) return BadRequest(new { message = "Failed to delete messages" });
            return Ok(new { message = "Messages Deleted Successfully" });
            
        }
        [HttpPost("GetSessionMessages")]
        public async Task<IActionResult> GetSessionMessages([FromBody] Guid sessionID)
        {
            var messages = await _sessionService.GetSessionMessages(sessionID);
            if (messages == null) return NotFound(new { message = "No messages found for this session" });
            return Ok(messages);
        }
        [HttpPost("GetUsersOfSession")]
        public async Task<IActionResult> GetUsersOfSession([FromBody] Guid sessionID)
        {
            var users = await _sessionService.GetUsersOfSession(sessionID);
            if (users == null) return NotFound(new { message = "No users found for this session" });
            return Ok(users);
        }
        [HttpGet("GetAllSessions")]
        public async Task<IActionResult> GetAllSessions()
        {
            var sessions = await _sessionService.GetAllSessions();
            if (sessions == null) return NotFound(new { message = "No sessions found" });
            return Ok(sessions);
        }
    }
}
