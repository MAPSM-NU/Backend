using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Gym_App.Service.Functions.The_Applied
{
    public class ExerciseService : IExerciseService
    {
        private readonly DbBase _db;
        public ExerciseService(DbBase db)
        {
            _db = db;
        }
        public async Task<int> CreateExercise(ExerciseDTO exercise)
        {
            if (exercise == null || string.IsNullOrWhiteSpace(exercise.Name))
            {
                return await Task.FromResult(0);
            }
            var isExerciseExists = (from E in _db.Exercises
                                   where E.Name.ToLower() == exercise.Name.ToLower()
                                   select E).FirstOrDefault();
            if(isExerciseExists != null) return await Task.FromResult(0);
            var newExercise = new Exercise
            {
                ExerciseID = Guid.NewGuid(),
                Name = exercise.Name,
                Description = exercise.Description,
                Difficulty = exercise.Difficulty,
                VideoUrl = exercise.VideoUrl,
                Category = exercise.Category,
                Grip = exercise.Grip
            };
            _db.Exercises.Add(newExercise);
            await _db.SaveChangesAsync();
            return 1;
        }

        public async Task<int> DeleteExercise(Guid ExerciseID)
        {
            
            var isExerciseExists = (from E in _db.Exercises
                                    where E.ExerciseID == ExerciseID
                                    select E).FirstOrDefault();
            if (isExerciseExists == null) return await Task.FromResult(0);
            _db.Exercises.Remove(isExerciseExists);
            await _db.SaveChangesAsync();
            return await Task.FromResult(1);
        }

        public async Task<int> UpdateExercise(ExerciseDTO exercise)
        {
            var toBeUpdated = (from E in _db.Exercises
                               where E.ExerciseID == exercise.ExerciseID
                               select E).FirstOrDefault();
            if (toBeUpdated == null) return await Task.FromResult(0);
            if(!string.IsNullOrEmpty(exercise.Category)) toBeUpdated.Category = exercise.Category;
            if(!string.IsNullOrEmpty(exercise.Difficulty)) toBeUpdated.Difficulty = exercise.Difficulty;
            if(!string.IsNullOrEmpty(exercise.Name)) toBeUpdated.Name = exercise.Name;
            if(!string.IsNullOrEmpty(exercise.Description)) toBeUpdated.Description = exercise.Description;
            if (!string.IsNullOrEmpty(exercise.VideoUrl)) toBeUpdated.VideoUrl = exercise.VideoUrl;
            _db.Exercises.Update(toBeUpdated);
            await _db.SaveChangesAsync();
            return await Task.FromResult(1);
        }
        public async Task<int> AddMusclesToExercise(Guid exerciseId, List<Guid> muscleIds)
        {
            bool AnyAdded = false;
            var exercise = (from e in _db.Exercises.Include(m => m.Muscles)//really important. If you dont add the include the db will give you the Exercise without its muscles
                            where e.ExerciseID == exerciseId               //so when you try to access the muscles, the list will always be empty.
                            select e).FirstOrDefault();
            if (exercise == null) return await Task.FromResult(0);
            foreach (var muscleId in muscleIds)
            {
                var muscle = (from m in _db.Muscles
                              where m.MusclesID == muscleId
                              select m).FirstOrDefault();
                if (muscle != null && !exercise.Muscles.Any(m => m.MusclesID == muscleId))
                {
                    exercise.Muscles.Add(muscle);
                    AnyAdded = true;
                }
            }
            if(!AnyAdded) return await Task.FromResult(0);
            _db.Exercises.Update(exercise);
            await _db.SaveChangesAsync();
            return await Task.FromResult(1);
        }
        public async Task<int> RemoveMusclesFromExercise(Guid exerciseId, List<Guid> muscleIds)
        {
            bool DeletedAny = false;
            var exercise = (from e in _db.Exercises.Include(m =>m.Muscles)//really important. If you dont add the include the db will give you the Exercise without its muscles
                            where e.ExerciseID == exerciseId              //so when you try to access the muscles, the list will always be empty.       
                            select e).FirstOrDefault();
            if (exercise == null) return await Task.FromResult(0);
            foreach (var muscleId in muscleIds)
            {
                var muscle = exercise.Muscles.FirstOrDefault(m => m.MusclesID == muscleId);
                if (muscle != null)
                {
                    exercise.Muscles.Remove(muscle);
                    DeletedAny = true;
                }
            }
            if (!DeletedAny) return await Task.FromResult(0);
            _db.Exercises.Update(exercise);
            await _db.SaveChangesAsync();
            return await Task.FromResult(1);
        }
        public Task<ExerciseDTO?> GetExerciseByName(string name)
        {
            var Exercise = (from e in _db.Exercises.Include(e => e.Muscles)
                            where e.Name == name
                            select new ExerciseDTO
                            {
                                ExerciseID = e.ExerciseID,
                                Name = e.Name,
                                Description = e.Description,
                                Difficulty = e.Difficulty,
                                VideoUrl = e.VideoUrl,
                                Category = e.Category,
                                Grip = e.Grip,
                            }).FirstOrDefault();
            if (Exercise == null) return Task.FromResult(Exercise);
            return Task.FromResult(Exercise);
        }
        public async Task<List<ExerciseDTO>>? GetExercisesByMuscle(ExerciseListDTO muscles)
        {
            var muscleNames = muscles.Muscles;
            var query = await (from e in _db.Exercises
                where muscleNames.All(name => e.Muscles.Any(m => m.Name == name))
                select new ExerciseDTO
                {
                    ExerciseID = e.ExerciseID,
                    Name = e.Name,
                    Description = e.Description,
                    Difficulty = e.Difficulty,
                    VideoUrl = e.VideoUrl,
                    Category = e.Category,
                    Grip = e.Grip,
                }).ToListAsync();
            return await Task.FromResult(query);
        }
        public async Task<List<ExerciseDTO>> GetAllExercises()
        {
            var exercises = await (from e in _db.Exercises
                            select  new ExerciseDTO
                            {
                                ExerciseID = e.ExerciseID,
                                Name = e.Name,
                                Description = e.Description,
                                Difficulty = e.Difficulty,
                                VideoUrl = e.VideoUrl,
                                Category = e.Category,
                                Grip = e.Grip,
                            }).ToListAsync();
            return await Task.FromResult(exercises);
        }
    }
}
