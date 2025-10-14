using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;

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
        public Task<List<ExerciseDTO>?> GetExercisesOfWorkout(Guid WorkoutID);
        public Task<List<WorkoutDTO>?> GetAllWorkouts();
    }
}
