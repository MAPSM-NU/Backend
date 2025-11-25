using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Service.Controllers
{
    [Authorize(Policy ="NormalUsage")]
    [Route("[controller]")]
    public class ScheduleController : Controller
    {
        private readonly IScheduleService _scheduleService;
        public ScheduleController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpPost("AddSchedule")]
        public async Task<IActionResult> AddSchedule([FromBody] ScheduleDTO schedule)
        {
            var result = await _scheduleService.AddSchedule(User, schedule);
            if (result == 3)
                return Ok(new { message = "Schedule added successfully" });
            else if (result == 2)
                return Forbid();
            else if (result == 1)
                return BadRequest(new { message = "Given user does not exist" });
            else
                return BadRequest(new { message = "Faulty DTO given" });
        }
        [HttpPut("UpdateSchedule")]
        public async Task<IActionResult> UpdateSchedule([FromBody] ScheduleDTO schedule)
        {
            var result = await _scheduleService.UpdateSchedule(User, schedule);
            if (result == 3)
                return Ok(new { message = "Schedule updated successfully" });
            else if (result == 2)
                return Forbid();
            else if (result == 1)
                return BadRequest(new { message = "Given schedule does not exist" });
            else 
                return BadRequest(new { message = "Faulty DTO given" });
        }
        [HttpDelete("DeleteSchedule")]
        public async Task<IActionResult> DeleteSchedule([FromQuery] Guid scheduleID)
        {
            var result = await _scheduleService.DeleteSchedule(User, scheduleID);
            if(result == 3)
                return Ok(new { message = "Schedule deleted successfully" });
            else if (result == 2)
                return Forbid();
            else if (result == 1)
                return BadRequest(new { message = "Given schedule does not exist" });
            else 
                return BadRequest(new { message = "Faulty DTO given" });
        }
        [HttpPost("AddWorkoutsToSchedule")]
        public async Task<IActionResult> AddWorkoutsToSchedule([FromBody] ScheduleWorkoutDTO scheduleWorkout)
        {
            var result = await _scheduleService.AddWorkoutsToSchedule(User, scheduleWorkout);
            if (result == 5)
                return Ok(new { Message = "Workouts added to Schedule successfully" });
            else if (result == 4)
                return BadRequest(new { Message = "Wrong IDs given for the Exercises" });
            else if (result == 3)
                return BadRequest(new { message = "Workouts already in schedule" });
            else if (result == 2)
                return Forbid();
            else if (result == 1)
                return BadRequest(new { message = "Given Schedule does not exist" });
            else
                return BadRequest(new { message = "Faulty DTO given" });
        }
        [HttpPost("SetWorkoutsOfSchedule")]
        public async Task<IActionResult> SetWorkoutsOfSchedule([FromBody] ScheduleWorkoutDTO scheduleWorkout)
        {
            var result = await _scheduleService.SetWorkoutsOfSchedule(User, scheduleWorkout);
            if(result == 4)
                return Ok(new {Message = "Workouts set to Schedule successfully"}) ;
            else if(result == 3)
                return BadRequest(new { message = "Wrong IDs given for the Exercises" });
            else if(result == 2)
                return Forbid();
            else if (result == 1)
                return BadRequest(new { message = "Given Schedule does not exist" });
            else
                return BadRequest(new { message = "Faulty DTO given" });
        }
        [HttpDelete("DeleteWorkoutsFromSchedule")]
        public async Task<IActionResult> DeleteWorkoutsFromSchedule([FromBody] ScheduleWorkoutDTO scheduleWorkout)
        {
            var result = await _scheduleService.DeleteWorkoutsFromSchedule(User, scheduleWorkout);
            if (result == 5)
                return Ok(new { Message = "Workouts removed from Schedule successfully" });
            else if (result == 4)
                return BadRequest(new { Message = "Wrong IDs given for the Exercises" });
            else if (result == 3)
                return BadRequest(new { message = "Workouts are not in schedule" });
            else if (result == 2)
                return Forbid();
            else if (result == 1)
                return BadRequest(new { message = "Given Schedule does not exist" });
            else
                return BadRequest(new { message = "Faulty DTO given" });
        }
        [HttpGet("GetScheduleByID")]
        public async Task<IActionResult> GetScheduleById([FromQuery] Guid scheduleID)
        {
            var schedule = await _scheduleService.GetScheduleById(scheduleID);
            if (schedule == null)
                return BadRequest(new { message = "Schedule not found" });
            else 
                return Ok(schedule);
        }
        [HttpGet("GetWorkoutsOfSchedule")]
        public async Task<IActionResult> GetWorkoutsOfSchedule([FromQuery] Guid scheduleID)
        {
            var workouts = await _scheduleService.GetScheduleWorkouts(scheduleID);
            if (workouts == null)
                return BadRequest(new { message = "Schedule not found" });
            else 
                return Ok(workouts);
        }

        [HttpGet("GetSchedulesOfUser")]
        public async Task<IActionResult> GetSchedulesByOfUser([FromQuery] Guid userID, string startDate, string endDate, string sortColumn, string OrderBy, string searchTerm, int page, int pageSize)
        {
            var schedules = await _scheduleService.GetSchedulesByOfUser(userID,startDate,endDate,page,sortColumn, OrderBy,searchTerm,pageSize);
            return Ok(schedules);
        }
        [HttpGet("GetAllSchedules")]
        public async Task<IActionResult> GetAllSchedules([FromQuery] int page,int pageSize)
        {
            var schedules = await _scheduleService.GetAllSchedules(page,pageSize);
            return Ok(schedules);
        }

    }
}
