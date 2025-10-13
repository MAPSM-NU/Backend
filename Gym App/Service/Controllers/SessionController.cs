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
            if (result == 2) return Ok(new { Message = "Session created succcessfully" });
            else if (result == 1) return BadRequest(new { Message = "User(s) ID is wrong" });
            else return BadRequest(new { Message = "Faulty DTO given" });

        }
        [HttpDelete("DeleteSession")]
        public async Task<IActionResult> DeleteSession([FromBody] Guid sessionID)
        {
            var result = await _sessionService.DeleteSession(sessionID);
            if(result == 0) return BadRequest(new { message = "Session not Found" });
            return Ok(new { message = "Session deleted Succesfully" });
        }
        [HttpPost("AddMessages")]
        public async Task<IActionResult> AddMessages([FromBody] SessionMessagesDTO sessionMessages)
        {
            var result = await _sessionService.AddMessages(sessionMessages);
            if (result == 3) return Ok(new { Message = "Messages added succesfully" });
            else if (result == 2) return BadRequest(new { Message = "Messages either don't exist or they are already in session " });
            else if (result == 1) return BadRequest(new { Message = "Session not found" });
            else return BadRequest(new { Message = "Faulty DTO given" });
           
        }
        [HttpDelete("DeleteMessages")]
        public async Task<IActionResult> DeleteMessages([FromBody] SessionMessagesDTO sessionMessages)
        {
            var result = await _sessionService.DeleteMessages(sessionMessages);
            if (result == 3) return Ok(new { Message = "Messages deleted succesfully" });
            else if (result == 2) return BadRequest(new { Message = "Messages either don't exist or they are already in session" });
            else if (result == 1) return BadRequest(new { Message = "Session not found" });
            else return BadRequest(new { Message = "Faulty DTO given" });

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
