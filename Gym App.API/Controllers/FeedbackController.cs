using Gym_App.Infastructure.DTOs.Feedback;
using Gym_App.Infastructure.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Api.Controllers
{
    [Authorize(Policy = "NormalUsage")]
    [Route("api/v1/feedback")]
    public class FeedbackController : Controller
    {
        private readonly IFeedbackService _feedbackService;
        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }
        [HttpPost("create/{userID}")]
        public async Task<IActionResult> CreateFeedback([FromRoute]Guid userID,[FromBody] FeedbackCreationDTO feedbackDTO)
        {
            var result = await _feedbackService.CreateFeedback(userID, feedbackDTO);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpPut("update/{feedbackID}")]
        public async Task<IActionResult> UpdateFeedback([FromRoute]Guid feedbackID,[FromBody] FeedbackUpdateDTO feedbackDTO)
        {
            var result = await _feedbackService.UpdateFeedback(feedbackID, feedbackDTO);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpDelete("delete/{feedbackID}")]
        public async Task<IActionResult> DeleteFeedback([FromRoute]Guid feedbackID)
        {
            var result = await _feedbackService.DeleteFeedback(feedbackID);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpGet("get/{feedbackID}")]
        public async Task<IActionResult> GetFeedbackByID([FromRoute]Guid feedbackID)
        {
            //Talking to Database

            var result = await _feedbackService.GetFeedbackByID(feedbackID);
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Value);
        }
        [HttpGet("get-workout-feedback/{workoutID}")]
        public async Task<IActionResult> GetFeedbackOfWorkout([FromRoute]Guid workoutID)
        {
            var result = await _feedbackService.GetFeedbackOfWorkout(workoutID);
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Value);
        }
        [HttpGet("get-user-feedbacks/{userID}")]
        public async Task<IActionResult> GetUserFeedbacks([FromRoute]Guid userID,[FromQuery] string startDate,string endDate,string sortColumn, string OrderBy, string SearchTerm, int page = 1, int pageSize = 10)
        {//startDate and endDate are string so I can parse and check them
            var result = await _feedbackService.GetUserFeedbacks(userID,startDate, endDate, page, sortColumn, OrderBy, SearchTerm, pageSize);
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Data);
        }
        //[Authorize(Policy = "ElevatedPower")]
        [HttpGet("get")]
        public async Task<IActionResult> GetAllFeedbacks([FromQuery] int page = 1,int pageSize = 10)
        {
            var result = await _feedbackService.GetAllFeedbacks(page,pageSize);
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Data);
        }
    }
}
