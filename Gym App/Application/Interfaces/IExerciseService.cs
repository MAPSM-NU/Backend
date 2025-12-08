using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.DTOs;
using Gym_App.Infastructure.DTOs.Muscle;

namespace Gym_App.Application.Interfaces
{
    public interface IExerciseService
    {
        public Task<int> CreateExercise(ExerciseDTO exercise);
        public Task<int> UpdateExercise(ExerciseDTO exercise);
        public Task<int> DeleteExercise(Guid exerciseId);
        public Task<int> AddMusclesToExercise(ExerciseMusclesDTO exerciseMuscles);
        public Task<int> RemoveMusclesFromExercise(ExerciseMusclesDTO exerciseMuscles);
        public Task<ExerciseDTO?> GetExerciseByName(string name);
        public Task<ExerciseDTO?> GetExerciseByID(Guid id);
        public Task<List<MuscleViewDTO>?> GetExerciseMuscles(Guid exerciseID);
        public Task<PagedList<ExerciseDTO>>? GetExercisesByMuscle(ExerciseListDTO muscles,int page,int pageSize);
        public Task<PagedList<ExerciseDTO>?> GetExercisesByFilter(int page, string sortColumn, string OrderBy, string searchTerm, int pageSize = 5);
        public Task<PagedList<ExerciseDTO>?> GetAllExercises(int page,int pageSize = 5);
    }
}
