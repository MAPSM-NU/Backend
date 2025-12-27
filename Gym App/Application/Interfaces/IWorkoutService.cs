using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.DTOs.Exercise;
using Gym_App.Infastructure.DTOs.WorkoutDTOs;
using Gym_App.Infastructure.Transfer_Classes;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Gym_App.Application.Interfaces
{
    public interface IWorkoutService
    {
        public Task<SettersResponse> CreateWorkout(ClaimsPrincipal User,WorkoutCreationDTO workout);
        public Task<SettersResponse> UpdateWorkout(ClaimsPrincipal User,Guid workoutID, WorkoutUpdateDTO workout);
        public Task<SettersResponse> DeleteWorkout(ClaimsPrincipal User, Guid workoutID);
        public Task<SettersResponse> AddExercisesToWorkout(ClaimsPrincipal User, Guid workoutID, WorkoutExerciseDTO workoutExercises);
        public Task<SettersResponse> SetExercisesOfWorkout(ClaimsPrincipal User, Guid workoutID, WorkoutExerciseDTO workoutExercises);
        public Task<SettersResponse> DeleteExercisesFromWorkout(ClaimsPrincipal User, Guid workoutID, WorkoutExerciseDTO workoutExercises);
        public Task<GettersResponse<WorkoutViewDTO>> GetWorkoutByName(string name);//not used
        public Task<GettersResponse<WorkoutViewDTO>> GetWorkoutByID(Guid ID);
        public Task<GettersResponse<ExerciseViewDTO>> GetExercisesOfWorkout(Guid WorkoutID,int page, string sortColumn, string OrderBy, string searchTerm, int pageSize);
        public Task<GettersResponse<WorkoutViewDTO>> GetAllWorkouts(int page,int pageSize);
        public Task<Guid> GetWorkoutUserID(Guid WorkoutID);
    }
}
