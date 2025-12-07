using Gym_App.Application.Interfaces;
using Gym_App.Infastructure.DTOs;
using Gym_App.Infastructure.DTOs.Session;
using MailKit.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Api.Controllers
{
    [Authorize(Policy ="NormalUsage")]
    [Route("[controller]")]
    public class SessionController : Controller
    {
        private readonly ISessionService _sessionService;
        public SessionController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }
        [HttpPost("CreateSession")]
        public async Task<IActionResult> CreateSession([FromQuery] Guid user1,Guid user2)
        {
            var result = await _sessionService.CreateSession(User,user1,user2);

            if (result == 3)
                return Ok(new { Message = "Session created succcessfully" });
            else if (result == 2)
                return Forbid();
            else if (result == 1)
                return BadRequest(new { Message = "User(s) ID is wrong" });
            else
                return BadRequest(new { Message = "Faulty DTO given" });

        }
        [Authorize(Policy = "ElevatedPower")]//Only for admins to delete a session
        [HttpDelete("DeleteSession")]
        public async Task<IActionResult> DeleteSession([FromQuery] Guid sessionID)
        {
            var result = await _sessionService.DeleteSession(User,sessionID);
            if (result == 1)
                return Forbid();
            else if(result == 0) 
                return BadRequest(new { message = "Session not Found" });
            return Ok(new { message = "Session deleted Succesfully" });
        }
        [HttpPost("AddMessages")]
        public async Task<IActionResult> AddMessages([FromQuery] Guid sessionID, [FromBody] List<Guid> messages)
        {
            var result = await _sessionService.AddMessages(User, sessionID, messages);
            if (result == 5)
                return Ok(new { Message = "Messages added succesfully" });
            else if (result == 4)
                return BadRequest(new { Message = "no messages found" });
            else if (result == 3)
                return BadRequest(new { Message = "Messages are already in the session " });
            else if (result == 2)
                return Forbid();
            else if (result == 1)
                return BadRequest(new { Message = "Session not found" });
            else
                return BadRequest(new { Message = "Faulty DTO given" });
           
        }
        [HttpDelete("DeleteMessages")]
        public async Task<IActionResult> DeleteMessages([FromQuery] Guid sessionID, [FromBody] List<Guid> messages)
        {
            var result = await _sessionService.DeleteMessages(User, sessionID, messages);
            if (result == 6) 
                return Ok(new { Message = "Messages deleted succesfully" });
            else if (result == 5)
                return BadRequest(new { Message = "no messages were found" });
            else if (result == 4)
                return BadRequest(new { Message = "Messages either don't exist or they are not in session" });
            else if (result == 3)
                return BadRequest(new { Message = "Session has no messages" });
            else if (result == 2)
                return Forbid();
            else if (result == 1)
                return BadRequest(new { Message = "Session not found" });
            else
                return BadRequest(new { Message = "Faulty DTO given" });

        }
        [HttpGet("GetSessionMessages")]
        public async Task<IActionResult> GetSessionMessages([FromQuery] Guid sessionID, string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize)
        {
            var messages = await _sessionService.GetSessionMessages(User,sessionID,startDate, endDate, page, sortColumn, OrderBy, searchTerm, pageSize);
            if (messages == null) 
                return NotFound(new { message = "either you are not permitted to view the messages of this session or this" +
                    " session has no messages" });
            return Ok(messages);
        }
        [Authorize(Policy = "ElevatedPower")]//Only for admins to get all users of a session
        [HttpGet("GetUsersOfSession")]
        public async Task<IActionResult> GetUsersOfSession([FromQuery] Guid sessionID,int page,int pageSize)
        {
            var users = await _sessionService.GetUsersOfSession(User,sessionID,page,pageSize);
            if (users == null) 
                return NotFound(new { message = "No users found for this session" });
            return Ok(users);
        }
        //[Authorize(Policy ="ElevatedPower")]//Only for admins to get all sessions
        [HttpGet("GetAllSessions")]
        public async Task<IActionResult> GetAllSessions([FromQuery] int page,int pageSize)
        {
            var sessions = await _sessionService.GetAllSessions(page,pageSize);
            if (sessions == null) 
                return NotFound(new { message = "No sessions found" });
            return Ok(sessions);
        }
    }
}
