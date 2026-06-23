using Gym_App.Infastructure.DTOs.WorkoutDTOs;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infrastructure.DTOs.Exercise;
using Gym_App.Infrastructure.DTOs.Workout;
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
        [HttpPost("create/{userId}")]
        public async Task<IActionResult> CreateWorkout([FromRoute]Guid userId, [FromBody] WorkoutCreationDTO workout)
        {
            var result = await _workoutService.CreateWorkoutWithExercisesAsync(userId, workout);

            if(result.status == 0)
                return BadRequest(new { message = result.msg });
            else if(result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpPost("update-progress/{userId}")]
        public async Task<IActionResult> UpdateWorkoutProgress([FromRoute] Guid userId, [FromBody] WorkoutUpdateProgressDTO workoutProgress)
        {
            var result = await _workoutService.UpdateWorkoutProgressAsync(userId, workoutProgress);
            
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpPut("start-workout/{workoutID}/{userId}")]
        public async Task<IActionResult> StartWorkout([FromRoute] Guid workoutID, [FromRoute] Guid userId)
        {
            var result = await _workoutService.StartWorkoutAsync(workoutID, userId);
            
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpPut("complete-workout/{workoutID}/{userId}")]
        public async Task<IActionResult> FinishWorkout([FromRoute] Guid workoutID, [FromRoute] Guid userId)
        {
            var result = await _workoutService.CompleteWorkoutAsync(workoutID, userId);
            
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpPut("manage-workout-exercise/{workoutID}")]
        public async Task<IActionResult> ManageWorkout([FromRoute] Guid workoutID, [FromBody] ExerciseManagementDTO workoutManager)
        {
            var result = await _workoutService.ManageWorkoutExerciseAsync(workoutID, workoutManager);
            
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpGet("get-personal-records/{userId}")]
                public async Task<IActionResult> GetPersonalRecords([FromRoute] Guid userId)
        {
            var result = await _workoutService.GetUserPersonalRecordsAsync(userId);
            
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpPut("update/{workoutID}")]
        public async Task<IActionResult> UpdateWorkout([FromRoute] Guid workoutID, [FromBody] WorkoutUpdateDTO workout)
        {
            var result = await _workoutService.UpdateWorkout(workoutID, workout);
            
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
            var result = await _workoutService.DeleteWorkout(workout);
            
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
            var result = await _workoutService.AddExercisesToWorkout(workoutID, workoutExercise);
            
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
            var result = await _workoutService.SetExercisesOfWorkout(workoutID,workoutExercises);
            
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpDelete("delete-exercises/{workoutID}")]
        public async Task<IActionResult> DeleteExercisesFromWorkout([FromRoute] Guid workoutID, [FromBody] List<Guid> exerciseInstanceIds)
        {
            var result = await _workoutService.DeleteExercisesFromWorkout(workoutID,exerciseInstanceIds);
            
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
        [HttpGet("get-workout-progress/{workoutID}")]
        public async Task<IActionResult> GetWorkoutProgress([FromRoute] Guid workoutID)
        {
            var result = await _workoutService.GetWorkoutCurrentProgress(workoutID);
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Value);
        }
        [HttpGet("get-user-workouts/{userId}")]
        public async Task<IActionResult> GetUsersWorkout([FromRoute] Guid userId, [FromQuery] DateTime startDate, DateTime endDate, string searchTerm, string sortColumn, string OrderBy, int page = 1, int pageSize = 10)
        {
            var result = await _workoutService.GetUsersWorkouts(userId, startDate, endDate, searchTerm, sortColumn, OrderBy, page, pageSize);
            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(result.Data);
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
        [HttpGet("make-scheduleTime-to-date")]
        public async Task<IActionResult> ChangescheduledTimes()
        {
            await _workoutService.MakeAllScheduledDatesEqualData();
            return Ok();
        }
    }
}
