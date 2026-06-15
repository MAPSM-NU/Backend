using Gym_App.Domain;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.DTOs.Exercise;
using Gym_App.Infastructure.DTOs.Muscle;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infastructure.Transfer_Classes;
using Microsoft.EntityFrameworkCore;

namespace Gym_App.Application.Services
{
    public class ExerciseService : IExerciseService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExerciseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //        *********** Setters ***********

        //0 == Error(Bad Request) || 1 == Unauthorized (Forbid) || 2 == Success (Ok)

        public async Task<SettersResponse> CreateExercise(ExerciseCreationDTO exercise)
        {
            if (exercise == null || string.IsNullOrWhiteSpace(exercise.Name)) 
                return new SettersResponse { status = 0, msg = "Invalid exercise data" };

            if (await isExerciseExist(exercise.Name))
                return new SettersResponse { status = 0, msg = "Exercise already exists" };

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

            await _unitOfWork.Exercises.Create(newExercise);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Exercise created successfully" };
        }
        public async Task<SettersResponse> UpdateExercise(Guid Id, ExerciseCreationDTO exercise)
        {
            if (exercise == null || Id == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid exercise data" };

            var toBeUpdated = await (from E in _unitOfWork.Exercises.GetAll()
                               where E.Id == Id
                               select E).FirstOrDefaultAsync();
            
            if (toBeUpdated == null)
                return new SettersResponse { status = 0, msg = "Exercise not found" };

            if (!string.IsNullOrEmpty(exercise.Category))
                toBeUpdated.Category = exercise.Category;
            
            if (!string.IsNullOrEmpty(exercise.Difficulty))
                toBeUpdated.Difficulty = exercise.Difficulty;
            
            if (!string.IsNullOrEmpty(exercise.Name))
            {
                if (await isExerciseExist(exercise.Name))
                    return new SettersResponse { status = 0, msg = "Exercise with this name already exists" };
                else 
                    toBeUpdated.Name = exercise.Name;
            }
            
            if (!string.IsNullOrEmpty(exercise.Description))
                toBeUpdated.Description = exercise.Description;
            
            if (!string.IsNullOrEmpty(exercise.VideoUrl))
                toBeUpdated.VideoUrl = exercise.VideoUrl;

            await _unitOfWork.Exercises.Update(toBeUpdated);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Exercise updated successfully" };
        }
        public async Task<SettersResponse> DeleteExercise(Guid ExerciseID)
        {
            var isExerciseExists = await (from E in _unitOfWork.Exercises.GetAll()
                                    where E.Id == ExerciseID
                                    select E).FirstOrDefaultAsync();
            if (isExerciseExists == null)
                return new SettersResponse { status = 0, msg = "Exercise not found" };

            await _unitOfWork.Exercises.Delete(isExerciseExists);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Exercise deleted successfully" };
        }
        public async Task<SettersResponse> AddMusclesToExercise(Guid exerciseID, ExerciseMusclesDTO exerciseMuscles)
        {
            if (exerciseMuscles == null || exerciseMuscles.Muscles == null || exerciseMuscles.Muscles.Count == 0) 
                return new SettersResponse { status = 0, msg = "Invalid DTO" };

            var exercise = await (from e in _unitOfWork.Exercises.GetAll().Include(e => e.Muscles)
                            where e.Id == exerciseID       
                            select e).FirstOrDefaultAsync();

            if (exercise == null) 
                return new SettersResponse { status = 0, msg = "Exercise not found" };

            var existingMuscleIDs = new HashSet<Guid>(exercise.Muscles!.Select(m => m.Id));
            var musclesIDsToAdd = exerciseMuscles.Muscles?.Where(id => !existingMuscleIDs.Contains(id)).ToList();
            if (musclesIDsToAdd == null || musclesIDsToAdd.Count == 0)
                return new SettersResponse { status = 0, msg = "No new muscles to add" };

            var musclesToAdd = await (from m in _unitOfWork.Muscles.GetAll()
                                      where musclesIDsToAdd.Contains(m.Id)
                                      select m).ToListAsync();
            if (musclesToAdd.Count == 0) 
                return new SettersResponse { status = 0, msg = "Muscles to add not found" };

            foreach (var muscle in musclesToAdd)
            {
                exercise.Muscles!.Add(muscle);
            }

            await _unitOfWork.Exercises.Update(exercise);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Muscles added successfully" };
        }
        public async Task<SettersResponse> RemoveMusclesFromExercise(Guid exerciseID, ExerciseMusclesDTO exerciseMuscles)
        {
            if (exerciseMuscles == null || exerciseMuscles.Muscles == null || exerciseMuscles.Muscles.Count == 0)
                return new SettersResponse { status = 0, msg = "Invalid DTO" };

            var exercise = await (from e in _unitOfWork.Exercises.GetAll().Include(e => e.Muscles)
                                 where e.Id == exerciseID       
                            select e).FirstOrDefaultAsync();

            if (exercise == null) 
                return new SettersResponse { status = 0, msg = "Exercise not found" };

            var existingMuscleIDs = new HashSet<Guid>(exercise.Muscles.Select(m => m.Id));
            var musclesIDsToRemove = exerciseMuscles.Muscles?.Where(id => existingMuscleIDs.Contains(id)).ToList();
            if (musclesIDsToRemove == null || musclesIDsToRemove.Count == 0)
                return new SettersResponse { status = 0, msg = "No muscles to remove" };

            var musclesToRemove = await (from m in _unitOfWork.Muscles.GetAll()
                               where musclesIDsToRemove.Contains(m.Id)
                               select m).ToListAsync();

            if (musclesToRemove.Count == 0)
                return new SettersResponse { status = 0, msg = "Muscles to remove not found" };

            foreach (var muscle in musclesToRemove)
            {
                exercise.Muscles.Remove(muscle);
            }

            await _unitOfWork.Exercises.Update(exercise);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Muscles removed successfully" };
        }

        //-----------------------------------------------------------------------


        //        *********** Extra Validation Function ***********

        public async Task<bool> isExerciseExist(string name)
        {
            var isExerciseExists = await _unitOfWork.Exercises.isExerciseNameExist(name);
            return isExerciseExists;
        }
        
        //-----------------------------------------------------------------------

        //        *********** Getters ***********
        public async Task<GettersResponse<ExerciseViewDTO>> GetExerciseByName(string name)
        {
            var Exercise = await (from e in _unitOfWork.Exercises.GetAll()
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
                                Muscles = e.Muscles.Select(m => m.Name),
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
            var Exercise = await (from e in _unitOfWork.Exercises.GetAll()
                                  where e.Id == id
                            select new ExerciseMiniViewDTO
                            {
                                Name = e.Name,
                                Description = e.Description ?? "",
                                Difficulty = e.Difficulty ?? "",
                                VideoUrl = e.VideoUrl ?? "",
                                Category = e.Category ?? "",
                                Grip = e.Grip ?? "",
                                Muscles = e.Muscles!.Select(m => m.Name),
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
            var exercise = await (from e in _unitOfWork.Exercises.GetAll().Include(e => e.Muscles)
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
        public async Task<GettersResponse<ExerciseViewDTO>> GetExercisesByMuscle(ExerciseListDTO muscles, int page, int pageSize)
        {
            var muscleNames = muscles.Muscles;
            var exercisequery = from e in _unitOfWork.Exercises.GetAll().Include(e => e.Muscles)
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
                                    Muscles = e.Muscles.Select(m => m.Name),
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
            IQueryable<Exercise> exerciseQuery = _unitOfWork.Exercises.GetAll();

            if (!string.IsNullOrEmpty(searchTerm))
                exerciseQuery = _unitOfWork.Exercises.Search(searchTerm, exerciseQuery);

            if (exerciseQuery.Count() == 0)
                return new GettersResponse<ExerciseViewDTO>
                {
                    status = 0,
                    msg = "No Exercise found with specified conditions"
                };

            if (!string.IsNullOrEmpty(sortColumn)) 
                exerciseQuery = _unitOfWork.Exercises.FilterSortColumn(sortColumn, OrderBy, exerciseQuery);

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
                                            Muscles = e.Muscles.Select(m => m.Name),
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
            IQueryable<Exercise> exerciseQuery = _unitOfWork.Exercises.GetAll();
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
                                            Muscles = e.Muscles.Select(m => m.Name),
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
