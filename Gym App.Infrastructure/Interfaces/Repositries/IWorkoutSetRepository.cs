using Gym_App.Domain;

namespace Gym_App.Infastructure.Interfaces.Repositries
{
    public interface IWorkoutSetRepository : IBaseRepositry<WorkoutSet>
    {
        Task<IEnumerable<WorkoutSet>> GetSetsByExerciseInstanceAsync(Guid exerciseInstanceId);
        Task<int> GetCompletedSetsCountAsync(Guid exerciseInstanceId);
    }
}
