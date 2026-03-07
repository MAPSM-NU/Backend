using Gym_App.Infastructure.DTOs.Exercise;
using Gym_App.Infastructure.DTOs.Muscle;
using Gym_App.Infastructure.Transfer_Classes;

namespace Gym_App.Infastructure.Interfaces.Services
{
    public interface IExerciseService
    {
        public Task<SettersResponse> CreateExercise(ExerciseCreationDTO exercise);
        public Task<SettersResponse> UpdateExercise(Guid exerciseID, ExerciseCreationDTO exercise);
        public Task<SettersResponse> DeleteExercise(Guid exerciseId);
        public Task<SettersResponse> AddMusclesToExercise(Guid exerciseID, ExerciseMusclesDTO exerciseMuscles);
        public Task<SettersResponse> RemoveMusclesFromExercise(Guid exerciseID, ExerciseMusclesDTO exerciseMuscles);
        public Task<GettersResponse<ExerciseViewDTO>> GetExerciseByName(string name);
        public Task<GettersResponse<ExerciseMiniViewDTO>> GetExerciseByID(Guid id);
        public Task<GettersResponse<List<MuscleViewDTO>>> GetExerciseMuscles(Guid exerciseID);
        public Task<GettersResponse<ExerciseViewDTO>> GetExercisesByMuscle(ExerciseListDTO muscles,int page,int pageSize);
        public Task<GettersResponse<ExerciseViewDTO>> GetExercisesByFilter(int page, string sortColumn, string OrderBy, string searchTerm, int pageSize = 5);
        public Task<GettersResponse<ExerciseViewDTO>> GetAllExercises(int page,int pageSize = 5);
    }
}
