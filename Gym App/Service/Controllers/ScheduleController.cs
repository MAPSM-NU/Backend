using Gym_App.Domain.DTOs;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Service.Controllers
{
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
            return Ok(new { message = "Schedule added successfully" });
        }
        [HttpPut("Update a Schedule")]
        public async Task<IActionResult> UpdateSchedule([FromBody] ScheduleDTO schedule)
        {
            var result = await _scheduleService.UpdateSchedule(schedule);
            if (result == 0) return BadRequest(new { message = "Could not update Schedule" });
            return Ok(new { message = "Schedule updated successfully" });
        }
        [HttpDelete("Delete a Schedule")]
        public async Task<IActionResult> DeleteSchedule([FromQuery] Guid scheduleID)
        {
            var result = await _scheduleService.DeleteSchedule(scheduleID);
            if (result == 0) return BadRequest(new { message = "Could not delete Schedule" });
            return Ok(new { message = "Schedule deleted successfully" });
        }
        [HttpPost("Add Workouts to a Schedule")]
        public async Task<IActionResult> AddWorkoutsToSchedule([FromBody] ScheduleWorkoutDTO scheduleWorkout)
        {
            var result = await _scheduleService.AddWorkoutsToSchedule(scheduleWorkout);
            if (result == 0) return BadRequest(new { message = "Could not add Workouts to Schedule" });
            return Ok(new { message = "Workouts added to Schedule successfully" });
        }
        [HttpPost("Set Workouts of a Schedule")]
        public async Task<IActionResult> SetWorkoutsOfSchedule([FromBody] ScheduleWorkoutDTO scheduleWorkout)
        {
            var result = await _scheduleService.SetWorkoutsOfSchedule(scheduleWorkout);
            if (result == 0) return BadRequest(new { message = "Could not set Workouts of Schedule" });
            return Ok(new { message = "Workouts set of Schedule successfully" });
        }
        [HttpDelete("Delete Workouts from a Schedule")]
        public async Task<IActionResult> DeleteWorkoutsFromSchedule([FromBody] ScheduleWorkoutDTO scheduleWorkout)
        {
            var result = await _scheduleService.DeleteWorkoutsFromSchedule(scheduleWorkout);
            if (result == 0) return BadRequest(new { message = "Could not delete Workouts from Schedule" });
            return Ok(new { message = "Workouts deleted from Schedule successfully" });
        }
        [HttpPost("Get Schedule by ID")]
        public async Task<IActionResult> GetScheduleById([FromQuery] Guid scheduleID)
        {
            var schedule = await _scheduleService.GetScheduleById(scheduleID);
            if (schedule == null) return NotFound(new { message = "Schedule not found" });
            return Ok(schedule);
        }
        [HttpPost("Get Schedules of a User")]
        public async Task<IActionResult> GetSchedulesByOfUser([FromQuery] Guid userID)
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
