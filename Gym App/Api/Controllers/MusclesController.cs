using Gym_App.Application.Interfaces;
using Gym_App.Infastructure.DTOs.Muscle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Api.Controllers
{
    //[Authorize(Policy = "ElevatedPower")]
    [Route("api/v1/muscle")]
    public class MusclesController : Controller
    {
        private readonly IMuscleService _muscleService;
        public MusclesController(IMuscleService muscleService)
        {
            _muscleService = muscleService;
        }
        [HttpPost("create")]
        public async Task<IActionResult> AddMuscle([FromBody] MuscleCreationAndEditDTO muscle)
        {
            var result = await _muscleService.CreateMuscle(muscle);
            
            if(result.status == 0)
                return BadRequest(new {message = result.msg});
            else
                return Ok(new {message = result.msg});
        }
        
        [HttpPut("update/{muscleID}")]
        public async Task<IActionResult> UpdateMuscle([FromRoute]Guid muscleID,[FromBody] MuscleCreationAndEditDTO muscle)
        {
            var result = await _muscleService.UpdateMuscle(muscleID,muscle);
            
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else
                return Ok(new { message = result.msg });
        }
        [HttpDelete("delete/{muscleID}")]
        public async Task<IActionResult> DeleteMuscle([FromRoute] Guid muscleID)
        {
            var result = await _muscleService.DeleteMuscle(muscleID);
            
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else
                return Ok(new { message = result.msg });
        }
        [HttpGet("get")]
        public async Task<IActionResult> GetAllMuscles(int page = 1, int pageSize = 15)
        {
            var result = await _muscleService.GetAllMuscles(page, pageSize);
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Data);
        }
    }
}
