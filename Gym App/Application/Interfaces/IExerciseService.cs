using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.DTOs.Exercise;
using Gym_App.Infastructure.DTOs.Muscle;

namespace Gym_App.Application.Interfaces
{
    public interface IExerciseService
    {
        public Task<int> CreateExercise(ExerciseCreationDTO exercise);
        public Task<int> UpdateExercise(Guid exerciseID, ExerciseCreationDTO exercise);
        public Task<int> DeleteExercise(Guid exerciseId);
        public Task<int> AddMusclesToExercise(Guid exerciseID, ExerciseMusclesDTO exerciseMuscles);
        public Task<int> RemoveMusclesFromExercise(Guid exerciseID, ExerciseMusclesDTO exerciseMuscles);
        public Task<ExerciseViewDTO?> GetExerciseByName(string name);
        public Task<ExerciseMiniViewDTO?> GetExerciseByID(Guid id);
        public Task<List<MuscleViewDTO>?> GetExerciseMuscles(Guid exerciseID);
        public Task<PagedList<ExerciseViewDTO>>? GetExercisesByMuscle(ExerciseListDTO muscles,int page,int pageSize);
        public Task<PagedList<ExerciseViewDTO>?> GetExercisesByFilter(int page, string sortColumn, string OrderBy, string searchTerm, int pageSize = 5);
        public Task<PagedList<ExerciseViewDTO>?> GetAllExercises(int page,int pageSize = 5);
    }
}
