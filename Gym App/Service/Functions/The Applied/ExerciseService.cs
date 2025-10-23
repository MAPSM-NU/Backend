
using DocumentFormat.OpenXml.Wordprocessing;
using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Service.Functions.Interfaces;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace Gym_App.Service.Functions.The_Applied
{
    public class ExerciseService : IExerciseService
    {
        private readonly DbBase _db;
        public ExerciseService(DbBase db)
        {
            _db = db;
        }
        public async Task<int> CreateExercise(ExerciseDTO exercise)//0 == null exercise or name || 1 == exercise already exists || 2 = success
        {
            if (exercise == null || string.IsNullOrWhiteSpace(exercise.Name)) return 0;

            var isExerciseExists = await (from E in _db.Exercises
                                   where E.Name.ToLower() == exercise.Name.ToLower()
                                   select E).FirstOrDefaultAsync();
            if(isExerciseExists != null) return 1;
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
            return 2;
        }

        public async Task<int> DeleteExercise(Guid ExerciseID)
        {
            
            var isExerciseExists = await (from E in _db.Exercises
                                    where E.ExerciseID == ExerciseID
                                    select E).FirstOrDefaultAsync();
            if (isExerciseExists == null) return 0;
            _db.Exercises.Remove(isExerciseExists);
            await _db.SaveChangesAsync();
            return 1;
        }

        public async Task<int> UpdateExercise(ExerciseDTO exercise)
        {
            var toBeUpdated = await (from E in _db.Exercises
                               where E.ExerciseID == exercise.ExerciseID
                               select E).FirstOrDefaultAsync();
            if (toBeUpdated == null) return 0;
            if(!string.IsNullOrEmpty(exercise.Category)) toBeUpdated.Category = exercise.Category;
            if(!string.IsNullOrEmpty(exercise.Difficulty)) toBeUpdated.Difficulty = exercise.Difficulty;
            if(!string.IsNullOrEmpty(exercise.Name)) toBeUpdated.Name = exercise.Name;
            if(!string.IsNullOrEmpty(exercise.Description)) toBeUpdated.Description = exercise.Description;
            if (!string.IsNullOrEmpty(exercise.VideoUrl)) toBeUpdated.VideoUrl = exercise.VideoUrl;
            _db.Exercises.Update(toBeUpdated);
            await _db.SaveChangesAsync();
            return 1;
        }
        public async Task<int> AddMusclesToExercise(ExerciseMusclesDTO exerciseMuscles)//0 == Faulty DTO || 1 == exercise not found || 2 == no new muscles added || 3 == muscles to add not found || 4 == success
        {
            if(exerciseMuscles == null || exerciseMuscles.Muscles == null || exerciseMuscles.Muscles.Count == 0) return 0;
            var exercise = await(from e in _db.Exercises.Include(m =>m.Muscles)
                            where e.ExerciseID == exerciseMuscles.ExerciseID       
                            select e).FirstOrDefaultAsync();
            if (exercise == null) return 1;
            var existingMuscleIDs = new HashSet<Guid>(exercise.Muscles.Select(m => m.MusclesID));
            var musclesIDsToAdd = exerciseMuscles.Muscles?.Where(id => !existingMuscleIDs.Contains(id)).ToList();
            if (musclesIDsToAdd == null || musclesIDsToAdd.Count == 0) return 2;
            var musclesToAdd = await (from m in _db.Muscles
                               where musclesIDsToAdd.Contains(m.MusclesID)
                               select m).ToListAsync();
            if (musclesToAdd.Count == 0) return 3;
            foreach (var muscle in musclesToAdd)
            {
                exercise.Muscles.Add(muscle);
            }
            _db.Exercises.Update(exercise);
            await _db.SaveChangesAsync();
            return 4;
        }
        public async Task<int> RemoveMusclesFromExercise(ExerciseMusclesDTO exerciseMuscles)//0 == Faulty DTO || 1 == exercise not found || 2 == no muscles to remove || 3 == muscles to remove not found || 4 == success
        {
            if(exerciseMuscles == null || exerciseMuscles.Muscles == null || exerciseMuscles.Muscles.Count == 0) return 0;
            var exercise = await(from e in _db.Exercises.Include(m =>m.Muscles)
                            where e.ExerciseID == exerciseMuscles.ExerciseID       
                            select e).FirstOrDefaultAsync();
            if (exercise == null) return 1;
            var existingMuscleIDs = new HashSet<Guid>(exercise.Muscles.Select(m => m.MusclesID));
            var musclesIDsToRemove = exerciseMuscles.Muscles?.Where(id => existingMuscleIDs.Contains(id)).ToList();
            if (musclesIDsToRemove == null || musclesIDsToRemove.Count == 0) return 2;
            var musclesToRemove = await (from m in _db.Muscles
                               where musclesIDsToRemove.Contains(m.MusclesID)
                               select m).ToListAsync();
            if (musclesToRemove.Count == 0) return 3;
            foreach (var muscle in musclesToRemove)
            {
                exercise.Muscles.Remove(muscle);
            }
            _db.Exercises.Update(exercise);
            await _db.SaveChangesAsync();
            return 4;
        }
        public async Task<ExerciseDTO?> GetExerciseByName(string name)
        {
            var Exercise = await (from e in _db.Exercises
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
                            }).FirstOrDefaultAsync();
            if (Exercise == null) return Exercise;
            return Exercise;
        }
        public async Task<ExerciseDTO?> GetExerciseByID(Guid id)
        {
            var Exercise = await (from e in _db.Exercises
                            where e.ExerciseID == id
                            select new ExerciseDTO
                            {
                                ExerciseID = e.ExerciseID,
                                Name = e.Name,
                                Description = e.Description,
                                Difficulty = e.Difficulty,
                                VideoUrl = e.VideoUrl,
                                Category = e.Category,
                                Grip = e.Grip,
                            }).FirstOrDefaultAsync();
            if (Exercise == null) return Exercise;
            return Exercise;
        }
        public async Task<List<MuscleDTO>?> GetExerciseMuscles(Guid exerciseID)
        {
            var exercise = await (from e in _db.Exercises.Include(e=>e.Muscles)
                                 where e.ExerciseID == exerciseID
                                 select e).FirstOrDefaultAsync();
            if (exercise == null) return null;
            var muscleDTOs = (from m in exercise.Muscles
                              select new MuscleDTO
                              {
                                MusclesID = m.MusclesID,
                                Name = m.Name,
                                Description = m.Description
                              }).ToList();
            return muscleDTOs;
        }
        public async Task<PagedList<ExerciseDTO>>? GetExercisesByMuscle(ExerciseListDTO muscles,int page,int pageSize)
        {
            if (page == 0) page = 1;
            if(pageSize == 0) pageSize = 10;
            var muscleNames = muscles.Muscles;
            var exercisequery = (from e in _db.Exercises
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
                });
            var exercises = await PagedList<ExerciseDTO>.CreateAsync(exercisequery, page, pageSize);
            return exercises;
        }
        public async Task<PagedList<ExerciseDTO>?> GetExercisesByFilter(int page, string sortColumn, string OrderBy, string searchTerm, int pageSize = 5)
        {
            IQueryable<Exercise> exerciseQuery = _db.Exercises;

            if(!string.IsNullOrEmpty(searchTerm))exerciseQuery = exerciseQuery.Where(e => e.Name.Contains(searchTerm) || e.Difficulty.Contains(searchTerm));

            if (!string.IsNullOrEmpty(sortColumn))
            {
                Expression<Func<Exercise, Object>> keySelector = sortColumn.ToLower() switch // throws error when sortColumn is null
                {
                    "name" => Exercise => Exercise.Name,
                    "difficulty" => Exercise => Exercise.Difficulty,
                    _ => Exercise => Exercise.ExerciseID
                };
                if (!string.IsNullOrEmpty(OrderBy))exerciseQuery = exerciseQuery.OrderBy(keySelector);//If any kind of value is in OrderBy then it is ascending
                else exerciseQuery = exerciseQuery.OrderByDescending(keySelector);
            }
            var exercisesResponse = (exerciseQuery
                                        .Select(e => new ExerciseDTO
                                        {
                                            ExerciseID = e.ExerciseID,
                                            Name = e.Name,
                                            Description = e.Description,
                                            Difficulty = e.Difficulty,
                                            VideoUrl = e.VideoUrl,
                                            Category = e.Category,
                                            Grip = e.Grip,
                                        }));
        var exercises = await PagedList<ExerciseDTO>.CreateAsync(exercisesResponse, page, pageSize);
            return exercises;
        }
        public async Task<PagedList<ExerciseDTO>?> GetAllExercises(int page, int pageSize = 5)
        {
            IQueryable<Exercise> exerciseQuery = _db.Exercises;

            var exercisesResponse = (exerciseQuery
                                        .Select(e=> new ExerciseDTO
                                       {
                                           ExerciseID = e.ExerciseID,
                                           Name = e.Name,
                                           Description = e.Description,
                                           Difficulty = e.Difficulty,
                                           VideoUrl = e.VideoUrl,
                                           Category = e.Category,
                                           Grip = e.Grip,
                                       }));
            var exercises = await PagedList<ExerciseDTO>.CreateAsync(exercisesResponse, page, pageSize);
            return exercises;
        }
    }
}
