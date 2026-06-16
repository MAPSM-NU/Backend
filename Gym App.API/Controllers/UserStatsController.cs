using Gym_App.Infrastructure.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Controllers
{
    [Authorize(Policy = "NormalUsage")]
    [Route("api/v1/user-stats")]
    public class UserStatsController : Controller
    {
        private readonly IUserStatsService _stats;
        public UserStatsController(IUserStatsService stats)
        {
            _stats = stats;
        }
        [HttpGet("daily-stat/{userId}")]
        public async Task<IActionResult> GetDailyResult([FromRoute]Guid userId, [FromQuery] DateOnly date)
        {
            var result = await _stats.GetDailyStats(userId,date);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Value);
        }
        [HttpGet("weekly-stat/{userId}")]
        public async Task<IActionResult> GetWeeklyResult([FromRoute] Guid userId, [FromQuery] int weekNumber, int year)
        {
            var result = await _stats.GetWeeklyStats(userId, weekNumber, year);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Value);
        }
        [HttpGet("monthly-stat/{userId}")]
        public async Task<IActionResult> GetmonthlyResult([FromRoute] Guid userId, [FromQuery] string monthName, int year)
        {
            var result = await _stats.GetMonthlyStats(userId, monthName, year);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Value);
        }
        [HttpGet("all-time-stats/{userId}")]
        public async Task<IActionResult> GetmonthlyResult([FromRoute] Guid userId)
        {
            var result = await _stats.GetOverallStats(userId);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Value);
        }

    }
}
