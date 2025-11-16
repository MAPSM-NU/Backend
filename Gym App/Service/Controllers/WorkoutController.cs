using Gym_App.Domain.DTOs;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Service.Controllers
{
    [Authorize(Policy = "NormalUsage")]
    [Route("[controller]")]
    public class WorkoutController : Controller
    {
        private readonly IWorkoutService _workoutService;
        private readonly IAuthorizationService _authorizationService;
        public WorkoutController(IWorkoutService workoutService,IAuthorizationService authorizationService)
        {
            _workoutService = workoutService;
            _authorizationService = authorizationService;
            
        }
        [HttpPost("CreateWorkout")]//That is one way of adding it. I like Route but I may diverge into this
        public async Task<IActionResult> CreateWorkout([FromBody] WorkoutDTO workout)
        {
            var result = await _workoutService.CreateWorkout(workout);

            if(result == 2) 
                return Ok(new { message = "Workout created successfully." });
            else if(result == 1) 
                return BadRequest(new { message = "Given user does not exist." });
            else 
                return BadRequest(new { message = "Faulty DTO given." });
        }
        [HttpPut("ModifyWorkout")]
        public async Task<IActionResult> UpdateWorkout([FromBody] WorkoutDTO workout)
        {
            //Authorization

            if(workout.WorkoutID == Guid.Empty)
                return BadRequest(new { message = "Given user does not exist." });
            
            var UserID = await _workoutService.GetWorkoutUserID(workout.WorkoutID);
            if(UserID == Guid.Empty)
                return BadRequest(new { message = "Given workout does not exist." });
            
            var authResult = await _authorizationService.AuthorizeAsync(User,UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return Forbid();

            //Talking to Database

            var result = await _workoutService.UpdateWorkout(workout);
            if (result == 2)
                return Ok(new { message = "Workout updated successfully." });
            else if(result == 1)
                return BadRequest(new { message = "Given workout does not exist." });
            else 
                return BadRequest(new { message = "Faulty DTO given." });
        }
        [HttpDelete("DeleteWorkout")]
        public async Task<IActionResult> DeleteWorkout([FromBody] WorkoutDTO workout)
        {
            //Authorization

            if (workout.WorkoutID == Guid.Empty)
                return BadRequest(new { message = "Given user does not exist." });

            var UserID = await _workoutService.GetWorkoutUserID(workout.WorkoutID);
            if(UserID == Guid.Empty)
                return BadRequest(new { message = "Given workout does not exist." });
            
            var authResult = await _authorizationService.AuthorizeAsync(User, UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return Forbid();

            //Talking to Database

            var result = await _workoutService.DeleteWorkout(workout);
            if (result == 2)
                return Ok(new { message = "Workout deleted successfully." });
            else if(result == 1)
                return BadRequest(new { message = "Given workout does not exist." });
            else 
                return BadRequest(new { message = "Faulty DTO given." });
        }
        [HttpPost("AddExercisestoWorkout")]
        public async Task<IActionResult> AddExercisesToWorkout([FromBody] WorkoutExerciseDTO workoutExercise)
        {
            //Authorization
            
            var UserID = await _workoutService.GetWorkoutUserID(workoutExercise.WorkoutID);
            if(UserID == Guid.Empty) return BadRequest(new { message = "Given workout does not exist." });
            
            var authResult = await _authorizationService.AuthorizeAsync(User, UserID, "SameUserPolicy");
            if (!authResult.Succeeded) return Forbid();
            
            //Talking to Database
            
            var result = await _workoutService.AddExercisesToWorkout(workoutExercise);
            if(result == 3)
                return Ok(new { message = "Exercises added to workout successfully." });
            else if(result == 2)
                return BadRequest(new { message = "Either given exercises don't exist or no new exercises to add." });
            else if(result == 1)
                return BadRequest(new { message = "Given workout does not exist." });
            else 
                return BadRequest(new { message = "Faulty DTO given." });
        }
        [HttpPost("SetExercisesofWorkout")]
        public async Task<IActionResult> SetExercisesOfWorkout([FromBody] WorkoutExerciseDTO workoutExercise)
        {
            //Authorization
            
            var UserID = await _workoutService.GetWorkoutUserID(workoutExercise.WorkoutID);
            if(UserID == Guid.Empty)
                return BadRequest(new { message = "Given workout does not exist." });
            
            var authResult = await _authorizationService.AuthorizeAsync(User, UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return Forbid();
            
            //Talking to Database
            
            var result = await _workoutService.SetExercisesOfWorkout(workoutExercise);
            if(result == 3)
                return Ok(new { message = "Exercises set for workout successfully." });
            else if(result == 2)
                return BadRequest(new { message = "Either given exercises don't exist or no new exercises to set." });
            else if(result == 1)
                return BadRequest(new { message = "Given workout does not exist." });
            else
                return BadRequest(new { message = "Faulty DTO given." });
        }
        [HttpDelete("DeleteExercisesfromWorkout")]
        public async Task<IActionResult> DeleteExercisesFromWorkout([FromBody] WorkoutExerciseDTO workoutExercise)
        {
            //Authorization
            
            var UserID = await _workoutService.GetWorkoutUserID(workoutExercise.WorkoutID);
            if(UserID == Guid.Empty) return BadRequest(new { message = "Given workout does not exist." });
            
            var authResult = await _authorizationService.AuthorizeAsync(User, UserID, "SameUserPolicy");
            if (!authResult.Succeeded) return Forbid();
            //Talking to Database

            var result = await _workoutService.DeleteExercisesFromWorkout(workoutExercise);
            if(result == 3) 
                return Ok(new { message = "Exercises removed from workout successfully." });
            else if(result == 2) 
                return BadRequest(new { message = "Either given exercises don't exist or no exercises to remove." });
            else if(result == 1) 
                return BadRequest(new { message = "Given workout does not exist." });
            else 
                return BadRequest(new { message = "Faulty DTO given." });
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
        [HttpGet("GetAllWorkouts")]
        public async Task<IActionResult> GetAllWorkouts(int page,int pageSize)
        {
            var result = await _workoutService.GetAllWorkouts(page,pageSize);
            return Ok(result);
        }
    }
}
