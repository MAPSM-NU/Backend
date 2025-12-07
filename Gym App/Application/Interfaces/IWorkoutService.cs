using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.DTOs;
using Gym_App.Infastructure.DTOs.WorkoutDTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Gym_App.Application.Interfaces
{
    public interface IWorkoutService
    {
        public Task<int> CreateWorkout(ClaimsPrincipal User,WorkoutCreationDTO workout);
        public Task<int> UpdateWorkout(ClaimsPrincipal User,Guid workoutID, WorkoutUpdateDTO workout);
        public Task<int> DeleteWorkout(ClaimsPrincipal User, Guid workoutID);
        public Task<int> AddExercisesToWorkout(ClaimsPrincipal User, Guid workoutID, List<Guid> exercises);
        public Task<int> SetExercisesOfWorkout(ClaimsPrincipal User, Guid workoutID, List<Guid> exercises);
        public Task<int> DeleteExercisesFromWorkout(ClaimsPrincipal User, Guid workoutID, List<Guid> exercises);
        public Task<WorkoutViewDTO?> GetWorkoutByName(string name);
        public Task<WorkoutViewDTO?> GetWorkoutByID(Guid ID);
        public Task<PagedList<ExerciseDTO>?> GetExercisesOfWorkout(Guid WorkoutID,int page, string sortColumn, string OrderBy, string searchTerm, int pageSize);
        public Task<PagedList<WorkoutViewDTO>?> GetAllWorkouts(int page,int pageSize);
        public Task<Guid> GetWorkoutUserID(Guid WorkoutID);
    }
}
