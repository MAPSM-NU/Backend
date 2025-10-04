using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;

namespace Gym_App.Service.Functions.Interfaces
{
    public interface IExerciseService
    {
        public Task<int> CreateExercise(ExerciseDTO exercise);
        public Task<int> UpdateExercise(ExerciseDTO exercise);
        public Task<int> DeleteExercise(Guid exerciseId);
        public Task<int> AddMusclesToExercise(Guid exerciseId, List<Guid> muscleIds);
        public Task<int> RemoveMusclesFromExercise(Guid exerciseId, List<Guid> muscleIds);
        public Task<Exercise?> GetExerciseByName(string name);
        public Task<IQueryable<Exercise>>? GetExercisesByMuscle(ExerciseListDTO muscles);
        public Task<IQueryable<Exercise>> GetAllExercises();
    }
}
