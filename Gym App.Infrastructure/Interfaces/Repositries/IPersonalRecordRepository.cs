using Gym_App.Domain;
using Gym_App.Infastructure.Repositries;

namespace Gym_App.Infastructure.Interfaces.Repositries
{
    public interface IPersonalRecordRepository : IBaseRepositry<PersonalRecord>
    {
        Task<IEnumerable<PersonalRecord>> GetUserPersonalRecordsAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<PersonalRecord?> GetUserExercisePRAsync(Guid userId, Guid exerciseId, CancellationToken cancellationToken = default);

        Task<IEnumerable<PersonalRecord>> GetUnsentNotificationsAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<PersonalRecord>> GetPRsByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    }
}
