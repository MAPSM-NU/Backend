using Gym_App.Infastructure.DTOs.Exercise;
using Gym_App.Infastructure.DTOs.Muscle;
using Gym_App.Infastructure.Transfer_Classes;
using System.Threading;

namespace Gym_App.Infastructure.Interfaces.Services
{
    public interface IExerciseService
    {
        public Task<SettersResponse> CreateExercise(ExerciseCreationDTO exercise, CancellationToken cancellationToken = default);
        public Task<SettersResponse> UpdateExercise(Guid exerciseID, ExerciseCreationDTO exercise, CancellationToken cancellationToken = default);
        public Task<SettersResponse> DeleteExercise(Guid exerciseId, CancellationToken cancellationToken = default);
        public Task<SettersResponse> AddMusclesToExercise(Guid exerciseID, ExerciseMusclesDTO exerciseMuscles, CancellationToken cancellationToken = default);
        public Task<SettersResponse> RemoveMusclesFromExercise(Guid exerciseID, ExerciseMusclesDTO exerciseMuscles, CancellationToken cancellationToken = default);
        public Task<GettersResponse<ExerciseViewDTO>> GetExerciseByName(string name, CancellationToken cancellationToken = default);
        public Task<GettersResponse<ExerciseMiniViewDTO>> GetExerciseByID(Guid id, CancellationToken cancellationToken = default);
        public Task<GettersResponse<List<MuscleViewDTO>>> GetExerciseMuscles(Guid exerciseID, CancellationToken cancellationToken = default);
        public Task<GettersResponse<ExerciseViewDTO>> GetExercisesByMuscle(ExerciseListDTO muscles, int page, int pageSize, CancellationToken cancellationToken = default);
        public Task<GettersResponse<ExerciseViewDTO>> GetExercisesByFilter(int page, string sortColumn, string OrderBy, string searchTerm, int pageSize = 5, CancellationToken cancellationToken = default);
        public Task<GettersResponse<ExerciseViewDTO>> GetAllExercises(int page, int pageSize = 5, CancellationToken cancellationToken = default);
    }
}
