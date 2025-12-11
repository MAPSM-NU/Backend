using Gym_App.Application.Interfaces;
using Gym_App.Infastructure.DTOs.Muscle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Api.Controllers
{
    //[Authorize(Policy = "ElevatedPower")]
    [Route("[controller]")]
    public class MusclesController : Controller
    {
        private readonly IMuscleService _muscleService;
        public MusclesController(IMuscleService muscleService)
        {
            _muscleService = muscleService;
        }
        [HttpPost("AddMuscle")]
        public async Task<IActionResult> AddMuscle([FromBody] MuscleCreationAndEditDTO muscle)
        {
            var result = await _muscleService.CreateMuscle(muscle);
            
            if(result.status == 0)
                return BadRequest(new {message = result.msg});
            else
                return Ok(new {message = result.msg});
        }
        
        [HttpPut("UpdateMuscle")]
        public async Task<IActionResult> UpdateMuscle([FromQuery]Guid muscleID,[FromBody] MuscleCreationAndEditDTO muscle)
        {
            var result = await _muscleService.UpdateMuscle(muscleID,muscle);
            
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else
                return Ok(new { message = result.msg });
        }
        [HttpDelete("DeleteMuscle")]
        public async Task<IActionResult> DeleteMuscle([FromQuery] Guid muscleID)
        {
            var result = await _muscleService.DeleteMuscle(muscleID);
            
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else
                return Ok(new { message = result.msg });
        }
        [HttpGet("GetAllMuscles")]
        public async Task<IActionResult> GetAllMuscles(int page, int pageSize)
        {
            var result = await _muscleService.GetAllMuscles(page, pageSize);
            return Ok(result);
        }
    }
}
