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
            if (result == 0) return BadRequest("Failed to create feedback.");
            return Ok("Feedback created successfully.");
        }
        [HttpPut("UpdateFeedback")]
        public async Task<IActionResult> UpdateFeedback([FromBody] FeedbackDTO feedbackDTO)
        {
            var result = await _feedbackService.UpdateFeedback(feedbackDTO);
            if (result == 0) return BadRequest("Failed to update feedback.");
            return Ok("Feedback updated successfully.");
        }
        [HttpDelete("DeleteFeedback")]
        public async Task<IActionResult> DeleteFeedback([FromBody]Guid feedbackId)
        {
            var result = await _feedbackService.DeleteFeedback(feedbackId);
            if (result == 0) return BadRequest("Failed to delete feedback.");
            return Ok("Feedback deleted successfully.");
        }
        [HttpPost("GetFeedbackByID")]
        public async Task<IActionResult> GetFeedbackByID([FromBody]Guid feedbackId)
        {
            var feedback = await _feedbackService.GetFeedbackByID(feedbackId);
            if (feedback == null) return NotFound("Feedback not found.");
            return Ok(feedback);
        }
        [HttpGet("GetAllFeedbacks")]
        public async Task<IActionResult> GetAllFeedbacks()
        {
            var feedbacks = await _feedbackService.GetAllFeedbacks();
            return Ok(feedbacks);
        }
    }
}
