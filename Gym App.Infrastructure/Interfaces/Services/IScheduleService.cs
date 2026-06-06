using Gym_App.Infastructure.DTOs.Schedule;
using Gym_App.Infastructure.Transfer_Classes;
using System.Security.Claims;
using System.Threading;

namespace Gym_App.Infastructure.Interfaces.Services
{
    public interface IScheduleService
    {
        public Task<SettersResponse> AddSchedule(Guid userID, ScheduleCreationAndEditDTO schedule, CancellationToken cancellationToken = default);
        public Task<SettersResponse> UpdateSchedule(Guid scheduleID, ScheduleCreationAndEditDTO schedule, CancellationToken cancellationToken = default);
        public Task<SettersResponse> DeleteSchedule(Guid scheduleID, CancellationToken cancellationToken = default);
        public Task<SettersResponse> AddWorkoutsToSchedule(Guid scheduleID, ScheduleWorkoutDTO scheduleWorkout, CancellationToken cancellationToken = default);
        public Task<SettersResponse> SetWorkoutsOfSchedule(Guid scheduleID, ScheduleWorkoutDTO scheduleWorkout, CancellationToken cancellationToken = default);
        public Task<SettersResponse> DeleteWorkoutsFromSchedule(Guid scheduleID, ScheduleWorkoutDTO scheduleWorkout, CancellationToken cancellationToken = default);
        public Task<GettersResponse<ScheduleViewDTO>> GetScheduleById(Guid scheduleID, CancellationToken cancellationToken = default);
        public Task<GettersResponse<ScheduleWorkoutDTO>> GetScheduleWorkouts(Guid scheduleID, CancellationToken cancellationToken = default);
        public Task<GettersResponse<ScheduleViewDTO>> GetSchedulesByOfUser(Guid UserID, string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize, CancellationToken cancellationToken = default);
        public Task<GettersResponse<ScheduleViewDTO>> GetAllSchedules(int page, int pageSize, CancellationToken cancellationToken = default);
        public Task<Guid> GetScheduleUserID(Guid scheduleID, CancellationToken cancellationToken = default);
    }
}
