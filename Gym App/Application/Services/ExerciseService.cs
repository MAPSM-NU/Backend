
using DocumentFormat.OpenXml.Wordprocessing;
using Gym_App.Domain;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.DTOs.Exercise;
using Gym_App.Infastructure.DTOs.Muscle;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infastructure.Transfer_Classes;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace Gym_App.Application.Services
{
    public class ExerciseService : IExerciseService
    {
        private readonly IExerciseRepositry _exerciseRepositry;
        private readonly IMuscleRepositry _muscleRepositry;
        public ExerciseService(IExerciseRepositry exerciseRepositry,IMuscleRepositry muscleRepositry)
        {
            _exerciseRepositry = exerciseRepositry;
            _muscleRepositry = muscleRepositry;
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
                Id = Guid.NewGuid(),
                Name = exercise.Name,
                Description = exercise.Description,
                Difficulty = exercise.Difficulty,
                VideoUrl = exercise.VideoUrl,
                Category = exercise.Category,
                Grip = exercise.Grip
            };
            await _exerciseRepositry.Create(newExercise);
            return new SettersResponse { status = 2, msg = "Exercise created successfully" };
        }
        public async Task<SettersResponse> UpdateExercise(Guid Id, ExerciseCreationDTO exercise)
        {
            if(exercise == null || Id == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid exercise data" };

            var toBeUpdated = await (from E in _exerciseRepositry.GetAll()
                               where E.Id == Id
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

            await _exerciseRepositry.Update(toBeUpdated);
            return new SettersResponse { status = 2, msg = "Exercise updated successfully" };
        }
        public async Task<SettersResponse> DeleteExercise(Guid ExerciseID)
        {
            
            var isExerciseExists = await (from E in _exerciseRepositry.GetAll()
                                    where E.Id == ExerciseID
                                    select E).FirstOrDefaultAsync();
            if (isExerciseExists == null)
                return new SettersResponse { status = 0, msg = "Exercise not found" };
            await _exerciseRepositry.Create(isExerciseExists);
            return new SettersResponse { status = 2, msg = "Exercise deleted successfully" };
        }
        public async Task<SettersResponse> AddMusclesToExercise(Guid exerciseID, ExerciseMusclesDTO exerciseMuscles)
        {
            if(exerciseMuscles == null || exerciseMuscles.Muscles == null || exerciseMuscles.Muscles.Count == 0) 
                return new SettersResponse { status = 0, msg = "Invalid DTO"};
            var exercise = await(from e in _exerciseRepositry.GetAll().Include(e=>e.Muscles)
                            where e.Id == exerciseID       
                            select e).FirstOrDefaultAsync();

            if (exercise == null) 
                return new SettersResponse { status = 0, msg = "Exercise not found" };
            var existingMuscleIDs = new HashSet<Guid>(exercise.Muscles!.Select(m => m.Id));
            var musclesIDsToAdd = exerciseMuscles.Muscles?.Where(id => !existingMuscleIDs.Contains(id)).ToList();
            if (musclesIDsToAdd == null || musclesIDsToAdd.Count == 0)
                return new SettersResponse { status = 0, msg = "No new muscles to add" };

            var musclesToAdd = await (from m in _muscleRepositry.GetAll()
                                      where musclesIDsToAdd.Contains(m.Id)
                                      select m).ToListAsync();
            if (musclesToAdd.Count == 0) 
                return new SettersResponse { status = 0, msg = "Muscles to add not found" };
            foreach (var muscle in musclesToAdd)
            {
                exercise.Muscles!.Add(muscle);
            }
            await _exerciseRepositry.Update(exercise);  
            return new SettersResponse { status = 2, msg = "Muscles added successfully" };
        }
        public async Task<SettersResponse> RemoveMusclesFromExercise(Guid exerciseID, ExerciseMusclesDTO exerciseMuscles)
        {
            if(exerciseMuscles == null || exerciseMuscles.Muscles == null || exerciseMuscles.Muscles.Count == 0)
                return new SettersResponse { status = 0,msg = "Invalid DTO"};
            var exercise = await(from e in _exerciseRepositry.GetAll().Include(e=>e.Muscles)
                                 where e.Id == exerciseID       
                            select e).FirstOrDefaultAsync();

            if (exercise == null) 
                return new SettersResponse { status = 0, msg = "Exercise not found" };

            var existingMuscleIDs = new HashSet<Guid>(exercise.Muscles.Select(m => m.Id));
            var musclesIDsToRemove = exerciseMuscles.Muscles?.Where(id => existingMuscleIDs.Contains(id)).ToList();
            if (musclesIDsToRemove == null || musclesIDsToRemove.Count == 0)
                return new SettersResponse { status = 0, msg = "No muscles to remove" };

            var musclesToRemove = await (from m in _muscleRepositry.GetAll()
                               where musclesIDsToRemove.Contains(m.Id)
                               select m).ToListAsync();

            if (musclesToRemove.Count == 0)
                return new SettersResponse { status = 0, msg = "Muscles to remove not found" };
            foreach (var muscle in musclesToRemove)
            {
                exercise.Muscles.Remove(muscle);
            }
            await _exerciseRepositry.Update(exercise); 
            return new SettersResponse { status = 2, msg = "Muscles removed successfully" };
        }

        //-----------------------------------------------------------------------


        //        *********** Extra Validation Function ***********

        public async Task<bool> isExerciseExist(string name)
        {
            var isExerciseExists = await _exerciseRepositry.isExerciseNameExist(name);
            return isExerciseExists;
        }
        
        //-----------------------------------------------------------------------

        //        *********** Getters ***********
        public async Task<GettersResponse<ExerciseViewDTO>> GetExerciseByName(string name)
        {
            var Exercise = await (from e in _exerciseRepositry.GetAll()
                                  where e.Name == name
                            select new ExerciseViewDTO
                            {
                                ExerciseID = e.Id,
                                Name = e.Name,
                                Description = e.Description,
                                Difficulty = e.Difficulty,
                                VideoUrl = e.VideoUrl,
                                Category = e.Category,
                                Grip = e.Grip,
                            }).FirstOrDefaultAsync();
            if (Exercise == null) 
                return new GettersResponse<ExerciseViewDTO>
                {
                    status = 0,
                    msg = "Exercise not found"
                };
            return new GettersResponse<ExerciseViewDTO>
            {
                status = 2,
                msg = "Successful",
                Value = Exercise
            };
        }
        public async Task<GettersResponse<ExerciseMiniViewDTO>> GetExerciseByID(Guid id)
        {
            var Exercise = await (from e in _exerciseRepositry.GetAll()
                                  where e.Id == id
                            select new ExerciseMiniViewDTO
                            {
                                Name = e.Name,
                                Description = e.Description ?? "",
                                Difficulty = e.Difficulty ?? "",
                                VideoUrl = e.VideoUrl ?? "",
                                Category = e.Category ?? "",
                                Grip = e.Grip ?? "",
                            }).FirstOrDefaultAsync();
            if (Exercise == null)
                return new GettersResponse<ExerciseMiniViewDTO>
                {
                    status = 0,
                    msg = "Exercise not found"
                };
            return new GettersResponse<ExerciseMiniViewDTO>
            {
                status = 2,
                msg = "Successful",
                Value = Exercise
            };
        }
        public async Task<GettersResponse<List<MuscleViewDTO>>> GetExerciseMuscles(Guid exerciseID)
        {
            var exercise = await (from e in _exerciseRepositry.GetAll().Include(e => e.Muscles)
                                  where e.Id == exerciseID
                                 select e).FirstOrDefaultAsync();
            if (exercise == null) 
                return new GettersResponse<List<MuscleViewDTO>>
                {
                    status = 0,
                    msg = "Exercise not found"
                };

            var muscleDTOs = (from m in exercise.Muscles
                              select new MuscleViewDTO
                              {
                                MusclesID = m.Id,
                                Name = m.Name,
                                Description = m.Description
                              }).ToList();
            return new GettersResponse<List<MuscleViewDTO>>
            {
                status = 2,
                msg = "Successful",
                Value = muscleDTOs
            };
        }
        public async Task<GettersResponse<ExerciseViewDTO>> GetExercisesByMuscle(ExerciseListDTO muscles,int page,int pageSize)
        {
            var muscleNames = muscles.Muscles;
            var exercisequery = from e in _exerciseRepositry.GetAll().Include(e => e.Muscles)
                                where muscleNames.All(name => e.Muscles!.Any(m => m.Name.Contains(name)))
                                select new ExerciseViewDTO
                                {
                                    ExerciseID = e.Id,
                                    Name = e.Name,
                                    Description = e.Description,
                                    Difficulty = e.Difficulty,
                                    VideoUrl = e.VideoUrl,
                                    Category = e.Category,
                                    Grip = e.Grip,
                                };

            if (exercisequery.Count() == 0)
                return new GettersResponse<ExerciseViewDTO>
                {
                    status = 0,
                    msg = "No Exercise has the specified muscles(s)"
                };

            var exercises = await PagedList<ExerciseViewDTO>.CreateAsync(exercisequery, page, pageSize);
            return new GettersResponse<ExerciseViewDTO>
            {
                status = 2,
                msg = "Successful",
                Data = exercises
            };
        }
        public async Task<GettersResponse<ExerciseViewDTO>> GetExercisesByFilter(int page, string sortColumn, string OrderBy, string searchTerm, int pageSize = 5)
        {
            IQueryable<Exercise> exerciseQuery = _exerciseRepositry.GetAll();

            if(!string.IsNullOrEmpty(searchTerm))exerciseQuery = _exerciseRepositry.Search(searchTerm, exerciseQuery);

            if (exerciseQuery.Count() == 0)
                return new GettersResponse<ExerciseViewDTO>
                {
                    status = 0,
                    msg = "No Exercise found with specified conditions"
                };

            if (!string.IsNullOrEmpty(sortColumn)) exerciseQuery = _exerciseRepositry.FilterSortColumn(sortColumn, OrderBy, exerciseQuery);

            var exercisesResponse = exerciseQuery
                                        .Select(e => new ExerciseViewDTO
                                        {
                                            ExerciseID = e.Id,
                                            Name = e.Name,
                                            Description = e.Description,
                                            Difficulty = e.Difficulty,
                                            VideoUrl = e.VideoUrl,
                                            Category = e.Category,
                                            Grip = e.Grip,
                                        });
            var exercises = await PagedList<ExerciseViewDTO>.CreateAsync(exercisesResponse, page, pageSize);
            return new GettersResponse<ExerciseViewDTO>
            {
                status = 2,
                msg = "Successful",
                Data = exercises
            };
        }
        public async Task<GettersResponse<ExerciseViewDTO>> GetAllExercises(int page, int pageSize = 5)
        {
            IQueryable<Exercise> exerciseQuery = _exerciseRepositry.GetAll();
            var exercisesResponse = exerciseQuery
                                        .Select(e=> new ExerciseViewDTO
                                       {
                                           ExerciseID = e.Id,
                                           Name = e.Name,
                                           Description = e.Description,
                                           Difficulty = e.Difficulty,
                                           VideoUrl = e.VideoUrl,
                                           Category = e.Category,
                                           Grip = e.Grip,
                                       });
            var exercises = await PagedList<ExerciseViewDTO>.CreateAsync(exercisesResponse, page, pageSize);
            return new GettersResponse<ExerciseViewDTO>
            {
                status = 2,
                msg = "Successful",
                Data = exercises
            };
        }
    }
}
