using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Gym_App.Service.Functions.Interfaces
{
    public interface IWorkoutService
    {
        public Task<int> CreateWorkout(ClaimsPrincipal User,WorkoutDTO workout);
        public Task<int> UpdateWorkout(ClaimsPrincipal User, WorkoutDTO workout);
        public Task<int> DeleteWorkout(ClaimsPrincipal User, Guid workoutID);
        public Task<int> AddExercisesToWorkout(ClaimsPrincipal User, WorkoutExerciseDTO workoutExercise);
        public Task<int> SetExercisesOfWorkout(ClaimsPrincipal User, WorkoutExerciseDTO workoutExercise);
        public Task<int> DeleteExercisesFromWorkout(ClaimsPrincipal User, WorkoutExerciseDTO workoutExercise);
        public Task<WorkoutDTO?> GetWorkoutByName(string name);
        public Task<WorkoutDTO?> GetWorkoutByID(Guid ID);
        public Task<PagedList<ExerciseDTO>?> GetExercisesOfWorkout(Guid WorkoutID,int page, string sortColumn, string OrderBy, string searchTerm, int pageSize);
        public Task<PagedList<WorkoutDTO>?> GetAllWorkouts(int page,int pageSize);
        public Task<Guid> GetWorkoutUserID(Guid WorkoutID);
    }
}
