using Gym_App.Domain.DTOs;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WorkoutController : Controller
    {
        private readonly IWorkoutService _workoutService;
        public WorkoutController(IWorkoutService workoutService)
        {
            _workoutService = workoutService;
        }
        [HttpPost("Create Workout")]//That is one way of adding it. I like Route but I may diverge into this
        public async Task<IActionResult> CreateWorkout([FromBody] WorkoutDTO workout)
        {
            var result = await _workoutService.CreateWorkout(workout);
            if (result == 0) return BadRequest(new { message = "Workout creation failed." });
            return Ok(new { message = "Workout created successfully." });
        }
        [HttpPut("Modify Workout")]
        public async Task<IActionResult> UpdateWorkout([FromBody] WorkoutDTO workout)
        {
            var result = await _workoutService.UpdateWorkout(workout);
            if(result==0) return BadRequest(new { message = "Workout update failed." });
            return Ok(new { message = "Workout updated successfully." });
        }
        [HttpDelete("Delete Workout")]
        public async Task<IActionResult> DeleteWorkout([FromBody] WorkoutDTO workout)
        {
            var result = await _workoutService.DeleteWorkout(workout);
            if(result==0) return BadRequest(new { message = "Workout deletion failed." });
            return Ok(new { message = "Workout deleted successfully." });
        }
        [HttpPost("Add Exercises to Workout")]
        public async Task<IActionResult> AddExercisesToWorkout([FromBody] WorkoutExerciseDTO workoutExercise)
        {
            var result = await _workoutService.AddExercisesToWorkout(workoutExercise);
            if(result==0) return BadRequest(new { message = "Adding exercises to workout failed." });
            return Ok(new { message = "Exercises added to workout successfully." });
        }
        [HttpPost("Set Exercises of Workout")]
        public async Task<IActionResult> SetExercisesOfWorkout([FromBody] WorkoutExerciseDTO workoutExercise)
        {
            var result = await _workoutService.SetExercisesOfWorkout(workoutExercise);
            if(result==0) return BadRequest(new { message = "Setting exercises of workout failed." });
            return Ok(new { message = "Exercises set to workout successfully." });
        }
        [HttpDelete("Delete Exercises from Workout")]
        public async Task<IActionResult> DeleteExercisesFromWorkout([FromBody] WorkoutExerciseDTO workoutExercise)
        {
            var result = await _workoutService.DeleteExercisesFromWorkout(workoutExercise);
            if(result==0) return BadRequest(new { message = "Deleting exercises from workout failed." });
            return Ok(new { message = "Exercises deleted from workout successfully." });
        }
        [HttpPost("Get Workout by Name")]
        public async Task<IActionResult> GetWorkoutByName([FromBody] string name)
        {
            var result = await _workoutService.GetWorkoutByName(name);
            if(result==null) return BadRequest(new { message = "Workout not found." });
            return Ok(result);
        }
        [HttpGet("Get All Workouts")]
        public async Task<IActionResult> GetAllWorkouts()
        {
            var result = await _workoutService.GetAllWorkouts();
            return Ok(result);
        }
    }
}
