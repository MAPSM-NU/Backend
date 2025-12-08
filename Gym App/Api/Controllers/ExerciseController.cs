using Gym_App.Application.Interfaces;
using Gym_App.Infastructure.DTOs.Exercise;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Api.Controllers
{
    [Authorize(Policy ="NormalUsage")]
    [Route("[controller]")]
    public class ExerciseController : Controller
    {
        private readonly IExerciseService _exerciseService;
        public ExerciseController(IExerciseService exerciseService)
        {
            _exerciseService = exerciseService;
        }
        [Authorize(Policy = "ElevatedPower")]
        [HttpPost("AddExercise")]
        public async Task<IActionResult> AddExercise([FromBody] ExerciseCreationDTO exercise)
        {
            var result = await _exerciseService.CreateExercise(exercise);
            if (result == 2)
                return Ok(new { Message = "Exercise Added Succesfully" });
            else if (result == 1) 
                return BadRequest(new { Message = "The Exercise already exists" });
            else
                return BadRequest(new { Message = "Exercise or Exercise Name cannot be null" });
        }
        [Authorize(Policy = "ElevatedPower")]
        [HttpPut("UpdateExercise")]
        public async Task<IActionResult> UpdateExercise([FromQuery]Guid exerciseID,[FromBody] ExerciseCreationDTO exercise)
        {
            var result = await _exerciseService.UpdateExercise(exerciseID, exercise);
            if (result == 3)
                return Ok(new { message = "Exercise Updated Successfully" });
            else if (result == 2)
                return BadRequest(new { message = "The Exercise with the given name already exists" });
            else if (result == 1)
                return BadRequest(new { message = "Exercise not found" });
            else
                return BadRequest(new { message = "Faulty creditentials" });
        }
        [Authorize(Policy = "ElevatedPower")]
        [HttpDelete("DeleteExercise")]
        public async Task<IActionResult> DeleteExercise([FromQuery] Guid exerciseId)
        {
            var result = await _exerciseService.DeleteExercise(exerciseId);
            if (result > 0)
                return Ok(new { Message = "Exercise deleted successfully"});
            else
                return BadRequest(new { Message = "Exercise does not exist" });
        }

        [HttpPost("AddMusclesToExercise")]
        public async Task<IActionResult> AddMusclesToExercise([FromQuery]Guid exerciseID,[FromBody] ExerciseMusclesDTO Muscles)
        {
            var result = await _exerciseService.AddMusclesToExercise(exerciseID, Muscles);
            if(result == 4)
                return Ok(new { Message = "Muscles added to exercise successfully" });
            else if(result == 3)
                return BadRequest(new { Message = "Wrong IDs given for the Muscles" });
            else if(result == 2)
                return BadRequest(new { Message = "Given muscles are already in the Exercise" });
            else if(result == 1)
                return BadRequest(new { Message = "Given exercise does not exist" });
            else
                return BadRequest(new { Message = "Faulty DTO" });
        }

        [HttpDelete("RemoveMusclesFromExercise")]
        public async Task<IActionResult> RemoveMusclesFromExercise([FromQuery] Guid exerciseID, [FromBody] ExerciseMusclesDTO Muscles)//fe 8alta hena
        {
            var result = await _exerciseService.RemoveMusclesFromExercise(exerciseID,Muscles);
            if(result == 4)
                return Ok(new { Message = "Muscles removed from exercise successfully" });
            else if(result == 3) 
                return BadRequest(new { Message = "Wrong IDs given for the Muscles" });
            else if(result == 2) 
                return BadRequest(new { Message = "Given muscles are not in the Exercise" });
            else if(result == 1)
                return BadRequest(new { Message = "Given exercise does not exist" });
            else 
                return BadRequest(new { Message = "Faulty DTO" });
        }
        [HttpGet("GetExerciseByName")]
        public async Task<IActionResult> GetExerciseByName([FromQuery] string name)
        {
            var result = await _exerciseService.GetExerciseByName(name);
            if (result == null) 
                return BadRequest(new { Message = "Exercise not found" });
            else
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
            if (result == null) 
                return BadRequest(new { Message = "Exercise not found or no muscles found for this exercise" });
            else
                return Ok(result);
        }
        [HttpPost("GetExerciseByMuscle")]
        public async Task<IActionResult> GetWorkoutsByMuscle([FromQuery] int page, int pageSize,[FromBody] ExerciseListDTO muscles)
        {
            var result = await _exerciseService.GetExercisesByMuscle(muscles,page,pageSize);
            if(result == null) 
                return BadRequest(new { Message = "Either you entered the muscles wrong or no Exercise was found." });
            else
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
