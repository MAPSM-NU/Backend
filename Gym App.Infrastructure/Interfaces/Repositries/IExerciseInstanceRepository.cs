using Gym_App.Domain;
using Gym_App.Infastructure.Repositries;

namespace Gym_App.Infastructure.Interfaces.Repositries
{
    public interface IExerciseInstanceRepository : IBaseRepositry<ExerciseInstance>
    {
        /// <summary>
        /// Get all exercise instances for a workout
        /// </summary>
        Task<IEnumerable<ExerciseInstance>> GetExercisesByWorkoutAsync(Guid workoutId);

        /// <summary>
        /// Get exercise instances with their sets loaded
        /// </summary>
        Task<ExerciseInstance?> GetWithSetsAsync(Guid exerciseInstanceId);

        /// <summary>
        /// Get count of completed exercises in a workout
        /// </summary>
        Task<int> GetCompletedExercisesCountAsync(Guid workoutId);
    }
}
