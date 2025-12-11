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
        //[Authorize(Policy = "ElevatedPower")]
        [HttpPost("AddExercise")]
        public async Task<IActionResult> AddExercise([FromBody] ExerciseCreationDTO exercise)
        {
            var result = await _exerciseService.CreateExercise(exercise);
            
            if(result.status == 0)
                return BadRequest(new { message = result.msg });
            else
                return Ok(new { message = result.msg });
        }
        //[Authorize(Policy = "ElevatedPower")]
        [HttpPut("UpdateExercise")]
        public async Task<IActionResult> UpdateExercise([FromQuery]Guid exerciseID,[FromBody] ExerciseCreationDTO exercise)
        {
            var result = await _exerciseService.UpdateExercise(exerciseID, exercise);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else
                return Ok(new { message = result.msg });
        }
        //[Authorize(Policy = "ElevatedPower")]
        [HttpDelete("DeleteExercise")]
        public async Task<IActionResult> DeleteExercise([FromQuery] Guid exerciseId)
        {
            var result = await _exerciseService.DeleteExercise(exerciseId);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else
                return Ok(new { message = result.msg });
        }

        [HttpPost("AddMusclesToExercise")]
        public async Task<IActionResult> AddMusclesToExercise([FromQuery]Guid exerciseID,[FromBody] ExerciseMusclesDTO Muscles)
        {
            var result = await _exerciseService.AddMusclesToExercise(exerciseID, Muscles);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else
                return Ok(new { message = result.msg });
        }

        [HttpDelete("RemoveMusclesFromExercise")]
        public async Task<IActionResult> RemoveMusclesFromExercise([FromQuery] Guid exerciseID, [FromBody] ExerciseMusclesDTO Muscles)//fe 8alta hena
        {
            var result = await _exerciseService.RemoveMusclesFromExercise(exerciseID,Muscles);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else
                return Ok(new { message = result.msg });
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
