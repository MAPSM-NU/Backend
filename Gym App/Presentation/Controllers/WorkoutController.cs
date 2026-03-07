using Gym_App.Infastructure.DTOs.WorkoutDTOs;
using Gym_App.Infastructure.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Api.Controllers
{
    [Authorize(Policy = "NormalUsage")]
    [Route("api/v1/workout")]
    public class WorkoutController : Controller
    {
        private readonly IWorkoutService _workoutService;
        public WorkoutController(IWorkoutService workoutService)
        {
            _workoutService = workoutService;
        }
        [HttpPost("create")]//That is one way of adding it. I like Route but I may diverge into this
        public async Task<IActionResult> CreateWorkout([FromBody] WorkoutCreationDTO workout)
        {
            var result = await _workoutService.CreateWorkout(User,workout);

            if(result.status == 0)
                return BadRequest(new { message = result.msg });
            else if(result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpPut("update/{workoutID}")]
        public async Task<IActionResult> UpdateWorkout([FromRoute] Guid workoutID, [FromBody] WorkoutUpdateDTO workout)
        {
            var result = await _workoutService.UpdateWorkout(User, workoutID, workout);
            
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpDelete("delete/{workoutID}")]
        public async Task<IActionResult> DeleteWorkout([FromRoute] Guid workout)
        {
            var result = await _workoutService.DeleteWorkout(User,workout);
            
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpPost("add-exercises/{workoutID}")]
        public async Task<IActionResult> AddExercisesToWorkout([FromRoute] Guid workoutID,[FromBody] WorkoutExerciseDTO workoutExercise)
        {
            var result = await _workoutService.AddExercisesToWorkout(User, workoutID, workoutExercise);
            
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpPost("set-exercises/{workoutID}")]
        public async Task<IActionResult> SetExercisesOfWorkout([FromRoute] Guid workoutID,[FromBody] WorkoutExerciseDTO workoutExercises)
        {
            var result = await _workoutService.SetExercisesOfWorkout(User,workoutID,workoutExercises);
            
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpDelete("delete-exercises/{workoutID}")]
        public async Task<IActionResult> DeleteExercisesFromWorkout([FromRoute] Guid workoutID, [FromBody] WorkoutExerciseDTO workoutExercises)
        {
            var result = await _workoutService.DeleteExercisesFromWorkout(User,workoutID,workoutExercises);
            
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpGet("get/{workoutID}")]
        public async Task<IActionResult> GetWorkoutByID([FromRoute] Guid workoutID)
        {
            var result = await _workoutService.GetWorkoutByID(workoutID);
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Value);
        }
        [HttpGet("get-exercises/{workoutID}")]
        public async Task<IActionResult> GetExercisesOfWorkout([FromRoute] Guid workoutID,[FromQuery] string sortColumn, string OrderBy, string searchTerm, int page = 1, int pageSize = 10)
        {

            var result = await _workoutService.GetExercisesOfWorkout(workoutID,page,sortColumn,OrderBy,searchTerm,pageSize);
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Data);
        }
        //[Authorize(Policy = "ElevatedPower")]
        [HttpGet("get")]
        public async Task<IActionResult> GetAllWorkouts(int page=1,int pageSize = 10)
        {
            var result = await _workoutService.GetAllWorkouts(page,pageSize);
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Data);
        }
    }
}
