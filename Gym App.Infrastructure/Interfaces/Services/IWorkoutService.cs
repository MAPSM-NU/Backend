using Gym_App.Infastructure.DTOs.Exercise;
using Gym_App.Infastructure.DTOs.WorkoutDTOs;
using Gym_App.Infastructure.Transfer_Classes;
using System.Security.Claims;

namespace Gym_App.Infastructure.Interfaces.Services
{
    public interface IWorkoutService
    {
        public Task<SettersResponse> CreateWorkout(WorkoutCreationDTO workout);
        public Task<SettersResponse> UpdateWorkout(Guid workoutID, WorkoutUpdateDTO workout);
        public Task<SettersResponse> DeleteWorkout(Guid workoutID);
        public Task<SettersResponse> AddExercisesToWorkout(Guid workoutID, WorkoutExerciseDTO workoutExercises);
        public Task<SettersResponse> SetExercisesOfWorkout(Guid workoutID, WorkoutExerciseDTO workoutExercises);
        public Task<SettersResponse> DeleteExercisesFromWorkout(Guid workoutID, WorkoutExerciseDTO workoutExercises);
        public Task<GettersResponse<WorkoutViewDTO>> GetWorkoutByName(string name);//not used
        public Task<GettersResponse<WorkoutViewDTO>> GetWorkoutByID(Guid ID);
        public Task<GettersResponse<ExerciseViewDTO>> GetExercisesOfWorkout(Guid WorkoutID,int page, string sortColumn, string OrderBy, string searchTerm, int pageSize);
        public Task<GettersResponse<WorkoutViewDTO>> GetAllWorkouts(int page,int pageSize);
        public Task<Guid> GetWorkoutUserID(Guid WorkoutID);
    }
}
