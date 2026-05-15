using Gym_App.Domain;
using Gym_App.Infastructure.DTOs.Exercise;
using Gym_App.Infastructure.DTOs.WorkoutDTOs;
using Gym_App.Infastructure.Transfer_Classes;
using Gym_App.Infrastructure.DTOs.Exercise;
using Gym_App.Infrastructure.DTOs.Workout;

namespace Gym_App.Infastructure.Interfaces.Services
{
    public interface IWorkoutService
    {
        public Task<SettersResponse> CreateWorkoutWithExercisesAsync(Guid userId,WorkoutCreationDTO createWorkoutDto);
        public Task<SettersResponse> ManageWorkoutExerciseAsync(Guid workoutID, ExerciseManagementDTO workoutExercises);
        public Task<SettersResponse> StartWorkoutAsync(Guid workoutId, Guid userId);
        public Task<SettersResponse> UpdateWorkoutProgressAsync(Guid userId,WorkoutUpdateProgressDTO progressDto);
        public Task<SettersResponse> CompleteWorkoutAsync(Guid workoutId, Guid userId);
        public Task<SettersResponse> UpdateWorkout(Guid workoutID, WorkoutUpdateDTO workout);
        public Task<SettersResponse> DeleteWorkout(Guid workoutID);
        public Task<SettersResponse> AddExercisesToWorkout(Guid workoutID, WorkoutExerciseDTO workoutExercises);
        public Task<SettersResponse> SetExercisesOfWorkout(Guid workoutID, WorkoutExerciseDTO workoutExercises);
        public Task<SettersResponse> DeleteExercisesFromWorkout(Guid workoutID, List<Guid> exerciseInstanceIds);
        public Task<GettersResponse<WorkoutViewDTO>> GetWorkoutByName(string name);//not used
        public Task<GettersResponse<WorkoutViewDTO>> GetWorkoutByID(Guid ID);
        public Task<GettersResponse<PersonalRecord>> GetUserPersonalRecordsAsync(Guid userId);
        public Task<GettersResponse<ExerciseDetailDTO>> GetExercisesOfWorkout(Guid WorkoutID,int page, string sortColumn, string OrderBy, string searchTerm, int pageSize);
        public Task<GettersResponse<WorkoutViewDTO>> GetAllWorkouts(int page,int pageSize);
        public Task<Guid> GetWorkoutUserID(Guid WorkoutID);
    }
}
