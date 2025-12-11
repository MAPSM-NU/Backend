using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.DTOs.Exercise;
using Gym_App.Infastructure.DTOs.Muscle;
using Gym_App.Infastructure.Transfer_Classes;

namespace Gym_App.Application.Interfaces
{
    public interface IExerciseService
    {
        public Task<SettersResponse> CreateExercise(ExerciseCreationDTO exercise);
        public Task<SettersResponse> UpdateExercise(Guid exerciseID, ExerciseCreationDTO exercise);
        public Task<SettersResponse> DeleteExercise(Guid exerciseId);
        public Task<SettersResponse> AddMusclesToExercise(Guid exerciseID, ExerciseMusclesDTO exerciseMuscles);
        public Task<SettersResponse> RemoveMusclesFromExercise(Guid exerciseID, ExerciseMusclesDTO exerciseMuscles);
        public Task<ExerciseViewDTO?> GetExerciseByName(string name);
        public Task<ExerciseMiniViewDTO?> GetExerciseByID(Guid id);
        public Task<List<MuscleViewDTO>?> GetExerciseMuscles(Guid exerciseID);
        public Task<PagedList<ExerciseViewDTO>>? GetExercisesByMuscle(ExerciseListDTO muscles,int page,int pageSize);
        public Task<PagedList<ExerciseViewDTO>?> GetExercisesByFilter(int page, string sortColumn, string OrderBy, string searchTerm, int pageSize = 5);
        public Task<PagedList<ExerciseViewDTO>?> GetAllExercises(int page,int pageSize = 5);
    }
}
