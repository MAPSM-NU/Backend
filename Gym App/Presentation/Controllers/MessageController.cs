using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Gym_App.Domain;
using Gym_App.Infastructure.DTOs.Message;
using Gym_App.Infastructure.Interfaces.Services;
using MailKit.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Api.Controllers
{
    [Authorize(Policy ="NormalUsage")]
    [Route("api/v1/message")]
    public class MessageController : Controller
    {
        private readonly IMessageService _messageService;
        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }
        [HttpPost("send/{userID}")]
        public async Task<IActionResult> AddMessage([FromRoute]Guid userID,[FromBody] MessageCreationDTO message)
        {
            var result = await _messageService.AddMessage(User,userID, message);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpPut("update/{messageID}")]
        public async Task<IActionResult> UpdateMessage([FromRoute] Guid messageID, [FromBody] MessageUpdateDTO message)
        {
            var result = await _messageService.UpdateMessage(User, messageID, message);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpDelete("delete/{messageID}")]
        public async Task<IActionResult> DeleteMessage([FromRoute] Guid messageID)
        {
            var result = await _messageService.DeleteMessage(User, messageID);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpGet("session-messages/{sessionID}")]
        public async Task<IActionResult> GetSessionMessages([FromRoute] Guid sessionID, string startDate, string endDate, string sortColumn, string OrderBy, string searchTerm, int page = 1, int pageSize = 10)
        {
            var result = await _messageService.GetSessionMessages(User,sessionID, startDate, endDate, page, sortColumn, OrderBy, searchTerm, pageSize);
            if(result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Data);
        }
        //[Authorize(Policy = "ElevatedPower")]
        [HttpGet("messages-filter")]
        public async Task<IActionResult> GetMessagesByFilter([FromQuery]string startDate,string endDate, string sortColumn, string OrderBy, string SearchTerm, int page = 1, int pageSize = 10)
        {
            var result = await _messageService.GetMessagesByFilter(startDate,endDate, page, sortColumn, OrderBy, SearchTerm, pageSize);
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Data);
        }
        //[Authorize(Policy = "ElevatedPower")]
        [HttpGet("get")]
        public async Task<IActionResult> GetAllMessages([FromQuery]int page = 1, int pageSize = 10)
        {
            var result = await _messageService.GetMessages(page,pageSize);
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Data);
        }
    }
}
