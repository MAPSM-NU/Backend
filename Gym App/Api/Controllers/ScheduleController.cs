using Gym_App.Application.Interfaces;
using Gym_App.Infastructure.DTOs.Schedule;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Api.Controllers
{
    [Authorize(Policy ="NormalUsage")]
    [Route("api/v1/schedule")]
    public class ScheduleController : Controller
    {
        private readonly IScheduleService _scheduleService;
        public ScheduleController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpPost("create/{userID}")]
        public async Task<IActionResult> AddSchedule([FromRoute]Guid userID,[FromBody] ScheduleCreationAndEditDTO schedule)
        {
            var result = await _scheduleService.AddSchedule(User, userID, schedule);
            
            if(result.status == 0)
                return BadRequest(new { message = result.msg });
            else if(result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpPut("update/{scheduleID}")]
        public async Task<IActionResult> UpdateSchedule([FromRoute] Guid scheduleID,[FromBody] ScheduleCreationAndEditDTO schedule)
        {
            var result = await _scheduleService.UpdateSchedule(User, scheduleID, schedule);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpDelete("delete/{scheduleID}")]
        public async Task<IActionResult> DeleteSchedule([FromRoute] Guid scheduleID)
        {
            var result = await _scheduleService.DeleteSchedule(User, scheduleID);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpPost("add-workouts/{scheduleID}")]
        public async Task<IActionResult> AddWorkoutsToSchedule([FromRoute] Guid scheduleID, [FromBody] ScheduleWorkoutDTO scheduleWorkout)
        {
            var result = await _scheduleService.AddWorkoutsToSchedule(User, scheduleID, scheduleWorkout);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpPost("set-workouts/{scheduleID}")]
        public async Task<IActionResult> SetWorkoutsOfSchedule([FromRoute] Guid scheduleID, [FromBody] ScheduleWorkoutDTO scheduleWorkout)
        {
            var result = await _scheduleService.SetWorkoutsOfSchedule(User, scheduleID, scheduleWorkout);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpDelete("delete-workouts/{scheduleID}")]
        public async Task<IActionResult> DeleteWorkoutsFromSchedule([FromRoute] Guid scheduleID, [FromBody] ScheduleWorkoutDTO scheduleWorkout)
        {
            var result = await _scheduleService.DeleteWorkoutsFromSchedule(User, scheduleID, scheduleWorkout);

            if (result.status == 0)
                return BadRequest(new { message = result.msg });
            else if (result.status == 1)
                return Forbid();
            else
                return Ok(new { message = result.msg });
        }
        [HttpGet("get/{scheduleID}")]
        public async Task<IActionResult> GetScheduleById([FromRoute] Guid scheduleID)
        {
            var schedule = await _scheduleService.GetScheduleById(scheduleID);
            if (schedule == null)
                return BadRequest(new { message = "Schedule not found" });
            else 
                return Ok(schedule);
        }
        [HttpGet("get-workouts/{scheduleID}")]
        public async Task<IActionResult> GetWorkoutsOfSchedule([FromRoute] Guid scheduleID)
        {
            var workouts = await _scheduleService.GetScheduleWorkouts(scheduleID);
            if (workouts == null)
                return BadRequest(new { message = "Schedule not found" });
            else 
                return Ok(workouts);
        }

        [HttpGet("get-user-schedule/{userID}")]
        public async Task<IActionResult> GetSchedulesByOfUser([FromRoute] Guid userID,[FromQuery] string startDate, string endDate, string sortColumn, string OrderBy, string searchTerm, int page, int pageSize)
        {
            var schedules = await _scheduleService.GetSchedulesByOfUser(userID,startDate,endDate,page,sortColumn, OrderBy,searchTerm,pageSize);
            return Ok(schedules);
        }
        [HttpGet("get")]
        public async Task<IActionResult> GetAllSchedules([FromQuery] int page,int pageSize)
        {
            var schedules = await _scheduleService.GetAllSchedules(page,pageSize);
            return Ok(schedules);
        }

    }
}
