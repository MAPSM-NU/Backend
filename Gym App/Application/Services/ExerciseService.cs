
using DocumentFormat.OpenXml.Wordprocessing;
using Gym_App.Application.Interfaces;
using Gym_App.Domain;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.DTOs.Exercise;
using Gym_App.Infastructure.DTOs.Muscle;
using Gym_App.Infastructure.Transfer_Classes;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace Gym_App.Application.Services
{
    public class ExerciseService : IExerciseService
    {
        private readonly DbBase _db;
        public ExerciseService(DbBase db)
        {
            _db = db;
        }

        //        *********** Setters ***********

        //0 == Error(Bad Request) || 1 == Unauthorized (Forbid) || 2 == Success (Ok)

        public async Task<SettersResponse> CreateExercise(ExerciseCreationDTO exercise)
        {
            if (exercise == null || string.IsNullOrWhiteSpace(exercise.Name)) 
                return new SettersResponse { status = 0 ,msg = "Invalid exercise data" };

            if(await isExerciseExist(exercise.Name))
                return new SettersResponse { status = 0 ,msg = "Exercise already exists" };

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
            return new SettersResponse { status = 2, msg = "Exercise created successfully" };
        }
        public async Task<SettersResponse> UpdateExercise(Guid exerciseID, ExerciseCreationDTO exercise)
        {
            if(exercise == null || exerciseID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid exercise data" };

            var toBeUpdated = await (from E in _db.Exercises
                               where E.ExerciseID == exerciseID
                               select E).FirstOrDefaultAsync();
            
            if (toBeUpdated == null)
                return new SettersResponse { status = 0, msg = "Exercise not found" };

            if(!string.IsNullOrEmpty(exercise.Category))
                toBeUpdated.Category = exercise.Category;
            
            if(!string.IsNullOrEmpty(exercise.Difficulty))
                toBeUpdated.Difficulty = exercise.Difficulty;
            
            if(!string.IsNullOrEmpty(exercise.Name))
            {
                if(await isExerciseExist(exercise.Name))
                    return new SettersResponse { status = 0, msg = "Exercise with this name already exists" };
                else toBeUpdated.Name = exercise.Name;
            }
            
            if(!string.IsNullOrEmpty(exercise.Description))
                toBeUpdated.Description = exercise.Description;
            
            if (!string.IsNullOrEmpty(exercise.VideoUrl))
                toBeUpdated.VideoUrl = exercise.VideoUrl;
            
            _db.Exercises.Update(toBeUpdated);
            await _db.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Exercise updated successfully" };
        }
        public async Task<SettersResponse> DeleteExercise(Guid ExerciseID)
        {
            
            var isExerciseExists = await (from E in _db.Exercises
                                    where E.ExerciseID == ExerciseID
                                    select E).FirstOrDefaultAsync();
            if (isExerciseExists == null)
                return new SettersResponse { status = 0, msg = "Exercise not found" };
            _db.Exercises.Remove(isExerciseExists);
            await _db.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Exercise deleted successfully" };
        }
        public async Task<SettersResponse> AddMusclesToExercise(Guid exerciseID, ExerciseMusclesDTO exerciseMuscles)
        {
            if(exerciseMuscles == null || exerciseMuscles.Muscles == null || exerciseMuscles.Muscles.Count == 0) 
                return new SettersResponse { status = 0, msg = "Invalid DTO"};
            var exercise = await(from e in _db.Exercises.Include(m =>m.Muscles)
                            where e.ExerciseID == exerciseID       
                            select e).FirstOrDefaultAsync();

            if (exercise == null) 
                return new SettersResponse { status = 0, msg = "Exercise not found" };
            var existingMuscleIDs = new HashSet<Guid>(exercise.Muscles.Select(m => m.MusclesID));
            var musclesIDsToAdd = exerciseMuscles.Muscles?.Where(id => !existingMuscleIDs.Contains(id)).ToList();
            if (musclesIDsToAdd == null || musclesIDsToAdd.Count == 0)
                return new SettersResponse { status = 0, msg = "No new muscles to add" };

            var musclesToAdd = await (from m in _db.Muscles
                               where musclesIDsToAdd.Contains(m.MusclesID)
                               select m).ToListAsync();
            if (musclesToAdd.Count == 0) 
                return new SettersResponse { status = 0, msg = "Muscles to add not found" };
            foreach (var muscle in musclesToAdd)
            {
                exercise.Muscles.Add(muscle);
            }
            _db.Exercises.Update(exercise);
            await _db.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Muscles added successfully" };
        }
        public async Task<SettersResponse> RemoveMusclesFromExercise(Guid exerciseID, ExerciseMusclesDTO exerciseMuscles)
        {
            if(exerciseMuscles == null || exerciseMuscles.Muscles == null || exerciseMuscles.Muscles.Count == 0)
                return new SettersResponse { status = 0,msg = "Invalid DTO"};
            var exercise = await(from e in _db.Exercises.Include(m =>m.Muscles)
                            where e.ExerciseID == exerciseID       
                            select e).FirstOrDefaultAsync();

            if (exercise == null) 
                return new SettersResponse { status = 0, msg = "Exercise not found" };

            var existingMuscleIDs = new HashSet<Guid>(exercise.Muscles.Select(m => m.MusclesID));
            var musclesIDsToRemove = exerciseMuscles.Muscles?.Where(id => existingMuscleIDs.Contains(id)).ToList();
            if (musclesIDsToRemove == null || musclesIDsToRemove.Count == 0)
                return new SettersResponse { status = 0, msg = "No muscles to remove" };

            var musclesToRemove = await (from m in _db.Muscles
                               where musclesIDsToRemove.Contains(m.MusclesID)
                               select m).ToListAsync();

            if (musclesToRemove.Count == 0)
                return new SettersResponse { status = 0, msg = "Muscles to remove not found" };
            foreach (var muscle in musclesToRemove)
            {
                exercise.Muscles.Remove(muscle);
            }
            _db.Exercises.Update(exercise);
            await _db.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Muscles removed successfully" };
        }

        //-----------------------------------------------------------------------


        //        *********** Extra Validation Function ***********

        public async Task<bool> isExerciseExist(string name)
        {
            var isExerciseExists = await _db.Exercises.AnyAsync(e => e.Name.ToLower() == name.ToLower());
            return isExerciseExists;
        }
        
        //-----------------------------------------------------------------------

        //        *********** Getters ***********
        public async Task<ExerciseViewDTO?> GetExerciseByName(string name)
        {
            var Exercise = await (from e in _db.Exercises
                            where e.Name == name
                            select new ExerciseViewDTO
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
        public async Task<ExerciseMiniViewDTO?> GetExerciseByID(Guid id)
        {
            var Exercise = await (from e in _db.Exercises
                            where e.ExerciseID == id
                            select new ExerciseMiniViewDTO
                            {
                                Name = e.Name,
                                Description = e.Description ?? "",
                                Difficulty = e.Difficulty ?? "",
                                VideoUrl = e.VideoUrl ?? "",
                                Category = e.Category ?? "",
                                Grip = e.Grip ?? "",
                            }).FirstOrDefaultAsync();
            if (Exercise == null) return Exercise;
            return Exercise;
        }
        public async Task<List<MuscleViewDTO>?> GetExerciseMuscles(Guid exerciseID)
        {
            var exercise = await (from e in _db.Exercises.Include(e=>e.Muscles)
                                 where e.ExerciseID == exerciseID
                                 select e).FirstOrDefaultAsync();
            if (exercise == null) return null;
            var muscleDTOs = (from m in exercise.Muscles
                              select new MuscleViewDTO
                              {
                                MusclesID = m.MusclesID,
                                Name = m.Name,
                                Description = m.Description
                              }).ToList();
            return muscleDTOs;
        }
        public async Task<PagedList<ExerciseViewDTO>>? GetExercisesByMuscle(ExerciseListDTO muscles,int page,int pageSize)
        {
            if (page == 0) page = 1;
            if(pageSize == 0) pageSize = 10;
            var muscleNames = muscles.Muscles;
            var exercisequery = from e in _db.Exercises
                where muscleNames.All(name => e.Muscles.Any(m => m.Name == name))
                select new ExerciseViewDTO
                {
                    ExerciseID = e.ExerciseID,
                    Name = e.Name,
                    Description = e.Description,
                    Difficulty = e.Difficulty,
                    VideoUrl = e.VideoUrl,
                    Category = e.Category,
                    Grip = e.Grip,
                };
            var exercises = await PagedList<ExerciseViewDTO>.CreateAsync(exercisequery, page, pageSize);
            return exercises;
        }
        public async Task<PagedList<ExerciseViewDTO>?> GetExercisesByFilter(int page, string sortColumn, string OrderBy, string searchTerm, int pageSize = 5)
        {
            if (page == 0) page = 1;
            if (pageSize == 0) pageSize = 10;
            IQueryable<Exercise> exerciseQuery = _db.Exercises;

            if(!string.IsNullOrEmpty(searchTerm))exerciseQuery = exerciseQuery.Where(e => e.Name.Contains(searchTerm) || e.Difficulty.Contains(searchTerm));

            if (!string.IsNullOrEmpty(sortColumn))
            {
                Expression<Func<Exercise, object>> keySelector = sortColumn.ToLower() switch // throws error when sortColumn is null
                {
                    "name" => Exercise => Exercise.Name,
                    "difficulty" => Exercise => Exercise.Difficulty,
                    _ => Exercise => Exercise.ExerciseID
                };
                if (!string.IsNullOrEmpty(OrderBy))exerciseQuery = exerciseQuery.OrderBy(keySelector);//If any kind of value is in OrderBy then it is ascending
                else exerciseQuery = exerciseQuery.OrderByDescending(keySelector);
            }
            var exercisesResponse = exerciseQuery
                                        .Select(e => new ExerciseViewDTO
                                        {
                                            ExerciseID = e.ExerciseID,
                                            Name = e.Name,
                                            Description = e.Description,
                                            Difficulty = e.Difficulty,
                                            VideoUrl = e.VideoUrl,
                                            Category = e.Category,
                                            Grip = e.Grip,
                                        });
        var exercises = await PagedList<ExerciseViewDTO>.CreateAsync(exercisesResponse, page, pageSize);
            return exercises;
        }
        public async Task<PagedList<ExerciseViewDTO>?> GetAllExercises(int page, int pageSize = 5)
        {
            IQueryable<Exercise> exerciseQuery = _db.Exercises;

            if (page == 0) page = 1;
            if (pageSize == 0) pageSize = 10;
            var exercisesResponse = exerciseQuery
                                        .Select(e=> new ExerciseViewDTO
                                       {
                                           ExerciseID = e.ExerciseID,
                                           Name = e.Name,
                                           Description = e.Description,
                                           Difficulty = e.Difficulty,
                                           VideoUrl = e.VideoUrl,
                                           Category = e.Category,
                                           Grip = e.Grip,
                                       });
            var exercises = await PagedList<ExerciseViewDTO>.CreateAsync(exercisesResponse, page, pageSize);
            return exercises;
        }
    }
}
