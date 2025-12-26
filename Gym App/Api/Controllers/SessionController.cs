using Gym_App.Application.Interfaces;
using Gym_App.Infastructure.DTOs;
using Gym_App.Infastructure.DTOs.Session;
using MailKit.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Api.Controllers
{
    [Authorize(Policy ="NormalUsage")]
    [Route("api/v1/session")]
    public class SessionController : Controller
    {
        private readonly ISessionService _sessionService;
        public SessionController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }
        [HttpPost("create/{user1}/{user2}")]
        public async Task<IActionResult> CreateSession([FromRoute] Guid user1,[FromRoute] Guid user2)
        {
            var result = await _sessionService.CreateSession(User,user1,user2);

            if(result.status == 0)
                return BadRequest(new { message = result.msg });
            else if(result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [Authorize(Policy = "ElevatedPower")]//Only for admins to delete a session
        [HttpDelete("delete/{sessionID}")]
        public async Task<IActionResult> DeleteSession([FromRoute] Guid sessionID)
        {
            var result = await _sessionService.DeleteSession(User,sessionID);
            
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpPost("add-messages/{sessionID}")]
        public async Task<IActionResult> AddMessages([FromRoute] Guid sessionID, [FromBody] SessionMessagesDTO sessionMessages)
        {
            var result = await _sessionService.AddMessages(User, sessionID, sessionMessages);
            
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });

        }
        [HttpDelete("delete-messages/{sessionID}")]
        public async Task<IActionResult> DeleteMessages([FromRoute] Guid sessionID, [FromBody] SessionMessagesDTO sessionMessages)
        {
            var result = await _sessionService.DeleteMessages(User, sessionID, sessionMessages);
            
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });

        }
        [HttpGet("session-messages/{sessionID}")]
        public async Task<IActionResult> GetSessionMessages([FromRoute] Guid sessionID,[FromQuery] string startDate, string endDate,string sortColumn, string OrderBy, string searchTerm,int page = 1, int pageSize = 10)
        {
            var result = await _sessionService.GetSessionMessages(User,sessionID,startDate, endDate, page, sortColumn, OrderBy, searchTerm, pageSize);
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Data);
        }
        [Authorize(Policy = "ElevatedPower")]//Only for admins to get all users of a session
        [HttpGet("users/{sessionID}")]
        public async Task<IActionResult> GetUsersOfSession([FromRoute] Guid sessionID,[FromQuery] int page = 1,int pageSize = 10)
        {
            var result = await _sessionService.GetUsersOfSession(User,sessionID,page,pageSize);
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Data);
        }
        //[Authorize(Policy ="ElevatedPower")]//Only for admins to get all sessions
        [HttpGet("get")]
        public async Task<IActionResult> GetAllSessions([FromQuery] int page = 1 ,int pageSize = 10)
        {
            var result = await _sessionService.GetAllSessions(page,pageSize);
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Data);
        }
    }
}
