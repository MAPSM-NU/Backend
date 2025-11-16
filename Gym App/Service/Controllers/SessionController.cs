using Gym_App.Domain.DTOs;
using Gym_App.Service.Functions.Interfaces;
using MailKit.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Service.Controllers
{
    [Authorize(Policy ="NormalUsage")]
    [Route("[controller]")]
    public class SessionController : Controller
    {
        private readonly ISessionService _sessionService;
        private readonly IAuthorizationService _authorizationService;
        public SessionController(ISessionService sessionService,IAuthorizationService authorizationService)
        {
            _sessionService = sessionService;
            _authorizationService = authorizationService;
        }
        [HttpPost("CreateSession")]
        public async Task<IActionResult> CreateSession([FromBody] SessionDTO session)
        {
            //Authorization

            if (session == null) 
                return BadRequest(new { Message = "Faulty DTO given" });

            var authResult = await _authorizationService.AuthorizeAsync(User,session.UserIDs,"ListUserPolicy");
            if(!authResult.Succeeded) 
                return Forbid();

            //Talking to Database
            var result = await _sessionService.CreateSession(session);

            if (result == 2) 
                return Ok(new { Message = "Session created succcessfully" });
            else if (result == 1) 
                return BadRequest(new { Message = "User(s) ID is wrong" });
            else 
                return BadRequest(new { Message = "Faulty DTO given" });

        }
        [Authorize(Policy ="ElevatedPower")]//Only for admins to delete a session
        [HttpDelete("DeleteSession")]
        public async Task<IActionResult> DeleteSession([FromQuery] Guid sessionID)
        {
            var result = await _sessionService.DeleteSession(sessionID);

            if(result == 0) 
                return BadRequest(new { message = "Session not Found" });
            return Ok(new { message = "Session deleted Succesfully" });
        }
        [HttpPost("AddMessages")]
        public async Task<IActionResult> AddMessages([FromBody] SessionMessagesDTO sessionMessages)
        {
            //Authorization

            if (sessionMessages == null) 
                return BadRequest(new { Message = "Faulty DTO given" });

            var UserIDs = await _sessionService.GetSessionUsersIDs(sessionMessages.SessionID);
            if (UserIDs == null)
                return BadRequest(new { Message = "Session not found" });

            var authResult = await _authorizationService.AuthorizeAsync(User,UserIDs,"ListUserPolicy");
            if(!authResult.Succeeded) 
                return Forbid();

            //Talking to Database
            var result = await _sessionService.AddMessages(sessionMessages);
            if (result == 3) 
                return Ok(new { Message = "Messages added succesfully" });
            else if (result == 2) 
                return BadRequest(new { Message = "Messages either don't exist or they are already in session " });
            else if (result == 1) 
                return BadRequest(new { Message = "Session not found" });
            else 
                return BadRequest(new { Message = "Faulty DTO given" });
           
        }
        [HttpDelete("DeleteMessages")]
        public async Task<IActionResult> DeleteMessages([FromBody] SessionMessagesDTO sessionMessages)
        {//I honestly don't know what to do here for auhtorization
            var result = await _sessionService.DeleteMessages(sessionMessages);
            if (result == 4) 
                return Ok(new { Message = "Messages deleted succesfully" });
            else if (result == 3) 
                return BadRequest(new { Message = "Messages either don't exist or they are not in session" });
            else if (result == 2) 
                return BadRequest(new { Message = "Session has no messages" });
            else if (result == 1)
                return BadRequest(new { Message = "Session not found" });
            else
                return BadRequest(new { Message = "Faulty DTO given" });

        }
        [HttpGet("GetSessionMessages")]
        public async Task<IActionResult> GetSessionMessages([FromQuery] Guid sessionID, string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize)
        {
            //Authorization
            if(sessionID == Guid.Empty) 
                return BadRequest(new { Message = "Faulty Session ID given" });

            var UserIDs = await _sessionService.GetSessionUsersIDs(sessionID);
            if (UserIDs == null)
                return BadRequest(new { Message = "Session not found" });

            var authResult = await _authorizationService.AuthorizeAsync(User,UserIDs,"ListUserPolicy");
            if(!authResult.Succeeded) 
                return Forbid();

            //Talking to Database
            var messages = await _sessionService.GetSessionMessages(sessionID,startDate, endDate, page, sortColumn, OrderBy, searchTerm, pageSize);
            if (messages == null) 
                return NotFound(new { message = "No messages found for this session" });
            return Ok(messages);
        }
        [Authorize(Policy ="ElevatedPower")]//Only for admins to get all users of a session
        [HttpGet("GetUsersOfSession")]
        public async Task<IActionResult> GetUsersOfSession([FromQuery] Guid sessionID,int page,int pageSize)
        {
            var users = await _sessionService.GetUsersOfSession(sessionID,page,pageSize);
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
