using Gym_App.Service.Functions.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MusclesController : Controller
    {
        private readonly IMuscleService _muscleService;
        public MusclesController(IMuscleService muscleService)
        {
            _muscleService = muscleService;
        }
        [HttpPost]
        [Route("/Add Muscle")]
        public async Task<IActionResult> AddMuscle([FromBody] Domain.DTOs.MuscleDTO muscle)
        {
            var result = await _muscleService.CreateMuscle(muscle);
            if (result > 0)
            {
                return Ok(new { Message = "Muscle added successfully", Result = result });
            }
            return BadRequest(new { Message = "Failed to add muscle" });
        }
        [HttpDelete]
        [Route("/Delete Muscle")]
        public async Task<IActionResult> DeleteMuscle([FromBody] Guid muscleId)
        {
            var result = await _muscleService.DeleteMuscle(muscleId);
            if (result > 0)
            {
                return Ok(new { Message = "Muscle deleted successfully", Result = result });
            }
            return BadRequest(new { Message = "Failed to delete muscle" });
        }
        [HttpPut]
        [Route("/Modify Muscle")]
        public async Task<IActionResult> ModifyMuscle([FromBody] Domain.DTOs.MuscleDTO muscle)
        {
            var result = await _muscleService.UpdateMuscle(muscle);
            if (result > 0)
            {
                return Ok(new { Message = "Muscle modified successfully", Result = result });
            }
            return BadRequest(new { Message = "Failed to modify muscle" });
        }
        [HttpGet]
        [Route("/Get All Muscles")]
        public async Task<IActionResult> GetAllMuscles()
        {
            var result = await _muscleService.GetAllMuscles();
            return Ok(new { Message = "Muscles retrieved successfully", Result = result });
        }
    }
}
