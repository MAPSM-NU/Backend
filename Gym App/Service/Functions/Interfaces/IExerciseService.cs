using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;

namespace Gym_App.Service.Functions.Interfaces
{
    public interface IExerciseService
    {
        public Task<int> CreateExercise(ExerciseDTO exercise);
        public Task<int> UpdateExercise(ExerciseDTO exercise);
        public Task<int> DeleteExercise(Guid exerciseId);
        public Task<int> AddMusclesToExercise(ExerciseMusclesDTO exerciseMuscles);
        public Task<int> RemoveMusclesFromExercise(ExerciseMusclesDTO exerciseMuscles);
        public Task<ExerciseDTO?> GetExerciseByName(string name);
        public Task<List<ExerciseDTO>>? GetExercisesByMuscle(ExerciseListDTO muscles);
        public Task<List<ExerciseDTO>> GetAllExercises();
    }
}
