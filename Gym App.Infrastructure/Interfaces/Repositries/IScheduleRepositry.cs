using Gym_App.Domain;

namespace Gym_App.Infastructure.Interfaces.Repositries
{
    public interface IScheduleRepositry : IBaseRepositry<Schedule>
    {
        Task<Schedule> GetScheduleById(Guid schedId, CancellationToken cancellationToken = default);
        Task<ICollection<Schedule>> GetUserSchedules(Guid userId, CancellationToken cancellationToken = default);
        Task<int> GetUserSchedulesCount(Guid userId, CancellationToken cancellationToken = default);
        Task<bool> isScheduleExist(Guid schedId, CancellationToken cancellationToken = default);
        Task<bool> isUserHasSchedules(Guid userId, CancellationToken cancellationToken = default);
        IQueryable<Schedule> GetUserSchedulesQueryable(Guid userId);
        Task<bool> DeleteUserSchedules(Guid userId, CancellationToken cancellationToken = default);
    }
}
