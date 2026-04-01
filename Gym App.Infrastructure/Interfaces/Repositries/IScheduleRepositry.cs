using Gym_App.Domain;

namespace Gym_App.Infastructure.Interfaces.Repositries
{
    public interface IScheduleRepositry : IBaseRepositry<Schedule>
    {
        Task<Schedule> GetScheduleById(Guid schedId);
        Task<ICollection<Schedule>> GetUserSchedules(Guid userId);
        Task<int> GetUserSchedulesCount(Guid userId);
         Task<bool> isScheduleExist(Guid schedId);
         Task<bool> isUserHasSchedules(Guid userId);
         IQueryable<Schedule> GetUserSchedulesQueryable(Guid userId);
         Task<bool> DeleteUserSchedules(Guid userId);
    }
}
