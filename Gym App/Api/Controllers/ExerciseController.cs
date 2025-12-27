using Gym_App.Application.Interfaces;
using Gym_App.Infastructure.DTOs.Exercise;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Api.Controllers
{
    [Authorize(Policy ="NormalUsage")]
    [Route("api/v1/exercise")]
    public class ExerciseController : Controller
    {
        private readonly IExerciseService _exerciseService;
        public ExerciseController(IExerciseService exerciseService)
        {
            _exerciseService = exerciseService;
        }
        [Authorize(Policy = "ElevatedPower")]
        [HttpPost("create")]
        public async Task<IActionResult> AddExercise([FromBody] ExerciseCreationDTO exercise)
        {
            var result = await _exerciseService.CreateExercise(exercise);
            
            if(result.status == 0)
                return BadRequest(new { message = result.msg });
            else
                return Ok(new { message = result.msg });
        }
        [Authorize(Policy = "ElevatedPower")]
        [HttpPut("update/{exerciseID}")]
        public async Task<IActionResult> UpdateExercise([FromRoute]Guid exerciseID,[FromBody] ExerciseCreationDTO exercise)
        {
            var result = await _exerciseService.UpdateExercise(exerciseID, exercise);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else
                return Ok(new { message = result.msg });
        }
        [Authorize(Policy = "ElevatedPower")]
        [HttpDelete("delete/{exerciseID}")]
        public async Task<IActionResult> DeleteExercise([FromRoute] Guid exerciseID)
        {
            var result = await _exerciseService.DeleteExercise(exerciseID);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else
                return Ok(new { message = result.msg });
        }
        [HttpPost("add-muscles/{exerciseID}")]
        public async Task<IActionResult> AddMusclesToExercise([FromRoute]Guid exerciseID,[FromBody] ExerciseMusclesDTO Muscles)
        {
            var result = await _exerciseService.AddMusclesToExercise(exerciseID, Muscles);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else
                return Ok(new { message = result.msg });
        }
        [HttpDelete("remove-muscles/{exerciseID}")]
        public async Task<IActionResult> RemoveMusclesFromExercise([FromRoute] Guid exerciseID, [FromBody] ExerciseMusclesDTO Muscles)
        {
            var result = await _exerciseService.RemoveMusclesFromExercise(exerciseID,Muscles);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else
                return Ok(new { message = result.msg });
        }
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetExerciseByID([FromRoute] Guid id)
        {
            var result = await _exerciseService.GetExerciseByID(id);
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Value);
        }
        [HttpGet("get-muscles/{exerciseID}")]
        public async Task<IActionResult> GetExerciseMuscles([FromRoute] Guid exerciseID)
        {
            var result = await _exerciseService.GetExerciseMuscles(exerciseID);
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Value);
        }
        [HttpPost("get-by-muscle")]
        public async Task<IActionResult> GetWorkoutsByMuscle([FromBody] ExerciseListDTO muscles,[FromQuery] int page = 1, int pageSize = 10)
        {
            var result = await _exerciseService.GetExercisesByMuscle(muscles,page,pageSize);
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Data);
        }
        [HttpGet("get-by-filter")]
        public async Task<IActionResult> GetAllExercisesByFilter([FromQuery] string sortColumn, string OrderBy, string SearchTerm, int page = 1, int pageSize = 10)
        {
            var result = await _exerciseService.GetExercisesByFilter(page, sortColumn, OrderBy, SearchTerm, pageSize);
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Data);
        }
        [HttpGet("get")]
        public async Task<IActionResult> GetAllExercises([FromQuery] int page = 1, int pageSize = 10)
        {
            var result = await _exerciseService.GetAllExercises(page,pageSize);
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Data);
        }
    }
}
