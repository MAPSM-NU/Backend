using Gym_App.Domain;
using Gym_App.Infastructure.DTOs;
using Gym_App.Infastructure.DTOs.Session;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infrastructure.DTOs.Session;
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
        [HttpPost("create")]
        public async Task<IActionResult> CreateSession([FromBody] SessionUsersDTO Users)
        {
            var result = await _sessionService.CreateSession(Users);

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
            var result = await _sessionService.DeleteSession(sessionID);
            
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
            var result = await _sessionService.AddMessages(sessionID, sessionMessages);
            
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
            var result = await _sessionService.DeleteMessages(sessionID, sessionMessages);
            
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
            var result = await _sessionService.GetSessionMessages(sessionID,startDate, endDate, page, sortColumn, OrderBy, searchTerm, pageSize);
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Data);
        }
        //[Authorize(Policy = "ElevatedPower")]//Only for admins to get all users of a session
        [HttpGet("users/{sessionID}")]
        public async Task<IActionResult> GetUsersOfSession([FromRoute] Guid sessionID,[FromQuery] int page = 1,int pageSize = 10)
        {
            var result = await _sessionService.GetUsersOfSession(sessionID,page,pageSize);
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
