using Gym_App.Domain;

namespace Gym_App.Infastructure.Interfaces.Repositries
{
    public interface IWorkoutSetRepository : IBaseRepositry<WorkoutSet>
    {
        Task<IEnumerable<WorkoutSet>> GetSetsByExerciseInstanceAsync(Guid exerciseInstanceId, CancellationToken cancellationToken = default);
        Task<int> GetCompletedSetsCountAsync(Guid exerciseInstanceId, CancellationToken cancellationToken = default);
    }
}
