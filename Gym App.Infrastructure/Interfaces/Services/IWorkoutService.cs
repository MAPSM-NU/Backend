using Gym_App.Domain;
using Gym_App.Infastructure.DTOs.Exercise;
using Gym_App.Infastructure.DTOs.WorkoutDTOs;
using Gym_App.Infastructure.Transfer_Classes;
using Gym_App.Infrastructure.DTOs.Exercise;
using Gym_App.Infrastructure.DTOs.Workout;
using System.Threading;

namespace Gym_App.Infastructure.Interfaces.Services
{
    public interface IWorkoutService
    {
        public Task<SettersResponse> CreateWorkoutWithExercisesAsync(Guid userId, WorkoutCreationDTO createWorkoutDto, CancellationToken cancellationToken = default);
        public Task<SettersResponse> ManageWorkoutExerciseAsync(Guid workoutID, ExerciseManagementDTO workoutExercises, CancellationToken cancellationToken = default);
        public Task<SettersResponse> StartWorkoutAsync(Guid workoutId, Guid userId, CancellationToken cancellationToken = default);
        public Task<SettersResponse> UpdateWorkoutProgressAsync(Guid userId, WorkoutUpdateProgressDTO progressDto, CancellationToken cancellationToken = default);
        public Task<SettersResponse> CompleteWorkoutAsync(Guid workoutId, Guid userId, CancellationToken cancellationToken = default);
        public Task<SettersResponse> UpdateWorkout(Guid workoutID, WorkoutUpdateDTO workout, CancellationToken cancellationToken = default);
        public Task<SettersResponse> DeleteWorkout(Guid workoutID, CancellationToken cancellationToken = default);
        public Task<SettersResponse> AddExercisesToWorkout(Guid workoutID, WorkoutExerciseDTO workoutExercises, CancellationToken cancellationToken = default);
        public Task<SettersResponse> SetExercisesOfWorkout(Guid workoutID, WorkoutExerciseDTO workoutExercises, CancellationToken cancellationToken = default);
        public Task<SettersResponse> DeleteExercisesFromWorkout(Guid workoutID, List<Guid> exerciseInstanceIds, CancellationToken cancellationToken = default);
        public Task<GettersResponse<WorkoutViewDTO>> GetWorkoutByName(string name, CancellationToken cancellationToken = default);
        public Task<GettersResponse<WorkoutViewDTO>> GetWorkoutByID(Guid ID, CancellationToken cancellationToken = default);
        public Task<GettersResponse<PersonalRecord>> GetUserPersonalRecordsAsync(Guid userId, CancellationToken cancellationToken = default);
        public Task<GettersResponse<ExerciseDetailDTO>> GetExercisesOfWorkout(Guid WorkoutID, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize, CancellationToken cancellationToken = default);
        public Task<GettersResponse<WorkoutViewDTO>> GetAllWorkouts(int page, int pageSize, CancellationToken cancellationToken = default);
        public Task<Guid> GetWorkoutUserID(Guid WorkoutID, CancellationToken cancellationToken = default);
    }
}
