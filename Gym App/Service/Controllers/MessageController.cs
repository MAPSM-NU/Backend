using DocumentFormat.OpenXml.Wordprocessing;
using Gym_App.Domain.DTOs;
using Gym_App.Service.Functions.Interfaces;
using MailKit.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Service.Controllers
{
    [Authorize(Policy ="NormalUsage")]
    [Route("[controller]")]
    public class MessageController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly IAuthorizationService _authorizationService;
        public MessageController(IMessageService messageService, IAuthorizationService authorizationService)
        {
            _messageService = messageService;
            _authorizationService = authorizationService;
        }
        [HttpPost("AddMessage")]
        public async Task<IActionResult> AddMessage([FromBody] MessageDTO message)
        {
            //Authorize
            if (message == null)
                return BadRequest(new { message = "message was empty" });

            var authResult = await _authorizationService.AuthorizeAsync(User, message.SenderID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return Forbid();

            //Talking to Database
            var result = await _messageService.AddMessage(message);
            if (result == 2)
                return Ok(new { Message = "Message succesfully created" });
            if (result == 1)
                return BadRequest(new { Message = "Session not found" });
            else 
                return BadRequest(new {Message = "User not found"});
        }
        [HttpDelete("DeleteMessage")]
        public async Task<IActionResult> DeleteMessage([FromQuery] Guid messageID)
        {
            //Authorize
            if (messageID == Guid.Empty)
                return BadRequest(new { message = "message was empty" });

            var userID = await _messageService.GetMessageUserID(messageID);
            if (userID == Guid.Empty)
                return BadRequest(new { message = "User not found" });

            var authResult = await _authorizationService.AuthorizeAsync(User, userID, "SameUserPolicy");
            if(!authResult.Succeeded)
                return Forbid();

            //Talking to database
            var result = await _messageService.DeleteMessage(messageID);
            if (result == 0) 
                return BadRequest(new { message = "Couldn't find the message" });
            return
                Ok(new { message = "Message Deleted Successfully" });
        }
        [HttpPut("UpdateMessage")]
        public async Task<IActionResult> UpdateMessage([FromBody] MessageDTO message)
        {
            //Authorize
            if(message == null)
                return BadRequest(new { message = "message was empty" });

            if(message.SenderID == Guid.Empty)
                return BadRequest(new { message = "SenderID was empty" });

            var authResult = await _authorizationService.AuthorizeAsync(User, message.SenderID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return Forbid();

            //Talking to database
            var result = await _messageService.UpdateMessage(message);
            if (result == 0) return BadRequest(new { message = "Failed to update message" });
            return Ok(new { message = "Message Updated Successfully" });
        }
        [HttpGet("GetSessionMessages")]//2 Userpolicy bc both sender and receiver can see messages
        public async Task<IActionResult> GetSessionMessages([FromQuery] Guid sessionID, string startDate, string endDate, string sortColumn, string OrderBy, string searchTerm, int page, int pageSize)
        {
            //Authorization
            if (sessionID == Guid.Empty)
                return BadRequest(new { message = "sessionID empty" });
            var userIDs = await _messageService.GetSessionUsersIDs(sessionID);

            if (userIDs == null)
                return BadRequest(new { message = "Session does not exist" });
            //return Ok(userIDs);
            var authResult = await _authorizationService.AuthorizeAsync(User, userIDs, "ListUserPolicy");
            if (!authResult.Succeeded)
                return Forbid();

            //Talking to Database
            var messages = await _messageService.GetSessionMessages(sessionID, startDate, endDate, page, sortColumn, OrderBy, searchTerm, pageSize);
            if (messages == null) 
                return NotFound(new { message = "No messages found for this session" });
            return Ok(messages);
        }
        [HttpGet("GetMessagesByFilter")]
        public async Task<IActionResult> GetMessagesByFilter([FromQuery]string startDate,string endDate, string sortColumn, string OrderBy, string SearchTerm, int page, int pageSize)
        {
            var messages = await _messageService.GetMessagesByFilter(startDate,endDate, page, sortColumn, OrderBy, SearchTerm, pageSize);
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
