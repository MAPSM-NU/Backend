using Gym_App.Domain;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.DTOs.Exercise;
using Gym_App.Infastructure.DTOs.WorkoutDTOs;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infastructure.Transfer_Classes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Gym_App.Application.Services
{
    public class WorkoutService : IWorkoutService
    {
        private readonly IWorkoutRepositry _workoutRepositry;
        private readonly IExerciseRepositry _exerciseRepositry;                                         
        private readonly IUserRepositry _userRepositry;
        private readonly IAuthorizationService _authorizationService;

        public WorkoutService(IWorkoutRepositry workoutRepositry, IExerciseRepositry exerciseRepositry, IUserRepositry userRepositry, IAuthorizationService authorizationService)
        {
            _workoutRepositry = workoutRepositry;
            _exerciseRepositry = exerciseRepositry;
            _userRepositry = userRepositry;
            _authorizationService = authorizationService;
        }

        //        *********** Setters ***********

        public async Task<SettersResponse> CreateWorkout(ClaimsPrincipal User, WorkoutCreationDTO workout)
        {
            //Checking the validity of the DTO
            if (workout == null || string.IsNullOrEmpty(workout.Name) || workout.UserID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid workout data" };

            //Searching for the User
            var isUserExist = await _userRepositry.GetById(workout.UserID);
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
            await _workoutRepositry.Create(newWorkout);
            return new SettersResponse { status = 2, msg = "Workout created successfully" };
        }

        public async Task<SettersResponse> UpdateWorkout(ClaimsPrincipal User, Guid workoutID, WorkoutUpdateDTO workout)
        {
            //Checking the validity of the DTO
            if (workout == null || workoutID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid workout data" };

            //Searching for the Workout
            var WorkoutToBeUpdated = await _workoutRepositry.GetWorkoutById(workoutID);
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
            await _workoutRepositry.Update(WorkoutToBeUpdated);
            return new SettersResponse { status = 2, msg = "Workout updated successfully" };
        }

        public async Task<SettersResponse> DeleteWorkout(ClaimsPrincipal User, Guid workoutID)
        {
            //Checking the workoutID
            if (workoutID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid workout ID" };

            //Searching for the Workout
            var isWorkoutExist = await _workoutRepositry.GetWorkoutById(workoutID);
            if (isWorkoutExist == null)
                return new SettersResponse { status = 0, msg = "Workout not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, isWorkoutExist.User.Id, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Forbidden from access" };

            //Deleting from Database via repository
            await _workoutRepositry.Delete(workoutID);
            return new SettersResponse { status = 2, msg = "Workout deleted successfully" };
        }

        public async Task<SettersResponse> AddExercisesToWorkout(ClaimsPrincipal User, Guid workoutID, WorkoutExerciseDTO workoutExercises)
        {
            //Checking the validity of the DTO
            if (workoutExercises == null || workoutID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid workout ID" };

            //Searching for the Workout
            var workout = await _workoutRepositry.GetWorkoutById(workoutID);
            if (workout == null)
                return new SettersResponse { status = 0, msg = "Workout not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, workout.User.Id, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Invalid workout data" };

            //Determining if there are new exercises to add
            var existingExerciseIds = new HashSet<Guid>(workout.Exercises!.Select(e => e.Id));
            var exerciseIdsToAdd = workoutExercises.ExercisesID?.Where(id => !existingExerciseIds.Contains(id)).ToList();

            if (exerciseIdsToAdd == null || !exerciseIdsToAdd.Any())
                return new SettersResponse { status = 0, msg = "No new exercises to add" };

            //Getting the exercises to add from the repository
            var exercisesToAdd = await _exerciseRepositry.GetExercisesByIds(exerciseIdsToAdd);
            if (exercisesToAdd == null || !exercisesToAdd.Any())
                return new SettersResponse { status = 0, msg = "No new exercises found" };

            //Saving the new exercises to the workout
            foreach (var exercise in exercisesToAdd)
            {
                workout.Exercises!.Add(exercise);
            }

            //Saving to Database via repository
            await _workoutRepositry.Update(workout);
            return new SettersResponse { status = 2, msg = "Exercises added successfully" };
        }

        public async Task<SettersResponse> SetExercisesOfWorkout(ClaimsPrincipal User, Guid workoutID, WorkoutExerciseDTO workoutExercises)
        {
            //Checking the validity of the DTO
            if (workoutExercises == null || workoutID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid workout ID" };

            //Searching for the Workout
            var isWorkoutExist = await _workoutRepositry.GetWorkoutById(workoutID);
            if (isWorkoutExist == null)
                return new SettersResponse { status = 0, msg = "Workout not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, isWorkoutExist.User.Id, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Invalid workout data" };

            //Clearing existing exercises to add new ones
            isWorkoutExist.Exercises!.Clear();

            //Determining if there are new exercises to add
            var exerciseIDsToAdd = workoutExercises.ExercisesID.ToList();
            if (exerciseIDsToAdd == null || exerciseIDsToAdd.Count == 0)
                return new SettersResponse { status = 0, msg = "No new exercises to add" };

            //Getting the exercises to add from the repository
            var ExercisesToAdd = await _exerciseRepositry.GetExercisesByIds(exerciseIDsToAdd);
            if (ExercisesToAdd == null || !ExercisesToAdd.Any())
                return new SettersResponse { status = 0, msg = "No new exercises found" };

            //saving the new exercises to the workout
            foreach (var exercise in ExercisesToAdd)
            {
                isWorkoutExist.Exercises.Add(exercise);
            }

            //Saving to Database via repository
            await _workoutRepositry.Update(isWorkoutExist);
            return new SettersResponse { status = 2, msg = "Exercises added successfully" };
        }

        public async Task<SettersResponse> DeleteExercisesFromWorkout(ClaimsPrincipal User, Guid workoutID, WorkoutExerciseDTO workoutExercises)
        {
            //checking the Validity of the DTO
            if (workoutExercises == null || workoutID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid workout ID" };

            //Searching for the Workout
            var isWorkoutExist = await _workoutRepositry.GetWorkoutById(workoutID);
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
            var ExercisesToRemove = await _exerciseRepositry.GetExercisesByIds(exerciseIDsToRemove);
            if (ExercisesToRemove == null || !ExercisesToRemove.Any())
                return new SettersResponse { status = 0, msg = "No exercises found" };

            //Removing from the Workout
            foreach (var exercise in ExercisesToRemove)
            {
                isWorkoutExist.Exercises.Remove(exercise);
            }

            //Saving to Database via repository
            await _workoutRepositry.Update(isWorkoutExist);
            return new SettersResponse { status = 2, msg = "Exercises removed successfully" };
        }

        //-----------------------------------------------------------------------

        //        *********** Getters ***********

        public async Task<Guid> GetWorkoutId(Guid workoutID)
        {
            var workout = await _workoutRepositry.GetWorkoutById(workoutID);
            return workout?.User.Id ?? Guid.Empty;
        }

        public async Task<GettersResponse<WorkoutViewDTO>> GetWorkoutByName(string name)
        {
            //Getting workout by name from repository
            var allWorkouts = _workoutRepositry.GetAll();
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
            var workout = await _workoutRepositry.GetWorkoutById(ID);

            if (workout == null)
                return new GettersResponse<WorkoutViewDTO>
                {
                    status = 0,
                    msg = "Not Found"
                };

            var workoutDTO = new WorkoutViewDTO
            {
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
            var workout = await _workoutRepositry.GetWorkoutById(WorkoutID);

            if (workout == null || workout.Exercises == null || !workout.Exercises.Any())
                return new GettersResponse<ExerciseViewDTO>
                {
                    status = 0,
                    msg = "No Exercises in given workout"
                };

            var exercisesQuery = workout.Exercises.AsQueryable();

            //If the searchTerm is not null, filter by name, description, or difficulty
            if (!string.IsNullOrEmpty(searchTerm))
                exercisesQuery = _exerciseRepositry.Search(searchTerm,exercisesQuery);

            //If the sortColumn is not null, sort the data
            if (!string.IsNullOrEmpty(sortColumn))
                exercisesQuery = _exerciseRepositry.FilterSortColumn(sortColumn, OrderBy, exercisesQuery);

            //Projecting the resultant exercise queries as exerciseDTO
            var exerciseResult = exercisesQuery
                .Select(e => new ExerciseViewDTO
                {
                    ExerciseID = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    Difficulty = e.Difficulty,
                    Grip = e.Grip,
                    Category = e.Category,
                    VideoUrl = e.VideoUrl,
                });

            //Making the result as a paged list
            var exercises = await PagedList<ExerciseViewDTO>.CreateAsync(exerciseResult, page, pageSize);
            return new GettersResponse<ExerciseViewDTO>
            {
                status = 2,
                msg = "Successful",
                Data = exercises
            };
        }

        public async Task<GettersResponse<WorkoutViewDTO>> GetAllWorkouts(int page, int pageSize)
        {
            //Getting all workouts from repository
            var workoutsQuery = _workoutRepositry.GetAll()
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
