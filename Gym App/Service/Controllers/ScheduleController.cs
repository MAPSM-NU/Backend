using Gym_App.Domain.DTOs;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ScheduleController : Controller
    {
        private readonly IScheduleService _scheduleService;
        public ScheduleController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpPost("Add a Schedule")]
        public async Task<IActionResult> AddSchedule([FromBody] ScheduleDTO schedule)
        {
            var result = await _scheduleService.AddSchedule(schedule);
            if (result == 0) return BadRequest(new { message = "Could not add Schedule" });
            else return Ok(new { message = "Schedule added successfully" });
        }
        [HttpPut("Update a Schedule")]
        public async Task<IActionResult> UpdateSchedule([FromBody] ScheduleDTO schedule)
        {
            var result = await _scheduleService.UpdateSchedule(schedule);
            if (result == 0) return BadRequest(new { message = "Given schedule does not exist" });
            else return Ok(new { message = "Schedule updated successfully" });
        }
        [HttpDelete("Delete a Schedule")]
        public async Task<IActionResult> DeleteSchedule([FromBody] Guid scheduleID)
        {
            var result = await _scheduleService.DeleteSchedule(scheduleID);
            if (result == 0) return BadRequest(new { message = "Given schedule does not exist" });
            else return Ok(new { message = "Schedule deleted successfully" });
        }
        [HttpPost("Add Workouts to a Schedule")]
        public async Task<IActionResult> AddWorkoutsToSchedule([FromBody] ScheduleWorkoutDTO scheduleWorkout)
        {
            var result = await _scheduleService.AddWorkoutsToSchedule(scheduleWorkout);
            if(result == 3) return Ok(new {Message = "Workouts added to Schedule successfully"}) ;
            else if(result == 2) return BadRequest(new { message = "No new Workouts were added to Schedule" });
            else if(result == 1) return BadRequest(new { message = "Given Schedule does not exist" });
            else return BadRequest(new { message = "Faulty DTO given" });
        }
        [HttpPost("Set Workouts of a Schedule")]
        public async Task<IActionResult> SetWorkoutsOfSchedule([FromBody] ScheduleWorkoutDTO scheduleWorkout)
        {
            var result = await _scheduleService.SetWorkoutsOfSchedule(scheduleWorkout);
            if(result == 4) return Ok(new {Message = "Workouts set to Schedule successfully"}) ;
            else if(result == 3) return Ok(new { message = "Only removals were made to Schedule" });
            else if(result == 2) return BadRequest(new { message = "No changes were made to Schedule" });
            else if (result == 1) return BadRequest(new { message = "Given Schedule does not exist" });
            else return BadRequest(new { message = "Faulty DTO given" });
        }
        [HttpDelete("Delete Workouts from a Schedule")]
        public async Task<IActionResult> DeleteWorkoutsFromSchedule([FromBody] ScheduleWorkoutDTO scheduleWorkout)
        {
            var result = await _scheduleService.DeleteWorkoutsFromSchedule(scheduleWorkout);
            if (result == 3) return Ok(new { Message = "Workouts removed from Schedule successfully" });
            else if (result == 2) return BadRequest(new { Message = "No Workouts were removed from Schedule" });
            else if (result == 1) return BadRequest(new { message = "Given Schedule does not exist" });
            else return BadRequest(new { message = "Faulty DTO given" });
        }
        [HttpPost("Get Schedule by ID")]
        public async Task<IActionResult> GetScheduleById([FromBody] Guid scheduleID)
        {
            var schedule = await _scheduleService.GetScheduleById(scheduleID);
            if (schedule == null) return BadRequest(new { message = "Schedule not found" });
            else return Ok(schedule);
        }
        [HttpPost("Get Workouts of a Schedule")]
        public async Task<IActionResult> GetWorkoutsOfSchedule([FromBody] Guid scheduleID)
        {
            var workouts = await _scheduleService.GetScheduleWorkouts(scheduleID);
            if (workouts == null) return BadRequest(new { message = "Schedule not found" });
            else return Ok(workouts);
        }

        [HttpPost("Get Schedules of a User")]
        public async Task<IActionResult> GetSchedulesByOfUser([FromBody] Guid userID)
        {
            var schedules = await _scheduleService.GetSchedulesByOfUser(userID);
            return Ok(schedules);
        }
        [HttpGet("Get All Schedules")]
        public async Task<IActionResult> GetAllSchedules()
        {
            var schedules = await _scheduleService.GetAllSchedules();
            return Ok(schedules);
        }

    }
}
