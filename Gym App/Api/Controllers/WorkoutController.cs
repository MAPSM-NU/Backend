using Gym_App.Application.Interfaces;
using Gym_App.Infastructure.DTOs.WorkoutDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Api.Controllers
{
    [Authorize(Policy = "NormalUsage")]
    [Route("[controller]")]
    public class WorkoutController : Controller
    {
        private readonly IWorkoutService _workoutService;
        public WorkoutController(IWorkoutService workoutService)
        {
            _workoutService = workoutService;
        }
        [HttpPost("CreateWorkout")]//That is one way of adding it. I like Route but I may diverge into this
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
        [HttpPut("ModifyWorkout")]
        public async Task<IActionResult> UpdateWorkout([FromQuery] Guid workoutID, [FromBody] WorkoutUpdateDTO workout)
        {
            var result = await _workoutService.UpdateWorkout(User, workoutID, workout);
            
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpDelete("DeleteWorkout")]
        public async Task<IActionResult> DeleteWorkout([FromQuery] Guid workout)
        {
            var result = await _workoutService.DeleteWorkout(User,workout);
            
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpPost("AddExercisestoWorkout")]
        public async Task<IActionResult> AddExercisesToWorkout([FromQuery] Guid workoutID,[FromBody] WorkoutExerciseDTO workoutExercise)
        {
            var result = await _workoutService.AddExercisesToWorkout(User, workoutID, workoutExercise);
            
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpPost("SetExercisesofWorkout")]
        public async Task<IActionResult> SetExercisesOfWorkout([FromQuery] Guid workoutID,[FromBody] WorkoutExerciseDTO workoutExercises)
        {
            var result = await _workoutService.SetExercisesOfWorkout(User,workoutID,workoutExercises);
            
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpDelete("DeleteExercisesfromWorkout")]
        public async Task<IActionResult> DeleteExercisesFromWorkout([FromQuery] Guid workoutID, [FromBody] WorkoutExerciseDTO workoutExercises)
        {
            var result = await _workoutService.DeleteExercisesFromWorkout(User,workoutID,workoutExercises);
            
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpGet("GetWorkoutbyName")]
        public async Task<IActionResult> GetWorkoutByName([FromQuery] string name)
        {
            var result = await _workoutService.GetWorkoutByName(name);
            if(result==null) return BadRequest(new { message = "Workout not found." });
            return Ok(result);
        }
        [HttpGet("GetWorkoutbyID")]
        public async Task<IActionResult> GetWorkoutByID([FromQuery] Guid workoutID)
        {
            var result = await _workoutService.GetWorkoutByID(workoutID);
            if(result==null) return BadRequest(new { message = "Workout not found." });
            return Ok(result);
        }
        [HttpGet("GetExercisesofWorkout")]
        public async Task<IActionResult> GetExercisesOfWorkout([FromQuery] Guid workoutID, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize)
        {

            var result = await _workoutService.GetExercisesOfWorkout(workoutID,page,sortColumn,OrderBy,searchTerm,pageSize);
            if(result==null) return BadRequest(new { message = "Workout not found." });
            return Ok(result);
        }
        //[Authorize(Policy = "ElevatedPower")]
        [HttpGet("GetAllWorkouts")]
        public async Task<IActionResult> GetAllWorkouts(int page,int pageSize)
        {
            var result = await _workoutService.GetAllWorkouts(page,pageSize);
            return Ok(result);
        }
    }
}
