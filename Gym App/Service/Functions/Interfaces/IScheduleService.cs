using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;

namespace Gym_App.Service.Functions.Interfaces
{
    public interface IScheduleService
    {
        public Task<int> AddSchedule(ScheduleDTO schedule);
        public Task<int> UpdateSchedule(ScheduleDTO schedule);
        public Task<int> DeleteSchedule(Guid scheduleID);
        public Task<int> AddWorkoutsToSchedule(ScheduleWorkoutDTO scheduleWorkout);
        public Task<int> SetWorkoutsOfSchedule(ScheduleWorkoutDTO scheduleWorkout);
        public Task<int> DeleteWorkoutsFromSchedule(ScheduleWorkoutDTO scheduleWorkout);
        public Task<ScheduleDTO?> GetScheduleById(Guid scheduleID);
        public Task<ScheduleWorkoutDTO?> GetScheduleWorkouts(Guid scheduleID);
        public Task<List<ScheduleDTO>?> GetSchedulesByOfUser(Guid UserID);
        public Task<List<ScheduleDTO>?> GetAllSchedules();
    }
}
