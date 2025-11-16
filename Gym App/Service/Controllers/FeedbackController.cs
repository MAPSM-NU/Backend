using Gym_App.Domain.DTOs;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Service.Controllers
{
    [Authorize(Policy = "NormalUsage")]
    [Route("[controller]")]
    public class FeedbackController : Controller
    {
        private readonly IFeedbackService _feedbackService;
        private readonly IAuthorizationService _authorizationService;
        public FeedbackController(IFeedbackService feedbackService,IAuthorizationService authorizationService)
        {
            _feedbackService = feedbackService;
            _authorizationService = authorizationService;
        }
        [HttpPost("CreateFeedback")]
        public async Task<IActionResult> CreateFeedback([FromBody] FeedbackDTO feedbackDTO)
        {
            //Authorization

            if (feedbackDTO == null)
                return BadRequest(new { Message = "Faulty DTO was given" });

            var authResult = await _authorizationService.AuthorizeAsync(User, feedbackDTO.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return Forbid();

            //Talking to Database
            var result = await _feedbackService.CreateFeedback(feedbackDTO);
            if(result == 2) 
                return Ok(new { Message = "Feedback created successfully." });
            if(result == 1)
                return BadRequest(new { Message = "User or Workout not found." });
            return BadRequest(new { Message = "Faulty DTO was given" });
        }
        [HttpPut("UpdateFeedback")]
        public async Task<IActionResult> UpdateFeedback([FromBody] FeedbackDTO feedbackDTO)
        {
            //Authorization

            if (feedbackDTO == null)
                return BadRequest(new { Message = "Faulty DTO was given" });

            var authResult = await _authorizationService.AuthorizeAsync(User, feedbackDTO.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return Forbid();

            //Talking to Database
            var result = await _feedbackService.UpdateFeedback(feedbackDTO);
            if(result == 2) 
                return Ok(new { Message = "Feedback updated successfully." });
            if(result == 1) 
                return NotFound(new { Message = "Feedback not found." });
            return 
                BadRequest(new { Message = "Faulty DTO was given" });
        }
        [HttpDelete("DeleteFeedback")]
        public async Task<IActionResult> DeleteFeedback([FromQuery]Guid feedbackID)
        {
            //Authorization

            var userID = await _feedbackService.GetFeedbackUserID(feedbackID);
            if(userID == Guid.Empty)
                return NotFound(new { Message = "Feedback not found." });

            var authResult = await _authorizationService.AuthorizeAsync(User, userID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return Forbid();

            //Talking to Database
            var result = await _feedbackService.DeleteFeedback(feedbackID);
            if (result == 0) 
                return BadRequest("Couldn't find the Feedback");
            return Ok("Feedback deleted successfully.");
        }
        [HttpGet("GetFeedbackByID")]
        public async Task<IActionResult> GetFeedbackByID([FromQuery]Guid feedbackId)
        {
            //Talking to Database

            var feedback = await _feedbackService.GetFeedbackByID(feedbackId);
            if (feedback == null) 
                return NotFound("Feedback not found.");

            //Authorize
            var authResult = await _authorizationService.AuthorizeAsync(User, feedback.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return Forbid();

            return Ok(feedback);
        }
        [HttpGet("GetFeedbacksByFilter")]
        public async Task<IActionResult> GetFeedbacksByFilter([FromQuery]string startDate,string endDate,string sortColumn, string OrderBy, string SearchTerm, int page, int pageSize)
        {//startDate and endDate are string so I can parse and check them
            var feedbacks = await _feedbackService.GetFeedbackByFilter(startDate, endDate, page, sortColumn, OrderBy, SearchTerm, pageSize);
            return Ok(feedbacks);
        }
        //[Authorize(Policy = "ElevatedPower")]
        [HttpGet("GetAllFeedbacks")]
        public async Task<IActionResult> GetAllFeedbacks([FromQuery] int page,int pageSize)
        {
            var feedbacks = await _feedbackService.GetAllFeedbacks(page,pageSize);
            return Ok(feedbacks);
        }
    }
}
