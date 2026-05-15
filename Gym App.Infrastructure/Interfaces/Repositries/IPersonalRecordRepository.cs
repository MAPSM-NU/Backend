using Gym_App.Domain;
using Gym_App.Infastructure.Repositries;

namespace Gym_App.Infastructure.Interfaces.Repositries
{
    public interface IPersonalRecordRepository : IBaseRepositry<PersonalRecord>
    {
        Task<IEnumerable<PersonalRecord>> GetUserPersonalRecordsAsync(Guid userId);

        Task<PersonalRecord?> GetUserExercisePRAsync(Guid userId, Guid exerciseId);

        Task<IEnumerable<PersonalRecord>> GetUnsentNotificationsAsync();

        Task<IEnumerable<PersonalRecord>> GetPRsByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate);
    }
}
