using Gym_App.Domain.DTOs;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Service.Controllers
{
    [Route("[controller]")]
    public class ExerciseController : Controller
    {
        private readonly IExerciseService _exerciseService;
        public ExerciseController(IExerciseService exerciseService)
        {
            _exerciseService = exerciseService;
        }

        [HttpPost("AddExercise")]
        public async Task<IActionResult> AddExercise([FromBody] ExerciseDTO exercise)
        {
            var result = await _exerciseService.CreateExercise(exercise);
            if (result == 2) return Ok(new { Message = "Exercise Added Succesfully" });

            else if (result == 1) return BadRequest(new { Message = "The Exercise already exists" });

            else return BadRequest(new { Message = "Exercise or Exercise Name cannot be null" });
        }

        [HttpDelete("DeleteExercise")]
        public async Task<IActionResult> DeleteExercise([FromBody] Guid exerciseId)
        {
            var result = await _exerciseService.DeleteExercise(exerciseId);
            if (result > 0) return Ok(new { Message = "Exercise deleted successfully", Result = result });

            return BadRequest(new { Message = "Exercise does not exist" });
        }

        [HttpPost("UpdateExercise")]
        public async Task<IActionResult> UpdateExercise([FromBody] ExerciseDTO exercise)
        {
            var result = await _exerciseService.UpdateExercise(exercise);
            if(result > 0) return Ok(new { Message = "Exercise modified successfully", Result = result });

            return BadRequest(new { Message = "Failed to Modify Exercise" });
        }

        [HttpPost("AddMusclesToExercise")]
        public async Task<IActionResult> AddMusclesToExercise([FromBody] ExerciseMusclesDTO Muscles)
        {
            var result = await _exerciseService.AddMusclesToExercise(Muscles);
            if(result == 4) return Ok(new { Message = "Muscles added to exercise successfully" });
            else if(result == 3) return BadRequest(new { Message = "Wrong IDs given for the Muscles" });
            else if(result == 2) return BadRequest(new { Message = "Given muscles are already in the Exercise" });
            else if(result == 1) return BadRequest(new { Message = "Given exercise does not exist" });
            else return BadRequest(new { Message = "Faulty DTO" });
        }

        [HttpDelete("RemoveMusclesFromExercise")]
        public async Task<IActionResult> RemoveMusclesFromExercise([FromBody] ExerciseMusclesDTO Muscles)//fe 8alta hena
        {
            var result = await _exerciseService.RemoveMusclesFromExercise(Muscles);
            if(result == 4) return Ok(new { Message = "Muscles removed from exercise successfully" });
            else if(result == 3) return BadRequest(new { Message = "Wrong IDs given for the Muscles" });
            else if(result == 2) return BadRequest(new { Message = "Given muscles are not in the Exercise" });
            else if(result == 1) return BadRequest(new { Message = "Given exercise does not exist" });
            else return BadRequest(new { Message = "Faulty DTO" });
        }

        [HttpGet("GetExerciseByName")]
        public async Task<IActionResult> GetExerciseByName([FromQuery] string name)
        {
            var result = await _exerciseService.GetExerciseByName(name);
            if (result == null) return BadRequest(new { Message = "Exercise not found" });
            return Ok(result);
        }
        [HttpGet("GetExerciseByID")]
        public async Task<IActionResult> GetExerciseByID([FromQuery] Guid id)
        {
            var result = await _exerciseService.GetExerciseByID(id);
            if (result == null) return BadRequest(new { Message = "Exercise not found" });
            return Ok(result);
        }
        [HttpGet("GetExerciseMuscles")]
        public async Task<IActionResult> GetExerciseMuscles([FromQuery] Guid exerciseID)
        {
            var result = await _exerciseService.GetExerciseMuscles(exerciseID);
            if (result == null) return BadRequest(new { Message = "Exercise not found or no muscles found for this exercise" });
            return Ok(result);
        }
        [HttpPost("GetExerciseByMuscle")]
        public async Task<IActionResult> GetWorkoutsByMuscle([FromBody] ExerciseListDTO muscles)
        {
            var result = await _exerciseService.GetExercisesByMuscle(muscles);
            if(result == null) return BadRequest(new { Message = "Either you entered the muscles wrong or no Exercise was found." });
            return Ok(result);
        }
        [HttpGet("GetExercisesByFilter")]
        public async Task<IActionResult> GetAllExercisesByFilter([FromQuery] string sortColumn, string OrderBy, string SearchTerm, int page, int pageSize)
        {
            var result = await _exerciseService.GetExercisesByFilter(page, sortColumn, OrderBy, SearchTerm, pageSize);
            return Ok(result);
        }
        [HttpGet("GetAllExercises")]
        public async Task<IActionResult> GetAllExercises([FromQuery] int page, int pageSize)
        {
            var result = await _exerciseService.GetAllExercises(page,pageSize);
            return Ok(result);
        }
    }
}
