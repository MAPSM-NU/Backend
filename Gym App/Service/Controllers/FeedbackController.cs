using Gym_App.Domain.DTOs;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Service.Controllers
{
    [Route("[controller]")]
    public class FeedbackController : Controller
    {
        private readonly IFeedbackService _feedbackService;
        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }
        [HttpPost("CreateFeedback")]
        public async Task<IActionResult> CreateFeedback([FromBody] FeedbackDTO feedbackDTO)
        {
            var result = await _feedbackService.CreateFeedback(feedbackDTO);
            if(result == 2) return Ok(new { Message = "Feedback created successfully." });
            if(result == 1) return BadRequest(new { Message = "User or Workout not found." });
            return BadRequest(new { Message = "Faulty DTO was given" });
        }
        [HttpPut("UpdateFeedback")]
        public async Task<IActionResult> UpdateFeedback([FromBody] FeedbackDTO feedbackDTO)
        {
            var result = await _feedbackService.UpdateFeedback(feedbackDTO);
            if(result == 2) return Ok(new { Message = "Feedback updated successfully." });
            if(result == 1) return NotFound(new { Message = "Feedback not found." });
            return BadRequest(new { Message = "Faulty DTO was given" });
        }
        [HttpDelete("DeleteFeedback")]
        public async Task<IActionResult> DeleteFeedback([FromBody]Guid feedbackId)
        {
            var result = await _feedbackService.DeleteFeedback(feedbackId);
            if (result == 0) return BadRequest("Couldn't find the Feedback");
            return Ok("Feedback deleted successfully.");
        }
        [HttpGet("GetFeedbackByID")]
        public async Task<IActionResult> GetFeedbackByID([FromQuery]Guid feedbackId)
        {
            var feedback = await _feedbackService.GetFeedbackByID(feedbackId);
            if (feedback == null) return NotFound("Feedback not found.");
            return Ok(feedback);
        }
        [HttpGet("GetFeedbacksByFilter")]
        public async Task<IActionResult> GetFeedbacksByFilter([FromQuery] string sortColumn, string OrderBy, string SearchTerm, int page, int pageSize)
        {
            var feedbacks = await _feedbackService.GetFeedbackByFilter(page, sortColumn, OrderBy, SearchTerm, pageSize);
            return Ok(feedbacks);
        }
        [HttpGet("GetAllFeedbacks")]
        public async Task<IActionResult> GetAllFeedbacks([FromQuery] int page,int pageSize)
        {
            var feedbacks = await _feedbackService.GetAllFeedbacks(page,pageSize);
            return Ok(feedbacks);
        }
    }
}
