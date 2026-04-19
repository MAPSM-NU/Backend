using Gym_App.Infastructure.DTOs.Schedule;
using Gym_App.Infastructure.Transfer_Classes;
using System.Security.Claims;

namespace Gym_App.Infastructure.Interfaces.Services
{
    public interface IScheduleService
    {
        public Task<SettersResponse> AddSchedule(Guid userID, ScheduleCreationAndEditDTO schedule);
        public Task<SettersResponse> UpdateSchedule(Guid scheduleID, ScheduleCreationAndEditDTO schedule);
        public Task<SettersResponse> DeleteSchedule(Guid scheduleID);
        public Task<SettersResponse> AddWorkoutsToSchedule(Guid scheduleID, ScheduleWorkoutDTO scheduleWorkout);
        public Task<SettersResponse> SetWorkoutsOfSchedule(Guid scheduleID, ScheduleWorkoutDTO scheduleWorkout);
        public Task<SettersResponse> DeleteWorkoutsFromSchedule(Guid scheduleID, ScheduleWorkoutDTO scheduleWorkout);
        public Task<GettersResponse<ScheduleViewDTO>> GetScheduleById(Guid scheduleID);
        public Task<GettersResponse<ScheduleWorkoutDTO>> GetScheduleWorkouts(Guid scheduleID);
        public Task<GettersResponse<ScheduleViewDTO>> GetSchedulesByOfUser(Guid UserID,string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm,int pageSize);
        public Task<GettersResponse<ScheduleViewDTO>> GetAllSchedules(int page,int pageSize);
        public Task<Guid> GetScheduleUserID(Guid scheduleID);
    }
}
