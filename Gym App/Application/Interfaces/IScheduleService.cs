using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.DTOs.Schedule;
using System.Security.Claims;

namespace Gym_App.Application.Interfaces
{
    public interface IScheduleService
    {
        public Task<int> AddSchedule(ClaimsPrincipal User,Guid userID, ScheduleCreationAndEditDTO schedule);
        public Task<int> UpdateSchedule(ClaimsPrincipal User,Guid scheduleID, ScheduleCreationAndEditDTO schedule);
        public Task<int> DeleteSchedule(ClaimsPrincipal User, Guid scheduleID);
        public Task<int> AddWorkoutsToSchedule(ClaimsPrincipal User, Guid scheduleID, ScheduleWorkoutDTO scheduleWorkout);
        public Task<int> SetWorkoutsOfSchedule(ClaimsPrincipal User, Guid scheduleID, ScheduleWorkoutDTO scheduleWorkout);
        public Task<int> DeleteWorkoutsFromSchedule(ClaimsPrincipal User, Guid scheduleID, ScheduleWorkoutDTO scheduleWorkout);
        public Task<ScheduleViewDTO?> GetScheduleById(Guid scheduleID);
        public Task<ScheduleWorkoutDTO?> GetScheduleWorkouts(Guid scheduleID);
        public Task<PagedList<ScheduleViewDTO>?> GetSchedulesByOfUser(Guid UserID,string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm,int pageSize);
        public Task<PagedList<ScheduleViewDTO>?> GetAllSchedules(int page,int pageSize);
        public Task<Guid> GetScheduleUserID(Guid scheduleID);
    }
}
