using Gym_App.Domain.DTOs;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Service.Controllers
{
    [Authorize(Policy = "ElevatedPower")]
    [Route("[controller]")]
    public class MusclesController : Controller
    {
        private readonly IMuscleService _muscleService;
        public MusclesController(IMuscleService muscleService)
        {
            _muscleService = muscleService;
        }
        [HttpPost("AddMuscle")]
        public async Task<IActionResult> AddMuscle([FromBody] MuscleDTO muscle)
        {
            var result = await _muscleService.CreateMuscle(muscle);
            if(result == 2) 
                return Ok(new { Message = "Muscle added successfully"});
            if(result == 1)
                return BadRequest(new { Message = "Muscle already exists" });
            return BadRequest(new { Message = "Faulty DTO given" });
        }
        
        [HttpPut("UpdateMuscle")]
        public async Task<IActionResult> UpdateMuscle([FromBody] MuscleDTO muscle)
        {
            var result = await _muscleService.UpdateMuscle(muscle);
            if (result > 0)
                return Ok(new { Message = "Muscle Updated successfully"});

            return BadRequest(new { Message = "Couldn't find the given muscle" });
        }
        [HttpDelete("DeleteMuscle")]
        public async Task<IActionResult> DeleteMuscle([FromBody] Guid muscleId)
        {
            var result = await _muscleService.DeleteMuscle(muscleId);
            if (result > 0)
                return Ok(new { Message = "Muscle deleted successfully" });

            return BadRequest(new { Message = "Couldn't find the given muscle" });
        }
        [HttpGet("GetAllMuscles")]
        public async Task<IActionResult> GetAllMuscles(int page, int pageSize)
        {
            var result = await _muscleService.GetAllMuscles(page, pageSize);
            return Ok(result);
        }
    }
}
