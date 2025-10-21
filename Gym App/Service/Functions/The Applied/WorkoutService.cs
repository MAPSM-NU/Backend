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

        public async Task<int> CreateWorkout(WorkoutDTO workout)//0 == faulty DTO ||1 == User not found || 2 == success
        {
            if(workout == null || string.IsNullOrEmpty(workout.Name) || workout.UserID == Guid.Empty) return 0;
            var isUserExist = await (from user in _db.Users.Include(u => u.Workouts)
                               where user.UserID == workout.UserID
                               select user).FirstOrDefaultAsync();
            if (isUserExist == null) return 1;
            var newWorkout = new Workout
            {
                WorkoutID = Guid.NewGuid(),
                Name = workout.Name,
                Description = workout.Description,
                Date = workout.Date,
                Difficulty = workout.Difficulty,
                Day = workout.Day,
                CreatAt = DateTime.Now,
                Schedule = await _db.Schedules.FirstOrDefaultAsync(s => s.User.UserID == workout.UserID)
            };
            isUserExist.Workouts?.Add(newWorkout);
            _db.Users.Update(isUserExist);
            await _db.Workouts.AddAsync(newWorkout);
            await _db.SaveChangesAsync();
            return 2;
        }
        public async Task<int> UpdateWorkout(WorkoutDTO workout)//0 == faulty DTO ||1 == Workout not found || 2 == success
        {
            if(workout == null || workout.WorkoutID == Guid.Empty) return 0;
            var WorkoutToBeUpdated = await(from w in _db.Workouts
                                      where w.WorkoutID == workout.WorkoutID
                                      select w).FirstOrDefaultAsync();
            if (WorkoutToBeUpdated == null) return 1;
            if (!string.IsNullOrEmpty(workout.Name)) WorkoutToBeUpdated.Name = workout.Name;
            if (!string.IsNullOrEmpty(workout.Description)) WorkoutToBeUpdated.Description = workout.Description;
            if (workout.Date != default) WorkoutToBeUpdated.Date = workout.Date;
            if (!string.IsNullOrEmpty(workout.Difficulty)) WorkoutToBeUpdated.Difficulty = workout.Difficulty;
            if (!string.IsNullOrEmpty(workout.Day)) WorkoutToBeUpdated.Day = workout.Day;
            _db.Workouts.Update(WorkoutToBeUpdated);
            await _db.SaveChangesAsync();
            return 2;
        }
        public async Task<int> DeleteWorkout(WorkoutDTO workout)//0 == faulty DTO ||1 == Workout not found || 2 == success
        {
            if (workout == null || workout.UserID == Guid.Empty) return 0;
            var isWorkoutExist = await (from w in _db.Workouts
                                  where w.WorkoutID == workout.WorkoutID
                                  select w).FirstOrDefaultAsync();
            if (isWorkoutExist == null) return 1;
            _db.Workouts.Remove(isWorkoutExist);
            await _db.SaveChangesAsync();
            return 2;
        }
        public async Task<int> AddExercisesToWorkout(WorkoutExerciseDTO workoutExercise)//0 == faulty DTO || 1 == Workout not found || 2 == No new exercises to add || 3 == success
        {
            if(workoutExercise == null || workoutExercise.WorkoutID == Guid.Empty) return 0;
            var workout = await _db.Workouts
                .Include(w => w.Exercises)
                .FirstOrDefaultAsync(w => w.WorkoutID == workoutExercise.WorkoutID);

            if (workout == null) return 1;

            var existingExerciseIds = new HashSet<Guid>(workout.Exercises.Select(e => e.ExerciseID));
            var exerciseIdsToAdd = workoutExercise.ExercisesID?.Where(id => !existingExerciseIds.Contains(id)).ToList();

            if (exerciseIdsToAdd == null || !exerciseIdsToAdd.Any()) return 2;

            // Fetch all exercises to add in a single query
            var exercisesToAdd = await _db.Exercises
                .Where(e => exerciseIdsToAdd.Contains(e.ExerciseID))
                .ToListAsync();

            foreach (var exercise in exercisesToAdd)
            {
                workout.Exercises.Add(exercise);
            }
            _db.Workouts.Update(workout);
            await _db.SaveChangesAsync();
            return 3;
        }
        public async Task<int> SetExercisesOfWorkout(WorkoutExerciseDTO workoutExercise)//0 == faulty DTO || 1 == Workout not found || 2 == No new exercises to add || 3 == success
        {
            if (workoutExercise == null || workoutExercise.WorkoutID == Guid.Empty) return 0;
            var isWorkoutExist = await(from w in _db.Workouts.Include(w => w.Exercises)
                                  where w.WorkoutID == workoutExercise.WorkoutID
                                  select w).FirstOrDefaultAsync();
            if(isWorkoutExist == null) return 1;
            isWorkoutExist.Exercises.Clear();
            var exerciseIDsToAdd = workoutExercise.ExercisesID.ToList();
            if (exerciseIDsToAdd == null || exerciseIDsToAdd.Count == 0) return 2;
            var ExercisesToAdd = await _db.Exercises
                                .Where(e => exerciseIDsToAdd.Contains(e.ExerciseID))
                                .ToListAsync();
            foreach (var exercise in ExercisesToAdd)
            {
                isWorkoutExist.Exercises.Add(exercise);
            }
            _db.Workouts.Update(isWorkoutExist);
            await _db.SaveChangesAsync();
            return 3;
        }
        public async Task<int> DeleteExercisesFromWorkout(WorkoutExerciseDTO workoutExercise)//0 == faulty DTO || 1 == Workout not found || 2 == No exercises to remove || 3 == success
        {
            if (workoutExercise == null || workoutExercise.WorkoutID == Guid.Empty) return 0;
            var isWorkoutExist = await(from w in _db.Workouts.Include(w => w.Exercises)
                                  where w.WorkoutID == workoutExercise.WorkoutID
                                  select w).FirstOrDefaultAsync();
            if (isWorkoutExist == null) return 1;
            var existingExerciseIDs = new HashSet<Guid>(isWorkoutExist.Exercises.Select(i => i.ExerciseID));
            var exerciseIDsToRemove = workoutExercise.ExercisesID?.Where(id => existingExerciseIDs.Contains(id)).ToList();
            if (exerciseIDsToRemove == null || !exerciseIDsToRemove.Any()) return 2;
            var ExercisesToRemove = await _db.Exercises
                                    .Where(e => exerciseIDsToRemove.Contains(e.ExerciseID))
                                    .ToListAsync();                                      
            foreach (var exercise in ExercisesToRemove)
                {
                isWorkoutExist.Exercises.Remove(exercise);
                }
            _db.Workouts.Update(isWorkoutExist);
            await _db.SaveChangesAsync();
            return 3;
        }
        public async Task<WorkoutDTO?> GetWorkoutByName(string name)
        {
            var Workout = await(from w in _db.Workouts
                          where w.Name == name
                          select new WorkoutDTO
                          {
                                WorkoutID = w.WorkoutID,
                                Name = w.Name,
                                Description = w.Description,
                                Date = w.Date,
                                Difficulty = w.Difficulty,
                                Day = w.Day,
                          }).FirstOrDefaultAsync();
            return Workout;
        }
        public async Task<WorkoutDTO?> GetWorkoutByID(Guid ID)
        {
            var Workout = await(from w in _db.Workouts
                          where w.WorkoutID == ID
                          select new WorkoutDTO
                          {
                              WorkoutID = w.WorkoutID,
                              Name = w.Name,
                              Description = w.Description,
                              Date = w.Date,
                              Difficulty = w.Difficulty,
                              Day = w.Day,
                          }).FirstOrDefaultAsync();
            return Workout;
        }
        public async Task<List<ExerciseDTO>?> GetExercisesOfWorkout(Guid WorkoutID)
        {
            var Exercises = await(from w in _db.Workouts
                                 where w.WorkoutID == WorkoutID
                                 from e in w.Exercises
                                 select new ExerciseDTO
                                 {
                                     ExerciseID = e.ExerciseID,
                                     Name = e.Name,
                                     Description = e.Description,
                                     Difficulty = e.Difficulty,
                                     VideoUrl = e.VideoUrl,
                                 }).ToListAsync();
            return Exercises;
        }
        public async Task<List<WorkoutDTO>?> GetAllWorkouts()
        {
            var Workouts = await(from w in _db.Workouts
                                 select new WorkoutDTO
                                 {
                                     WorkoutID = w.WorkoutID,
                                     Name = w.Name,
                                     Description = w.Description,
                                     Date = w.Date,
                                     Difficulty = w.Difficulty,
                                     Day = w.Day,
                                 }).ToListAsync();
            return Workouts;
        }

    }
}
