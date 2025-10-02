using DocumentFormat.OpenXml.Wordprocessing;
using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gym_App.Service.Functions.The_Applied
{
    public class WorkoutService : IWorkoutService
    {
        private readonly DbBase _db;
        public WorkoutService(DbBase db)
        {
            _db = db;
        }

        public async Task<int> CreateWorkout(WorkoutDTO workout)
        {
            var isUserExist = (from user in _db.Users.Include(u => u.Workouts)
                               where user.UserID == workout.UserID
                               select user).FirstOrDefault();
            if (isUserExist == null) return await Task.FromResult(0);
            var newWorkout = new Workout
            {
                WorkoutID = Guid.NewGuid(),
                Name = workout.Name,
                Description = workout.Description,
                Date = workout.Date,
                Difficulty = workout.Difficulty,
                Day = workout.Day,
                Schedule = _db.Schedules.FirstOrDefault(s => s.User.UserID == workout.UserID)
            };
            isUserExist.Workouts.Add(newWorkout);
            _db.Users.Update(isUserExist);
            _db.Workouts.Add(newWorkout);
            await _db.SaveChangesAsync();
            return await Task.FromResult(1);
        }
        public async Task<int> DeleteWorkout(WorkoutDTO workout)
        {
            var isWorkoutExist = (from w in _db.Workouts
                                  where w.WorkoutID == workout.WorkoutID
                                  select w).FirstOrDefault();
            if (isWorkoutExist == null) return await Task.FromResult(0);
            _db.Workouts.Remove(isWorkoutExist);
            await _db.SaveChangesAsync();
            return await Task.FromResult(1);
        }
        public async Task<int> UpdateWorkout(WorkoutDTO workout)
        {
            var WorkoutToBeUpdated = (from w in _db.Workouts
                                      where w.WorkoutID == workout.WorkoutID
                                      select w).FirstOrDefault();
            if (WorkoutToBeUpdated == null) return await Task.FromResult(0);
            if (!string.IsNullOrEmpty(workout.Name)) WorkoutToBeUpdated.Name = workout.Name;
            if (!string.IsNullOrEmpty(workout.Description)) WorkoutToBeUpdated.Description = workout.Description;
            if (workout.Date != default) WorkoutToBeUpdated.Date = workout.Date;
            if (!string.IsNullOrEmpty(workout.Difficulty)) WorkoutToBeUpdated.Difficulty = workout.Difficulty;
            if (!string.IsNullOrEmpty(workout.Day)) WorkoutToBeUpdated.Day = workout.Day;
            _db.Workouts.Update(WorkoutToBeUpdated);
            await _db.SaveChangesAsync();
            return await Task.FromResult(1);
        }
        public async Task<int> AddExercisesToWorkout(WorkoutExerciseDTO workoutExercise)
        {
            bool AddedAny = false;
            var isWorkoutExist = (from w in _db.Workouts.Include(w => w.Exercises)
                                  where w.WorkoutID == workoutExercise.WorkoutID
                                  select w).FirstOrDefault();
            if (isWorkoutExist == null) return await Task.FromResult(0);
            foreach (var exerciseId in workoutExercise.ExercisesID)
            {
                var exercise = _db.Exercises.FirstOrDefault(e => e.ExerciseID == exerciseId);
                if (exercise != null && !isWorkoutExist.Exercises.Any(e => e.ExerciseID == exerciseId))
                {
                    isWorkoutExist.Exercises.Add(exercise);
                    AddedAny = true;
                }
            }
            if (!AddedAny) {return await Task.FromResult(0); }
            _db.Workouts.Update(isWorkoutExist);
            await _db.SaveChangesAsync();
            return await Task.FromResult(1);
        }

        public async Task<int> DeleteExercisesFromWorkout(WorkoutExerciseDTO workoutExercise)
        {
            bool DeletedAny = false;
            var isWorkoutExist = (from w in _db.Workouts.Include(w => w.Exercises)
                                  where w.WorkoutID == workoutExercise.WorkoutID
                                  select w).FirstOrDefault();
            if (isWorkoutExist == null) return await Task.FromResult(0);
            foreach (var exerciseId in workoutExercise.ExercisesID)
            {
                var exercise = _db.Exercises.FirstOrDefault(e => e.ExerciseID == exerciseId);
                if (exercise != null && isWorkoutExist.Exercises.Any(e => e.ExerciseID == exerciseId))
                {
                    isWorkoutExist.Exercises.Remove(exercise);
                    DeletedAny = true;
                }
            }
            if(!DeletedAny) { return await Task.FromResult(0); }
            _db.Workouts.Update(isWorkoutExist);
            await _db.SaveChangesAsync();
            return await Task.FromResult(0);
        }
        public Task<Workout> GetWorkoutByName(string name)
        {
            var Workout = _db.Workouts.Include(w => w.Exercises).FirstOrDefault(w => w.Name == name);
            return Task.FromResult(Workout!);
        }
        public Task<IQueryable<Workout>> GetAllWorkouts()
        {
            var Workouts = _db.Workouts.Include(w => w.Exercises).AsQueryable();
            return Task.FromResult(Workouts);
        }

    }
}
