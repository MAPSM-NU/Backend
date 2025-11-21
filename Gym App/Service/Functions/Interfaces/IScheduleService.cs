using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using System.Security.Claims;

namespace Gym_App.Service.Functions.Interfaces
{
    public interface IScheduleService
    {
        public Task<int> AddSchedule(ClaimsPrincipal User, ScheduleDTO schedule);
        public Task<int> UpdateSchedule(ClaimsPrincipal User, ScheduleDTO schedule);
        public Task<int> DeleteSchedule(ClaimsPrincipal User, Guid scheduleID);
        public Task<int> AddWorkoutsToSchedule(ClaimsPrincipal User, ScheduleWorkoutDTO scheduleWorkout);
        public Task<int> SetWorkoutsOfSchedule(ClaimsPrincipal User, ScheduleWorkoutDTO scheduleWorkout);
        public Task<int> DeleteWorkoutsFromSchedule(ClaimsPrincipal User, ScheduleWorkoutDTO scheduleWorkout);
        public Task<ScheduleDTO?> GetScheduleById(Guid scheduleID);
        public Task<ScheduleWorkoutDTO?> GetScheduleWorkouts(Guid scheduleID);
        public Task<PagedList<ScheduleDTO>?> GetSchedulesByOfUser(Guid UserID,string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm,int pageSize);
        public Task<PagedList<ScheduleDTO>?> GetAllSchedules(int page,int pageSize);
        public Task<Guid> GetScheduleUserID(Guid scheduleID);
    }
}
