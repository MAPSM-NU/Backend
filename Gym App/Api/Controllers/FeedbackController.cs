using Gym_App.Application.Interfaces;
using Gym_App.Infastructure.DTOs.Feedback;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Api.Controllers
{
    [Authorize(Policy = "NormalUsage")]
    [Route("[controller]")]
    public class FeedbackController : Controller
    {
        private readonly IFeedbackService _feedbackService;
        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }
        [HttpPost("CreateFeedback")]
        public async Task<IActionResult> CreateFeedback([FromQuery]Guid userID,[FromBody] FeedbackCreationDTO feedbackDTO)
        {
            var result = await _feedbackService.CreateFeedback(User,userID, feedbackDTO);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpPut("UpdateFeedback")]
        public async Task<IActionResult> UpdateFeedback([FromQuery]Guid feedbackID,[FromBody] FeedbackUpdateDTO feedbackDTO)
        {
            var result = await _feedbackService.UpdateFeedback(User,feedbackID, feedbackDTO);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpDelete("DeleteFeedback")]
        public async Task<IActionResult> DeleteFeedback([FromQuery]Guid feedbackID)
        {
            var result = await _feedbackService.DeleteFeedback(User, feedbackID);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpGet("GetFeedbackByID")]
        public async Task<IActionResult> GetFeedbackByID([FromQuery]Guid feedbackId)
        {
            //Talking to Database

            var feedback = await _feedbackService.GetFeedbackByID(User,feedbackId);
            if (feedback == null) 
                return BadRequest("Feedback not found or not authorized");

            return Ok(feedback);
        }
        [HttpGet("GetFeedbackOfWorkout")]
        public async Task<IActionResult> GetFeedbackOfWorkout([FromQuery]Guid workoutID)
        {
            var feedback = await _feedbackService.GetFeedbackOfWorkout(User, workoutID);
            if (feedback == null)
                return BadRequest("Feedback not found or not authorized");
            return Ok(feedback);
        }
        [HttpGet("GetUserFeedbacks")]
        public async Task<IActionResult> GetUserFeedbacks([FromQuery]Guid UserID, string startDate,string endDate,string sortColumn, string OrderBy, string SearchTerm, int page, int pageSize)
        {//startDate and endDate are string so I can parse and check them
            var feedbacks = await _feedbackService.GetUserFeedbacks(User, UserID,startDate, endDate, page, sortColumn, OrderBy, SearchTerm, pageSize);
            if (feedbacks == null)
                return BadRequest("No feedbacks found or not authorized");
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
