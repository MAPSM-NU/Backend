using Gym_App.Application.Interfaces;
using Gym_App.Domain.Entities;
using Gym_App.Infastructure.DTOs.Schedule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Api.Controllers
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
        public async Task<IActionResult> AddSchedule([FromQuery]Guid userID,[FromBody] ScheduleCreationAndEditDTO schedule)
        {
            var result = await _scheduleService.AddSchedule(User, userID, schedule);
            
            if(result.status == 0)
                return BadRequest(new { message = result.msg });
            else if(result.status == 1)
                return Forbid(result.msg);
            else
                return Ok(new { message = result.msg });
        }
        [HttpPut("UpdateSchedule")]
        public async Task<IActionResult> UpdateSchedule([FromQuery] Guid scheduleID,[FromBody] ScheduleCreationAndEditDTO schedule)
        {
            var result = await _scheduleService.UpdateSchedule(User, scheduleID, schedule);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid(result.msg);
            else
                return Ok(new { message = result.msg });
        }
        [HttpDelete("DeleteSchedule")]
        public async Task<IActionResult> DeleteSchedule([FromQuery] Guid scheduleID)
        {
            var result = await _scheduleService.DeleteSchedule(User, scheduleID);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid(result.msg);
            else
                return Ok(new { message = result.msg });
        }
        [HttpPost("AddWorkoutsToSchedule")]
        public async Task<IActionResult> AddWorkoutsToSchedule([FromQuery] Guid scheduleID, [FromBody] ScheduleWorkoutDTO scheduleWorkout)
        {
            var result = await _scheduleService.AddWorkoutsToSchedule(User, scheduleID, scheduleWorkout);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid(result.msg);
            else
                return Ok(new { message = result.msg });
        }
        [HttpPost("SetWorkoutsOfSchedule")]
        public async Task<IActionResult> SetWorkoutsOfSchedule([FromQuery] Guid scheduleID, [FromBody] ScheduleWorkoutDTO scheduleWorkout)
        {
            var result = await _scheduleService.SetWorkoutsOfSchedule(User, scheduleID, scheduleWorkout);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid(result.msg);
            else
                return Ok(new { message = result.msg });
        }
        [HttpDelete("DeleteWorkoutsFromSchedule")]
        public async Task<IActionResult> DeleteWorkoutsFromSchedule([FromQuery] Guid scheduleID, [FromBody] ScheduleWorkoutDTO scheduleWorkout)
        {
            var result = await _scheduleService.DeleteWorkoutsFromSchedule(User, scheduleID, scheduleWorkout);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid(result.msg);
            else
                return Ok(new { message = result.msg });
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
