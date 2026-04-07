using Gym_App.Domain;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.DTOs.Exercise;
using Gym_App.Infastructure.DTOs.WorkoutDTOs;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infastructure.Transfer_Classes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Gym_App.Application.Services  
{
    public class WorkoutService : IWorkoutService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthorizationService _authorizationService;

        public WorkoutService(IUnitOfWork unitOfWork, IAuthorizationService authorizationService)
        {
            _unitOfWork = unitOfWork;
            _authorizationService = authorizationService;
        }

        //        *********** Setters ***********

        public async Task<SettersResponse> CreateWorkout(ClaimsPrincipal User, WorkoutCreationDTO workout)
        {
            //Checking the validity of the DTO
            if (workout == null || string.IsNullOrEmpty(workout.Name) || workout.UserID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid workout data" };

            //Searching for the User
            var isUserExist = await _unitOfWork.Users.GetById(workout.UserID);
            if (isUserExist == null)
                return new SettersResponse { status = 0, msg = "User not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, isUserExist.Id, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Forbidden from access" };

            //Creating the new Workout
            var newWorkout = new Workout
            {
                Id = Guid.NewGuid(),
                Name = workout.Name,
                Description = workout.Description,
                Date = workout.Date,
                Difficulty = workout.Difficulty,
                Day = workout.Day,
                CreatedAt = DateTime.Now,
                User = isUserExist
            };

            //Saving to Database via repository
            await _unitOfWork.Workouts.Create(newWorkout);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Workout created successfully" };
        }

        public async Task<SettersResponse> UpdateWorkout(ClaimsPrincipal User, Guid workoutID, WorkoutUpdateDTO workout)
        {
            //Checking the validity of the DTO
            if (workout == null || workoutID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid workout data" };

            //Searching for the Workout
            var WorkoutToBeUpdated = await _unitOfWork.Workouts.GetWorkoutById(workoutID);
            if (WorkoutToBeUpdated == null)
                return new SettersResponse { status = 0, msg = "Workout not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, WorkoutToBeUpdated.User.Id, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Forbidden from access" };

            //Updating the Workout
            if (!string.IsNullOrEmpty(workout.Name))
                WorkoutToBeUpdated.Name = workout.Name;

            if (!string.IsNullOrEmpty(workout.Description))
                WorkoutToBeUpdated.Description = workout.Description;

            if (workout.Date != default)
                WorkoutToBeUpdated.Date = workout.Date;

            if (!string.IsNullOrEmpty(workout.Difficulty))
                WorkoutToBeUpdated.Difficulty = workout.Difficulty;

            if (!string.IsNullOrEmpty(workout.Day))
                WorkoutToBeUpdated.Day = workout.Day;

            //Saving to Database via repository
            await _unitOfWork.Workouts.Update(WorkoutToBeUpdated);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Workout updated successfully" };
        }

        public async Task<SettersResponse> DeleteWorkout(ClaimsPrincipal User, Guid workoutID)
        {
            //Checking the workoutID
            if (workoutID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid workout ID" };

            //Searching for the Workout
            var isWorkoutExist = await _unitOfWork.Workouts.GetWorkoutById(workoutID);
            if (isWorkoutExist == null)
                return new SettersResponse { status = 0, msg = "Workout not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, isWorkoutExist.User.Id, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Forbidden from access" };

            //Deleting from Database via repository
            await _unitOfWork.Workouts.Delete(workoutID);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Workout deleted successfully" };
        }

        public async Task<SettersResponse> AddExercisesToWorkout(ClaimsPrincipal User, Guid workoutID, WorkoutExerciseDTO workoutExercises)
        {
            //Checking the validity of the DTO
            if (workoutExercises == null || workoutID == Guid.Empty || workoutExercises.ExercisesID == null)
                return new SettersResponse { status = 0, msg = "Invalid data" };

            //Searching for the Workout
            var workout = await _unitOfWork.Workouts.GetWorkoutById(workoutID);
            if (workout == null)
                return new SettersResponse { status = 0, msg = "Workout not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, workout.User.Id, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Forbidden from access" };

            //Determining if there are new exercises to add
            var existingExerciseIds = new HashSet<Guid>(workout.Exercises!.Select(e => e.Id));
            var exerciseIdsToAdd = workoutExercises.ExercisesID?.Where(id => !existingExerciseIds.Contains(id)).ToList();

            if (exerciseIdsToAdd == null || !exerciseIdsToAdd.Any())
                return new SettersResponse { status = 0, msg = "No new exercises to add" };

            //Getting the exercises to add from the repository
            var exercisesToAdd = await _unitOfWork.Exercises.GetExercisesByIds(exerciseIdsToAdd);
            if (exercisesToAdd == null || !exercisesToAdd.Any())
                return new SettersResponse { status = 0, msg = "No new exercises found" };

            //Saving the new exercises to the workout
            foreach (var exercise in exercisesToAdd)
            {
                workout.Exercises!.Add(exercise);
            }

            //Saving to Database via repository
            await _unitOfWork.Workouts.Update(workout);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Exercises added successfully" };
        }

        public async Task<SettersResponse> SetExercisesOfWorkout(ClaimsPrincipal User, Guid workoutID, WorkoutExerciseDTO workoutExercises)
        {
            //Checking the validity of the DTO
            if (workoutExercises == null || workoutID == Guid.Empty || workoutExercises.ExercisesID == null)
                return new SettersResponse { status = 0, msg = "Invalid data" };

            //Searching for the Workout
            var isWorkoutExist = await _unitOfWork.Workouts.GetWorkoutById(workoutID);
            if (isWorkoutExist == null)
                return new SettersResponse { status = 0, msg = "Workout not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, isWorkoutExist.User.Id, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Forbidden from access" };

            //Clearing existing exercises to add new ones
            isWorkoutExist.Exercises!.Clear();

            //Getting the exercises to add from the repository
            var ExercisesToAdd = await _unitOfWork.Exercises!.GetExercisesByIds(workoutExercises.ExercisesID);
            if (ExercisesToAdd == null || !ExercisesToAdd.Any())
                return new SettersResponse { status = 0, msg = "No exercises found" };

            //saving the new exercises to the workout
            foreach (var exercise in ExercisesToAdd)
            {
                isWorkoutExist.Exercises.Add(exercise);
            }

            //Saving to Database via repository
            await _unitOfWork.Workouts.Update(isWorkoutExist);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Exercises set successfully" };
        }

        public async Task<SettersResponse> DeleteExercisesFromWorkout(ClaimsPrincipal User, Guid workoutID, WorkoutExerciseDTO workoutExercises)
        {
            //checking the Validity of the DTO
            if (workoutExercises == null || workoutID == Guid.Empty || workoutExercises.ExercisesID == null || !workoutExercises.ExercisesID.Any())
                return new SettersResponse { status = 0, msg = "Invalid data" };

            //Searching for the Workout
            var isWorkoutExist = await _unitOfWork.Workouts.GetWorkoutById(workoutID);
            if (isWorkoutExist == null)
                return new SettersResponse { status = 0, msg = "Workout not found" };

            //Authentication
            var authResult = await _authorizationService.AuthorizeAsync(User, isWorkoutExist.User.Id, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Forbidden from access" };

            //Determining if there are exercises to Delete
            var existingExerciseIDs = new HashSet<Guid>(isWorkoutExist.Exercises.Select(i => i.Id));
            var exerciseIDsToRemove = workoutExercises.ExercisesID?.Where(id => existingExerciseIDs.Contains(id)).ToList();
            if (exerciseIDsToRemove == null || !exerciseIDsToRemove.Any())
                return new SettersResponse { status = 0, msg = "No exercises to remove" };

            //Getting the exercises to delete from the Repository
            var ExercisesToRemove = await _unitOfWork.Exercises.GetExercisesByIds(exerciseIDsToRemove);

            //Removing from the Workout
            foreach (var exercise in ExercisesToRemove)
            {
                isWorkoutExist.Exercises.Remove(exercise);
            }

            //Saving to Database via repository
            await _unitOfWork.Workouts.Update(isWorkoutExist);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Exercises removed successfully" };
        }

        //-----------------------------------------------------------------------

        //        *********** Getters ***********

        public async Task<Guid> GetWorkoutId(Guid workoutID)
        {
            var workout = await _unitOfWork.Workouts.GetWorkoutById(workoutID);
            return workout?.User.Id ?? Guid.Empty;
        }

        public async Task<GettersResponse<WorkoutViewDTO>> GetWorkoutByName(string name)
        {
            //Getting workout by name from repository
            var allWorkouts = _unitOfWork.Workouts.GetAll();
            var workout = await allWorkouts
                .Where(w => w.Name == name)
                .Select(w => new WorkoutViewDTO
                {
                    WorkoutID = w.Id,
                    Name = w.Name,
                    Description = w.Description,
                    Date = w.Date,
                    Difficulty = w.Difficulty,
                    Day = w.Day,
                })
                .FirstOrDefaultAsync();

            if (workout == null)
                return new GettersResponse<WorkoutViewDTO>
                {
                    status = 0,
                    msg = "Not Found"
                };
            else
                return new GettersResponse<WorkoutViewDTO>
                {
                    status = 2,
                    msg = "Successful",
                    Value = workout
                };
        }

        public async Task<GettersResponse<WorkoutViewDTO>> GetWorkoutByID(Guid ID)
        {
            //Getting the Workout by ID from repository
            var workout = await _unitOfWork.Workouts.GetWorkoutById(ID);

            if (workout == null)
                return new GettersResponse<WorkoutViewDTO>
                {
                    status = 0,
                    msg = "Not Found"
                };

            var workoutDTO = new WorkoutViewDTO
            {
                UserID = workout.User.Id,
                WorkoutID = workout.Id,
                Name = workout.Name,
                Description = workout.Description,
                Date = workout.Date,
                Difficulty = workout.Difficulty,
                Day = workout.Day,
            };

            return new GettersResponse<WorkoutViewDTO>
            {
                status = 2,
                msg = "Successful",
                Value = workoutDTO
            };
        }

        public async Task<GettersResponse<ExerciseViewDTO>> GetExercisesOfWorkout(Guid WorkoutID, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize)
        {
            //Getting the workout and its exercises from repository
            var workout = await _unitOfWork.Workouts.GetWorkoutById(WorkoutID);

            if (workout == null || workout.Exercises == null || !workout.Exercises.Any())
                return new GettersResponse<ExerciseViewDTO>
                {
                    status = 0,
                    msg = "No Exercises in given workout"
                };

            // Start with exercises as an enumerable for in-memory operations
            var exercises = workout.Exercises.AsEnumerable();

            //If the searchTerm is not null, filter by name, description, or difficulty
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                exercises = exercises.Where(e =>
                    e.Name.ToLower().Contains(searchTerm) ||
                    e.Description?.ToLower().Contains(searchTerm) == true ||
                    e.Category?.ToLower().Contains(searchTerm) == true ||
                    e.Difficulty?.ToLower().Contains(searchTerm) == true);
            }

            //If the sortColumn is not null, sort the data
            if (!string.IsNullOrEmpty(sortColumn))
            {
                var orderLower = (OrderBy ?? string.Empty).ToLowerInvariant();
                bool descending = orderLower == "desc" || orderLower == "descending" || orderLower == "descend" || orderLower == "d";
                
                exercises = sortColumn.ToLower() switch
                {
                    "name" or "n" => descending 
                        ? exercises.OrderByDescending(e => e.Name)
                        : exercises.OrderBy(e => e.Name),
                    "difficulty" or "dif" => descending
                        ? exercises.OrderByDescending(e => e.Difficulty)
                        : exercises.OrderBy(e => e.Difficulty),
                    "description" or "desc" => descending
                        ? exercises.OrderByDescending(e => e.Description)
                        : exercises.OrderBy(e => e.Description),
                    "category" or "cat" => descending
                        ? exercises.OrderByDescending(e => e.Category)
                        : exercises.OrderBy(e => e.Category),
                    "date" or "createdat" or "created" => descending
                        ? exercises.OrderByDescending(e => e.CreatedAt)
                        : exercises.OrderBy(e => e.CreatedAt),
                    _ => exercises
                };
            }

            //Projecting the resultant exercises to ExerciseViewDTO
            var allExercises = exercises
                .Select(e => new ExerciseViewDTO
                {
                    ExerciseID = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    Difficulty = e.Difficulty,
                    Grip = e.Grip,
                    Category = e.Category,
                    VideoUrl = e.VideoUrl,
                })
                .ToList(); // Materialize to list

            //Manual paging for in-memory data
            int totalCount = allExercises.Count;
            var pagedItems = allExercises
                .Skip(pageSize * (page - 1))
                .Take(pageSize)
                .ToList();

            var pagedExercises = new PagedList<ExerciseViewDTO>(pagedItems, page, pageSize, totalCount);
            
            return new GettersResponse<ExerciseViewDTO>
            {
                status = 2,
                msg = "Successful",
                Data = pagedExercises
            };
        }

        public async Task<GettersResponse<WorkoutViewDTO>> GetAllWorkouts(int page, int pageSize)
        {
            //Getting all workouts from repository
            var workoutsQuery = _unitOfWork.Workouts.GetAll()
                .Select(w => new WorkoutViewDTO
                {
                    UserID = w.User.Id,
                    WorkoutID = w.Id,
                    Name = w.Name,
                    Description = w.Description,
                    Date = w.Date,
                    Difficulty = w.Difficulty,
                    Day = w.Day,
                });

            if (workoutsQuery == null || !workoutsQuery.Any())
                return new GettersResponse<WorkoutViewDTO>
                {
                    status = 0,
                    msg = "No workouts in Database"
                };

            //turning the result into a paged list
            var workouts = await PagedList<WorkoutViewDTO>.CreateAsync(workoutsQuery, page, pageSize);
            return new GettersResponse<WorkoutViewDTO>
            {
                status = 2,
                msg = "Successful",
                Data = workouts
            };
        }

        public Task<Guid> GetWorkoutUserID(Guid WorkoutID)
        {
            throw new NotImplementedException();
        }
    }
}
