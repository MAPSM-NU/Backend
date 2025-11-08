using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;

namespace Gym_App.Service.Functions.Interfaces
{
    public interface IWorkoutService
    {
        public Task<int> CreateWorkout(WorkoutDTO workout);
        public Task<int> UpdateWorkout(WorkoutDTO workout);
        public Task<int> DeleteWorkout(WorkoutDTO workout);
        public Task<int> AddExercisesToWorkout(WorkoutExerciseDTO workoutExercise);
        public Task<int> SetExercisesOfWorkout(WorkoutExerciseDTO workoutExercise);
        public Task<int> DeleteExercisesFromWorkout(WorkoutExerciseDTO workoutExercise);
        public Task<WorkoutDTO?> GetWorkoutByName(string name);
        public Task<WorkoutDTO?> GetWorkoutByID(Guid ID);
        public Task<PagedList<ExerciseDTO>?> GetExercisesOfWorkout(Guid WorkoutID,int page, string sortColumn, string OrderBy, string searchTerm, int pageSize);
        public Task<PagedList<WorkoutDTO>?> GetAllWorkouts(int page,int pageSize);
        public Task<Guid> GetWorkoutUserID(Guid WorkoutID);
    }
}
