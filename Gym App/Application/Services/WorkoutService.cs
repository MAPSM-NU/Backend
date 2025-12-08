using DocumentFormat.OpenXml.Wordprocessing;
using Gym_App.Application.Interfaces;
using Gym_App.Domain;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.DTOs;
using Gym_App.Infastructure.DTOs.WorkoutDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Gym_App.Application.Services
{
    public class WorkoutService : IWorkoutService
    {
        private readonly DbBase _db;
        private readonly IAuthorizationService _authorizationService;
        public WorkoutService(DbBase db,IAuthorizationService authorizationService)
        {
            _db = db;
            _authorizationService = authorizationService;
        }

        //        *********** Setters ***********

        public async Task<int> CreateWorkout(ClaimsPrincipal User,WorkoutCreationDTO workout)//0 == faulty DTO || 1 == User not found || 2 == forbidden from access || 3 == success
        {//Important detail: day attribute can't be more than 15 chars

            //Checking the validity of the DTO
            if (workout == null || string.IsNullOrEmpty(workout.Name) || workout.UserID == Guid.Empty) return 0;

            //Seaerching for the User
            var isUserExist = await (from user in _db.Users.Include(w => w.Workouts)
                               where user.UserID == workout.UserID
                               select user).FirstOrDefaultAsync();
            if (isUserExist == null) return 1;

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, isUserExist.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return 2;

            //Creating the new Workout
            var newWorkout = new Workout
            {
                WorkoutID = Guid.NewGuid(),
                Name = workout.Name,
                Description = workout.Description,
                Date = workout.Date,
                Difficulty = workout.Difficulty,
                Day = workout.Day,
                CreatAt = DateTime.Now,
                User = isUserExist,
                Schedule = await _db.Schedules.FirstOrDefaultAsync(s => s.User.UserID == workout.UserID)
            };

            //Saving to Database
            isUserExist.Workouts?.Add(newWorkout);
            _db.Users.Update(isUserExist);
            await _db.Workouts.AddAsync(newWorkout);
            await _db.SaveChangesAsync();
            return 3;
        }
        public async Task<int> UpdateWorkout(ClaimsPrincipal User,Guid workoutID, WorkoutUpdateDTO workout)//0 == faulty DTO ||1 == Workout not found || 2 == Forbidden from access || 3 == success
        {
            //Checking the validity of the DTO
            if (workout == null || workoutID == Guid.Empty) return 0;

            //Searching for the Workout
            var WorkoutToBeUpdated = await(from w in _db.Workouts.Include(w => w.User)
                                      where w.WorkoutID == workoutID
                                      select w).FirstOrDefaultAsync();
            if (WorkoutToBeUpdated == null) 
                return 1;

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, WorkoutToBeUpdated.User.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return 2;

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

            //Saving to Database
            _db.Workouts.Update(WorkoutToBeUpdated);
            await _db.SaveChangesAsync();
            return 3;
        }
        public async Task<int> DeleteWorkout(ClaimsPrincipal User, Guid workoutID)//0 == faulty DTO ||1 == Workout not found || 2 == Forbidden from access || 3 == success
        {
            //Checking the workoutID
            if (workoutID == Guid.Empty)
                return 0;

            //Searching for the Workout
            var isWorkoutExist = await (from w in _db.Workouts.Include(w => w.User)
                                        where w.WorkoutID == workoutID
                                  select w).FirstOrDefaultAsync();
            if (isWorkoutExist == null)
                return 1;

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, isWorkoutExist.User.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return 2;

            //Deleting from Database
            _db.Workouts.Remove(isWorkoutExist);
            await _db.SaveChangesAsync();
            return 3;
        }
        public async Task<int> AddExercisesToWorkout(ClaimsPrincipal User,Guid workoutID, WorkoutExerciseDTO workoutExercises)//0 == faulty DTO || 1 == Workout not found || 2 == Forbidden from access ||
                                                                                                              // 3 == No new exercises to add || 4 == success
        {//there is a problem here

            //Checking the validity of the DTO
            if (workoutExercises == null || workoutID == Guid.Empty) return 0;

            //Searching for the Workout
            var workout = await _db.Workouts
                .Include(w => w.Exercises)
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.WorkoutID == workoutID);
            if (workout == null)
                return 1;

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, workout.User.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return 2;

            //Determening if there are new exercises to add
            var existingExerciseIds = new HashSet<Guid>(workout.Exercises.Select(e => e.ExerciseID));
            var exerciseIdsToAdd = workoutExercises.ExercisesID?.Where(id => !existingExerciseIds.Contains(id)).ToList();

            if (exerciseIdsToAdd == null || !exerciseIdsToAdd.Any())
                return 3;

            //Getting the exercises to add from the database
            var exercisesToAdd = await _db.Exercises
                .Where(e => exerciseIdsToAdd.Contains(e.ExerciseID))
                .ToListAsync();
            if (exercisesToAdd == null || !exercisesToAdd.Any())
                return 3;

            //Saving the new exercises to the workout
            foreach (var exercise in exercisesToAdd)
            {
                workout.Exercises.Add(exercise);
            }

            //Saving to Database
            _db.Workouts.Update(workout);
            await _db.SaveChangesAsync();
            return 4;
        }
        public async Task<int> SetExercisesOfWorkout(ClaimsPrincipal User, Guid workoutID, WorkoutExerciseDTO workoutExercises)//0 == faulty DTO || 1 == Workout not found ||2 == Forbidden from access ||
                                                                                                              //3 == No new exercises to add || 4 == success
        {//there is a problem here

            //Checking the validity of the DTO
            if (workoutExercises == null || workoutID == Guid.Empty) 
                return 0;

            //Searching for the Workout
            var isWorkoutExist = await(from w in _db.Workouts.Include(w => w.Exercises).Include(w => w.User)
                                       where w.WorkoutID == workoutID
                                  select w).FirstOrDefaultAsync();
            if(isWorkoutExist == null) 
                return 1;

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, isWorkoutExist.User.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return 2;

            //Clearing existing exercises to add new ones
            isWorkoutExist.Exercises.Clear();

            //Determening if there are new exercises to add
            var exerciseIDsToAdd = workoutExercises.ExercisesID.ToList();
            if (exerciseIDsToAdd == null || exerciseIDsToAdd.Count == 0) 
                return 3;

            //Getting the exercises to add from the database
            var ExercisesToAdd = await _db.Exercises
                                .Where(e => exerciseIDsToAdd.Contains(e.ExerciseID))
                                .ToListAsync();
            if (ExercisesToAdd == null || !ExercisesToAdd.Any())
                return 3;

            //saving the new exercises to the workout
            foreach (var exercise in ExercisesToAdd)
            {
                isWorkoutExist.Exercises.Add(exercise);
            }

            //Saving to Database
            _db.Workouts.Update(isWorkoutExist);
            await _db.SaveChangesAsync();
            return 4;
        }
        public async Task<int> DeleteExercisesFromWorkout(ClaimsPrincipal User, Guid workoutID, WorkoutExerciseDTO workoutExercises)//0 == faulty DTO || 1 == Workout not found || 2 == Forbidden from access
                                                                                                                   //3 == No exercises to remove || 4 == success
        {

            //checking the Validitiy of the DTO
            if (workoutExercises == null || workoutID == Guid.Empty) 
                return 0;

            //Searching for the Workout
            var isWorkoutExist = await(from w in _db.Workouts.Include(w => w.Exercises).Include(w => w.User)
                                  where w.WorkoutID == workoutID
                                  select w).FirstOrDefaultAsync();
            if (isWorkoutExist == null) 
                return 1;

            //Authentication
            var authResult = await _authorizationService.AuthorizeAsync(User, isWorkoutExist.User.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return 2;

            //Determening if there are exercises to Delete
            var existingExerciseIDs = new HashSet<Guid>(isWorkoutExist.Exercises.Select(i => i.ExerciseID));
            var exerciseIDsToRemove = workoutExercises.ExercisesID?.Where(id => existingExerciseIDs.Contains(id)).ToList();
            if (exerciseIDsToRemove == null || !exerciseIDsToRemove.Any()) 
                return 3;

            //Getting the exercises to delete from the Database
            var ExercisesToRemove = await _db.Exercises
                                    .Where(e => exerciseIDsToRemove.Contains(e.ExerciseID))
                                    .ToListAsync();
            if (ExercisesToRemove == null || !ExercisesToRemove.Any())
                return 3;

            //Removing from the Workout
            foreach (var exercise in ExercisesToRemove)
            {
                isWorkoutExist.Exercises.Remove(exercise);
            }

            //Saving to Database
            _db.Workouts.Update(isWorkoutExist);
            await _db.SaveChangesAsync();
            return 4;
        }

        //-----------------------------------------------------------------------

        //        *********** Getters ***********
        public async Task<Guid> GetWorkoutUserID(Guid workoutID)
        {
            //Getting the user by ID
            Guid UserID = await(from w in _db.Workouts
                               where w.WorkoutID == workoutID
                               select w.User.UserID).FirstOrDefaultAsync();
            return UserID; 
        }
        public async Task<WorkoutViewDTO?> GetWorkoutByName(string name)
        {
            //Getting workout by name
            var Workout = await(from w in _db.Workouts
                          where w.Name == name
                          select new WorkoutViewDTO
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
        public async Task<WorkoutViewDTO?> GetWorkoutByID(Guid ID)
        {
            //Getting the Workout by ID
            var Workout = await(from w in _db.Workouts
                          where w.WorkoutID == ID
                          select new WorkoutViewDTO
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
        public async Task<PagedList<ExerciseDTO>?> GetExercisesOfWorkout(Guid WorkoutID, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize)
        {
            //checking if page and Pagesize are 0 or not entered
            if (page == 0) page = 1;
            if(pageSize == 0)pageSize = 10;

            //Getting the exercises in the given workout by workoutID
            var exercisesQuery = from w in _db.Workouts
                            from e in w.Exercises
                            where w.WorkoutID == WorkoutID && w.Exercises.Contains(e)
                            select e;

            //If the searchTerm is not null, we gonna return all the exercises that contains the searchTerm in the name, description Difficulty
            if(!string.IsNullOrEmpty(searchTerm))exercisesQuery = exercisesQuery.Where(e=>e.Name.Contains(searchTerm) || e.Description.Contains(searchTerm)
            || e.Difficulty.Contains(searchTerm));

            //If the sortColumn not null, we gonna order the data by the specified sortColumn
            if (!string.IsNullOrEmpty(sortColumn))
            {
                Expression<Func<Exercise, object>> keySelector = searchTerm.ToLower() switch
                {
                    "name" or "n" => Exercise => Exercise.Name, // Sort by name
                    "difficulty" or "dif" => Exercise => Exercise.Difficulty, // sort by difficulty
                    "description" or "desc" => Exercise => Exercise.Description, // sort by description
                    "category" or "c" => Exercise => Exercise.Category, // sort by category
                    _ => Exercise => Exercise.ExerciseID // failsafe: sort by ID
                };

                //If no orderby was inputed, then we sort ascending
                if (!string.IsNullOrEmpty(OrderBy)) exercisesQuery = exercisesQuery.OrderBy(keySelector);

                //else if anything was inputted we sort descending
                else exercisesQuery = exercisesQuery.OrderByDescending(keySelector);
            }

            //Projecting the resultant exercise queries as exerciseDTO
            var exerciseResult = exercisesQuery
                                .Select(e => new ExerciseDTO
                                {
                                    ExerciseID = e.ExerciseID,
                                    Name = e.Name,
                                    Description = e.Description,
                                    Difficulty = e.Difficulty,
                                    Grip = e.Grip,
                                    Category = e.Category,
                                    VideoUrl = e.VideoUrl,
                                });

            //Making the result as a paged list
            var exercises = await PagedList<ExerciseDTO>.CreateAsync(exerciseResult, page, pageSize);
            return exercises;
        }
        public async Task<PagedList<WorkoutViewDTO>?> GetAllWorkouts(int page, int pageSize)
        {
            //checking if page and Pagesize are 0 or not entered
            if (page == 0) page = 1;
            if(pageSize == 0) pageSize = 10;

            //Getting all workouts and projecting them as WorkoutDTO
            var workoutsQuery = from w in _db.Workouts
                           select new WorkoutViewDTO
                           {
                               UserID = w.User.UserID,
                               WorkoutID = w.WorkoutID,
                               Name = w.Name,
                               Description = w.Description,
                               Date = w.Date,
                               Difficulty = w.Difficulty,
                               Day = w.Day,
                           };

            //turning the result into a paged list
            var workouts = await PagedList<WorkoutViewDTO>.CreateAsync(workoutsQuery, page, pageSize);
            return workouts;
        }
    }
}
