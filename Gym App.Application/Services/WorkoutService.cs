using Gym_App.Application.Authorization;
using Gym_App.Application.BackgroundJobs;
using Gym_App.Domain;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.DTOs.Exercise;
using Gym_App.Infastructure.DTOs.WorkoutDTOs;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infastructure.Transfer_Classes;
using Gym_App.Infrastructure.DTOs.Exercise;
using Gym_App.Infrastructure.DTOs.Workout;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gym_App.Application.Services  
{
    public class WorkoutService : IWorkoutService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICachedAuthorizationService _authorizationService;
        private readonly IWorkoutNotificationSink _notificationService;
        private readonly ILogger<WorkoutService> _logger;

        public WorkoutService(IUnitOfWork unitOfWork, ICachedAuthorizationService authorizationService, IWorkoutNotificationSink notificationService, ILogger<WorkoutService> logger)
        {
            _unitOfWork = unitOfWork;
            _authorizationService = authorizationService;
            _notificationService = notificationService;
            _logger = logger;
        }

        //        *********** Setters ***********
        public async Task<SettersResponse> CreateWorkoutWithExercisesAsync(
            Guid userId,
            WorkoutCreationDTO createWorkoutDto)
        {
            if (createWorkoutDto == null || string.IsNullOrEmpty(createWorkoutDto.Name))
                return new SettersResponse { status = 0, msg = "Invalid workout data" };

            var authResult = await _authorizationService.IsUserAsync(userId);
            if (!authResult)
                return new SettersResponse { status = 1, msg = "Unauthorized" };
            try
            {
                var user = await _unitOfWork.Users.GetById(userId);
                if (user == null)
                    return new SettersResponse { status = 0, msg = "User not found" };

                // Create the main workout
                var workout = new Workout
                {
                    Name = createWorkoutDto.Name,
                    Description = createWorkoutDto.Description,
                    Date = createWorkoutDto.Date,
                    Difficulty = createWorkoutDto.Difficulty,
                    Day = createWorkoutDto.Day,
                    Type = createWorkoutDto.Type,
                    Duration = createWorkoutDto.Duration,
                    User = user,
                    IsCompleted = false,
                    ReminderSent = false,
                    NotificationSent = false
                };

                // Parse scheduled start time if provided
                if (!string.IsNullOrEmpty(createWorkoutDto.ScheduledStartTime))
                {
                    if (TimeSpan.TryParse(createWorkoutDto.ScheduledStartTime, out var startTime))
                    {
                        workout.ScheduledStartTime = startTime;
                        workout.ScheduledEndTime = startTime.Add(TimeSpan.FromMinutes(createWorkoutDto.Duration));
                    }
                }

                await _unitOfWork.Workouts.Create(workout);
                

                // Create exercise instances with sets
                int exerciseOrder = 1;
                bool exerciseAdded,setAdded;exerciseAdded = setAdded = false;
                foreach (var exerciseDto in createWorkoutDto.ExerciseDetails)
                {
                    var exercise = await _unitOfWork.Exercises.GetById(exerciseDto.ExerciseId);
                    if (exercise == null)
                    {
                        _logger.LogWarning($"Exercise {exerciseDto} not found");
                        continue;
                    }
                    exerciseAdded = true;
                    var exerciseInstance = new ExerciseInstance
                    {
                        WorkoutId = workout.Id,
                        ExerciseId = exerciseDto.ExerciseId,
                        ExerciseOrder = exerciseOrder++,
                        PlannedReps = exerciseDto.PlannedReps,
                        PlannedWeight = exerciseDto.PlannedWeight,
                        Notes = exerciseDto.Notes,
                        IsCompleted = false,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                    };

                    await _unitOfWork.ExerciseInstance!.Create(exerciseInstance);

                    // Create sets for this exercise
                    foreach (var setDto in exerciseDto.Sets)
                    {
                        setAdded = true;
                        var workoutSet = new WorkoutSet
                        {
                            ExerciseInstanceId = exerciseInstance.Id,
                            SetNumber = setDto.SetNumber,
                            Reps = setDto.Reps,
                            Weight = setDto.Weight,
                            RestSeconds = setDto.RestSeconds,
                            Notes = setDto.Notes,
                            IsCompleted = false,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                        };

                        await _unitOfWork.WorkoutSet.Create(workoutSet);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"Workout {workout.Id} created with exercises for user {userId}");

                if(exerciseAdded && setAdded)
                    return new SettersResponse { status = 2, msg = "Workout and exercises with sets created successfully" };
                else if(exerciseAdded)
                    return new SettersResponse { status = 2, msg = "Workout and exercises created successfully, but no sets were added" };
                else if (!exerciseAdded)
                    return new SettersResponse { status = 0, msg = "Workout created successfully, but no valid exercises were added" };
                else
                    return new SettersResponse
                    {
                        status = 2,
                        msg = "Workout created successfully",
                    };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating workout: {ex.Message}");
                return new SettersResponse { status = 0, msg = $"Error: {ex.Message}" };
            }
        }
        public async Task<SettersResponse> ManageWorkoutExerciseAsync(Guid workoutID, ExerciseManagementDTO workoutExercises)
        {
            bool anyChange = false;
            try
            {
                var workout = await _unitOfWork.Workouts.GetWorkoutById(workoutID);
                if (workout == null)
                    return new SettersResponse { status = 0, msg = "Workout not found" };
                var authResult = await _authorizationService.IsUserAsync(workout.User.Id);
                if (!authResult)
                    return new SettersResponse { status = 1, msg = "Unauthorized" };
                if(workoutExercises.requestType == requestType.Create)
                {
                    var exercise = await _unitOfWork.Exercises.GetById(workoutExercises.ExerciseId);
                    if (exercise == null)
                        return new SettersResponse { status = 0, msg = "Exercise not found" };
                    var exerciseInstance = new ExerciseInstance
                    {
                        WorkoutId = workout.Id,
                        ExerciseId = workoutExercises.ExerciseId,
                        PlannedReps = workoutExercises.PlannedReps,
                        PlannedWeight = workoutExercises.PlannedWeight,
                        Notes = workoutExercises.Notes,
                        IsCompleted = false,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                    };
                    await _unitOfWork.ExerciseInstance!.Create(exerciseInstance);
                    // Create sets for this exercise
                    foreach (var setDto in workoutExercises.Sets)
                    {
                        var workoutSet = new WorkoutSet
                        {
                            ExerciseInstanceId = exerciseInstance.Id,
                            SetNumber = setDto.SetNumber,
                            Reps = setDto.Reps,
                            Weight = setDto.Weight,
                            RestSeconds = setDto.RestSeconds,
                            Notes = setDto.Notes,
                            IsCompleted = false,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                        };
                        await _unitOfWork.WorkoutSet.Create(workoutSet);
                    }
                    anyChange = true;
                }
                else if(workoutExercises.requestType == requestType.Update)
                {
                    
                    var exerciseInstance = await _unitOfWork.ExerciseInstance.GetById(workoutExercises.ExerciseId);
                    if (exerciseInstance == null)
                        return new SettersResponse { status = 0, msg = "Exercise instance not found in this workout" };
                    if (workoutExercises.PlannedReps != 0)
                    {
                        exerciseInstance.PlannedReps = workoutExercises.PlannedReps;
                        anyChange = true;
                    }
                    if (workoutExercises.PlannedWeight != 0)
                    {
                        exerciseInstance.PlannedWeight = workoutExercises.PlannedWeight;
                        anyChange = true;
                    }
                    if (!string.IsNullOrEmpty(workoutExercises.Notes) && exerciseInstance.Notes != workoutExercises.Notes)
                    {
                        exerciseInstance.Notes = workoutExercises.Notes;
                        anyChange = true;
                    }
                    // Update sets for this exercise
                    var setsResult = await WorkoutSetManagement(workoutExercises.Sets);
                    if (setsResult.status == 0)
                        return new SettersResponse { status = 0, msg = $"Error : {setsResult.msg}" };

                    else if (setsResult.status == 1) 
                        _logger.LogInformation($"no change happened in the sets of {workoutExercises.ExerciseId}");

                    else 
                        anyChange = true;
                    if (anyChange) 
                    {
                        await _unitOfWork.ExerciseInstance.Update(exerciseInstance);
                        exerciseInstance.UpdatedAt = DateTime.Now;
                    } 
                }
                    
                else if(workoutExercises.requestType == requestType.Delete)
                {
                    var exerciseInstance = await _unitOfWork.ExerciseInstance.GetById(workoutExercises.ExerciseId);
                    if (exerciseInstance == null)
                        return new SettersResponse { status = 0, msg = "Exercise instance not found in this workout" };
                    await _unitOfWork.ExerciseInstance.Delete(exerciseInstance);
                    anyChange = true;
                }
                else
                {
                    return new SettersResponse { status = 0, msg = "Invalid request type" };
                }
                if (anyChange)
                {
                    await _unitOfWork.SaveChangesAsync();
                    return new SettersResponse { status = 2, msg = "Workout exercise managed successfully" };
                }
                else
                {
                    return new SettersResponse { status = 2, msg = "No changes were made to the workout exercise" };
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error managing workout exercise: {ex.Message}");
                return new SettersResponse { status = 0, msg = $"Error: {ex.Message}" };
            }
        }
        public async Task<SettersResponse> StartWorkoutAsync(Guid workoutId, Guid userId)
        {
            try
            {
                var workout = await _unitOfWork.Workouts.GetById(workoutId);
                if (workout == null)
                    return new SettersResponse { status = 0, msg = "Workout not found" };

                var authResult = await _authorizationService.IsUserAsync(workout.User.Id);
                if (!authResult)
                    return new SettersResponse { status = 1, msg = "Unauthorized" };

                workout.ActualStartTime = DateTime.UtcNow;
                _unitOfWork.Workouts.Update(workout);
                await _unitOfWork.SaveChangesAsync();

                // Send notification
                await _notificationService.SendWorkoutStartedNotificationAsync(workoutId, userId);

                return new SettersResponse { status = 2, msg = "Workout started" };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error starting workout: {ex.Message}");
                return new SettersResponse { status = 0, msg = $"Error: {ex.Message}" };
            }
        }
        public async Task<SettersResponse> UpdateWorkoutProgressAsync(
            Guid userId,
            WorkoutUpdateProgressDTO progressDto)
        {
            try
            {
                var workout = await _unitOfWork.Workouts.GetById(progressDto.WorkoutId);
                if (workout == null)
                    return new SettersResponse { status = 0, msg = "Workout not found" };

                var authResult = await _authorizationService.IsUserAsync(workout.User.Id);
                if (!authResult)
                    return new SettersResponse { status = 1, msg = "Unauthorized" };

                // Update workout timing
                if (progressDto.ActualStartTime != default)
                    workout.ActualStartTime = progressDto.ActualStartTime;

                if (progressDto.ActualEndTime != default)
                    workout.ActualEndTime = progressDto.ActualEndTime;

                workout.IsCompleted = progressDto.IsCompleted;

                _unitOfWork.Workouts.Update(workout);

                // Update exercise instances and sets
                foreach (var exerciseProgress in progressDto.Exercises)
                {
                    var exerciseInstance = await _unitOfWork.ExerciseInstance.GetById(
                        exerciseProgress.ExerciseId);

                    if (exerciseInstance != null)
                    {
                        exerciseInstance.StartedAt = exerciseProgress.StartedAt;
                        exerciseInstance.CompletedAt = exerciseProgress.CompletedAt;
                        exerciseInstance.IsCompleted = exerciseProgress.IsCompleted;

                        _unitOfWork.ExerciseInstance.Update(exerciseInstance);

                        // Update sets and detect PRs
                        foreach (var setProgress in exerciseProgress.Sets)
                        {
                            var workoutSet = await _unitOfWork.WorkoutSet.GetById(setProgress.SetId);
                            if (workoutSet != null)
                            {
                                workoutSet.IsCompleted = setProgress.IsCompleted;
                                workoutSet.ActualReps = setProgress.ActualReps;
                                workoutSet.ActualWeight = setProgress.ActualWeight;
                                workoutSet.Notes = setProgress.Notes;

                                _unitOfWork.WorkoutSet.Update(workoutSet);

                                // Detect if this is a new PR
                                if (setProgress.ActualWeight.HasValue && setProgress.ActualReps.HasValue)
                                {
                                    await DetectAndCreatePersonalRecordAsync(
                                        userId,
                                        exerciseInstance,
                                        workoutSet);
                                }
                            }
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Workout progress updated for {progressDto.WorkoutId}");

                return new SettersResponse { status = 2, msg = "Progress updated successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating workout progress: {ex.Message}");
                return new SettersResponse { status = 0, msg = $"Error: {ex.Message}" };
            }
        }
        public async Task<SettersResponse> CompleteWorkoutAsync(Guid workoutId, Guid userId)
        {
            try
            {
                var workout = await _unitOfWork.Workouts.GetById(workoutId);
                if (workout == null)
                    return new SettersResponse { status = 0, msg = "Workout not found" };

                var authResult = await _authorizationService.IsUserAsync(workout.User.Id);
                if (!authResult)
                    return new SettersResponse { status = 1, msg = "Unauthorized" };

                workout.IsCompleted = true;
                workout.ActualEndTime = DateTime.UtcNow;

                _unitOfWork.Workouts.Update(workout);
                await _unitOfWork.SaveChangesAsync();

                // Send completion notification with stats
                await _notificationService.SendWorkoutCompletedNotificationAsync(workoutId, userId);

                return new SettersResponse { status = 2, msg = "Workout completed" };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error completing workout: {ex.Message}");
                return new SettersResponse { status = 0, msg = $"Error: {ex.Message}" };
            }
        }
        public async Task<SettersResponse> CreateWorkout(WorkoutCreationDTO workout)
        {
            //Checking the validity of the DTO
            if (workout == null || string.IsNullOrEmpty(workout.Name) || workout.UserID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid workout data" };

            //Searching for the User
            var isUserExist = await _unitOfWork.Users.GetById(workout.UserID);
            if (isUserExist == null)
                return new SettersResponse { status = 0, msg = "User not found" };

            //Authorization
            var authResult = await _authorizationService.IsUserAsync(isUserExist.Id);
            if (!authResult)
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
                User = isUserExist,
                Type = workout.Type,
            };

            //Saving to Database via repository
            await _unitOfWork.Workouts.Create(newWorkout);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Workout created successfully" };
        }

        public async Task<SettersResponse> UpdateWorkout(Guid workoutID, WorkoutUpdateDTO workout)
        {
            //Checking the validity of the DTO
            if (workout == null || workoutID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid workout data" };

            //Searching for the Workout
            var WorkoutToBeUpdated = await _unitOfWork.Workouts.GetWorkoutById(workoutID);
            if (WorkoutToBeUpdated == null)
                return new SettersResponse { status = 0, msg = "Workout not found" };

            //Authorization
            var authResult = await _authorizationService.IsUserAsync(WorkoutToBeUpdated.User.Id);
            if (!authResult)
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

        public async Task<SettersResponse> DeleteWorkout(Guid workoutID)
        {
            //Checking the workoutID
            if (workoutID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid workout ID" };

            //Searching for the Workout
            var isWorkoutExist = await _unitOfWork.Workouts.GetWorkoutById(workoutID);
            if (isWorkoutExist == null)
                return new SettersResponse { status = 0, msg = "Workout not found" };

            //Authorization
            var authResult = await _authorizationService.IsUserAsync(isWorkoutExist.User.Id);
            if (!authResult)
                return new SettersResponse { status = 1, msg = "Forbidden from access" };

            //Deleting from Database via repository
            await _unitOfWork.Workouts.Delete(workoutID);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Workout deleted successfully" };
        }

        public async Task<SettersResponse> AddExercisesToWorkout(Guid workoutID, WorkoutExerciseDTO workoutExercises)
        {
            //Checking the validity of the DTO
            if (workoutExercises == null || workoutID == Guid.Empty || workoutExercises.exercisesDetails == null)
                return new SettersResponse { status = 0, msg = "Invalid data" };

            //Searching for the Workout
            var workout = await _unitOfWork.Workouts.GetWorkoutById(workoutID);
            if (workout == null)
                return new SettersResponse { status = 0, msg = "Workout not found" };

            //Authorization
            var authResult = await _authorizationService.IsUserAsync(workout.User.Id);
            if (!authResult)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            bool added, nonAdded; added = nonAdded = false;

            foreach (var exercise in workoutExercises.exercisesDetails)
            {
                var exerciseEntity = await _unitOfWork.Exercises.GetById(exercise.ExerciseId);
                if (exerciseEntity == null)
                {
                    nonAdded = true;
                    _logger.LogWarning($"Exercise with ID {exercise.ExerciseId} not found");
                    continue;
                }
                if(!workout.ExerciseInstances.Any(x=>x.ExerciseId == exerciseEntity.Id))
                {
                    added = true;
                    var exerciseInstance = new ExerciseInstance
                    {
                        ExerciseId = exerciseEntity.Id,
                        WorkoutId = workout.Id,
                        PlannedReps = exercise.PlannedReps,
                        PlannedWeight = exercise.PlannedWeight,
                        Notes = exercise.Notes,
                        ExerciseOrder = workout.ExerciseInstances.Count + 1
                    };
                    await _unitOfWork.ExerciseInstance.Create(exerciseInstance);
                    foreach (var setDto in exercise.Sets)
                    {
                        var workoutSet = new WorkoutSet
                        {
                            ExerciseInstanceId = exerciseInstance.Id,
                            SetNumber = setDto.SetNumber,
                            Reps = setDto.Reps,
                            Weight = setDto.Weight,
                            RestSeconds = setDto.RestSeconds,
                            Notes = setDto.Notes,
                            IsCompleted = false
                        };

                        await _unitOfWork.WorkoutSet.Create(workoutSet);
                    }
                }
                else
                {
                    _logger.LogInformation($"Exercise with ID {exercise.ExerciseId} already exists in workout {workoutID}");
                    nonAdded = true;
                    continue;
                }

            }
            if(added)await _unitOfWork.SaveChangesAsync();
            if(added && nonAdded)
                return new SettersResponse { status = 2, msg = "Some exercises added successfully, some were already in the workout" };
            else if(added)
                return new SettersResponse { status = 2, msg = "Exercises added successfully" };
            else
                return new SettersResponse { status = 0, msg = "No new exercises were added, all were already in the workout or ids were wrong" };

        }

        public async Task<SettersResponse> SetExercisesOfWorkout(Guid workoutID, WorkoutExerciseDTO workoutExercises)
        {
            //Checking the validity of the DTO
            if (workoutExercises == null || workoutID == Guid.Empty || workoutExercises.exercisesDetails == null)
                return new SettersResponse { status = 0, msg = "Invalid data" };

            //Searching for the Workout
            var isWorkoutExist = await _unitOfWork.Workouts.GetWorkoutById(workoutID);
            if (isWorkoutExist == null)
                return new SettersResponse { status = 0, msg = "Workout not found" };

            //Authorization
            var authResult = await _authorizationService.IsUserAsync(isWorkoutExist.User.Id);
            if (!authResult)
                return new SettersResponse { status = 1, msg = "Forbidden from access" };

            isWorkoutExist.ExerciseInstances.Clear();
            bool added, nonAdded; added = nonAdded = false;
            foreach (var exercise in workoutExercises.exercisesDetails)
            {
                var exerciseEntity = await _unitOfWork.Exercises.GetById(exercise.ExerciseId);
                if (exerciseEntity != null)
                {
                    added = true;
                    var exerciseInstance = new ExerciseInstance
                    {
                        ExerciseId = exerciseEntity.Id,
                        WorkoutId = isWorkoutExist.Id,
                        PlannedReps = 0,
                        PlannedWeight = 0,
                        Notes = "",
                        ExerciseOrder = isWorkoutExist.ExerciseInstances.Count + 1
                    };
                    await _unitOfWork.ExerciseInstance.Create(exerciseInstance);
                    foreach (var set in exercise.Sets)
                    {
                        var workoutSet = new WorkoutSet
                        {
                            ExerciseInstanceId = exerciseInstance.Id,
                            Reps = set.Reps,
                            Weight = set.Weight,
                            RestSeconds = set.RestSeconds,
                            Notes = set.Notes,
                            IsCompleted = false,
                            SetNumber = set.SetNumber
                        };
                        await _unitOfWork.WorkoutSet.Create(workoutSet);
                    }
                }
                else
                {
                    nonAdded = true;
                    _logger.LogInformation($"Exercise with ID {exercise.ExerciseId} does not exist");
                }
                
            }
            if (added) await _unitOfWork.SaveChangesAsync();
            if (added && nonAdded)
                return new SettersResponse { status = 2, msg = "Some exercises set successfully, some were not found" };
            else if (added)
                return new SettersResponse { status = 2, msg = "Exercises set successfully" };
            else
                return new SettersResponse { status = 0, msg = "No exercises were set, all ids were wrong" };

        }

        public async Task<SettersResponse> DeleteExercisesFromWorkout(Guid workoutID, WorkoutExerciseDTO workoutExercises)
        {
            //checking the Validity of the DTO
            if (workoutExercises == null || workoutID == Guid.Empty || workoutExercises.exercisesDetails == null || !workoutExercises.exercisesDetails.Any())
                return new SettersResponse { status = 0, msg = "Invalid data" };

            //Searching for the Workout
            var isWorkoutExist = await _unitOfWork.Workouts.GetWorkoutById(workoutID);
            if (isWorkoutExist == null)
                return new SettersResponse { status = 0, msg = "Workout not found" };

            //Authentication
            var authResult = await _authorizationService.IsUserAsync(isWorkoutExist.User.Id);
            if (!authResult)
                return new SettersResponse { status = 1, msg = "Forbidden from access" };

            bool deleted, nonDeleted; deleted = nonDeleted = false;
            foreach(var exerciseDetail in workoutExercises.exercisesDetails)
            {
                var exerciseInstance = isWorkoutExist.ExerciseInstances.FirstOrDefault(e => e.ExerciseId == exerciseDetail.ExerciseId);
                if (exerciseInstance != null)
                {
                    isWorkoutExist.ExerciseInstances.Remove(exerciseInstance);
                    await _unitOfWork.ExerciseInstance.Delete(exerciseInstance.Id);
                    deleted = true;
                }
                else
                {
                    nonDeleted = true;
                    _logger.LogInformation($"Exercise with ID {exerciseDetail.ExerciseId} not found in workout {workoutID}");
                }
            }
            if(deleted) await _unitOfWork.SaveChangesAsync();
            if(deleted && nonDeleted)
                return new SettersResponse { status = 2, msg = "Some exercises deleted successfully, some were not found" };
            else if(deleted)
                return new SettersResponse { status = 2, msg = "Exercises deleted successfully" };
            else
                return new SettersResponse { status = 0, msg = "No exercises were deleted, all ids were wrong" };
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
                    Exercises = w.ExerciseInstances!.Select(ei => new ExerciseDetailDTO
                    {
                        Id = ei.Id,
                        ExerciseId = ei.ExerciseId,
                        Name = ei.Exercise!.Name,
                        PlannedReps = (int)ei.PlannedReps!,
                        PlannedWeight = (int)ei.PlannedWeight!,
                        Notes = ei.Notes!,
                        Sets = ei.Sets!.Select(ws => new WorkoutSetDTO
                        {
                            SetId = ws.Id,
                            SetNumber = ws.SetNumber,
                            Reps = ws.Reps,
                            Weight = ws.Weight,
                            RestSeconds = ws.RestSeconds,
                            Notes = ws.Notes
                        }).ToList()
                    }).ToList()
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
                Exercises = workout.ExerciseInstances!.Select(ei => new ExerciseDetailDTO
                {
                    Id = ei.Id,
                    ExerciseId = ei.ExerciseId,
                    Name = ei.Exercise!.Name,
                    PlannedReps = (int)ei.PlannedReps!,
                    PlannedWeight = (int)ei.PlannedWeight!,
                    Notes = ei.Notes!,
                    Sets = ei.Sets!.Select(ws => new WorkoutSetDTO
                    {
                        SetId = ws.Id,
                        SetNumber = ws.SetNumber,
                        Reps = ws.Reps,
                        Weight = ws.Weight,
                        RestSeconds = ws.RestSeconds,
                        Notes = ws.Notes
                    }).ToList()
                }).ToList(),
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

            if (workout == null)
                return new GettersResponse<ExerciseViewDTO>
                {
                    status = 0,
                    msg = "No Exercises in given workout"
                };

            // Start with exercises as an enumerable for in-memory operations
            var exercises = workout.ExerciseInstances.AsEnumerable() ?? Enumerable.Empty<ExerciseInstance>();

            //If the searchTerm is not null, filter by name, description, or difficulty
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                exercises = exercises.Where(e =>
                    e.Exercise.Name.ToLower().Contains(searchTerm) ||
                    e.Exercise.Description?.ToLower().Contains(searchTerm) == true ||
                    e.Exercise.Category?.ToLower().Contains(searchTerm) == true ||
                    e.Exercise.Difficulty?.ToLower().Contains(searchTerm) == true);
            }

            //If the sortColumn is not null, sort the data
            if (!string.IsNullOrEmpty(sortColumn))
            {
                var orderLower = (OrderBy ?? string.Empty).ToLowerInvariant();
                bool descending = orderLower == "desc" || orderLower == "descending" || orderLower == "descend" || orderLower == "d";
                
                exercises = sortColumn.ToLower() switch
                {
                    "name" or "n" => descending 
                        ? exercises.OrderByDescending(e => e.Exercise.Name)
                        : exercises.OrderBy(e => e.Exercise.Name),
                    "difficulty" or "dif" => descending
                        ? exercises.OrderByDescending(e => e.Exercise.Difficulty)
                        : exercises.OrderBy(e => e.Exercise.Difficulty),
                    "description" or "desc" => descending
                        ? exercises.OrderByDescending(e => e.Exercise.Description)
                        : exercises.OrderBy(e => e.Exercise.Description),
                    "category" or "cat" => descending
                        ? exercises.OrderByDescending(e => e.Exercise.Category)
                        : exercises.OrderBy(e => e.Exercise.Category),
                    "date" or "createdat" or "created" => descending
                        ? exercises.OrderByDescending(e => e.Exercise.CreatedAt)
                        : exercises.OrderBy(e => e.Exercise.CreatedAt),
                    _ => exercises
                };
            }

            //Projecting the resultant exercises to ExerciseViewDTO
            var allExercises = exercises
                .Select(e => new ExerciseViewDTO
                {
                    ExerciseID = e.Id,
                    Name = e.Exercise.Name,
                    Description = e.Exercise.Description,
                    Difficulty = e.Exercise.Difficulty,
                    Grip = e.Exercise.Grip,
                    Category = e.Exercise.Category,
                    VideoUrl = e.Exercise.VideoUrl,
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
        public async Task<GettersResponse<PersonalRecord>> GetUserPersonalRecordsAsync(Guid userId)
        {
            try
            {
                var prs = _unitOfWork.PersonalRecords.GetAll().Where(pr => pr.UserId == userId)
                    .OrderByDescending(pr => pr.AchievedDate);
                var pagedPrs = await PagedList<PersonalRecord>.CreateAsync(prs, 1, prs.Count());

                return new GettersResponse<PersonalRecord>
                {
                    status = 2,
                    msg = "Personal records retrieved",
                    Data = pagedPrs
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting personal records: {ex.Message}");
                return new GettersResponse<PersonalRecord> { status = 0, msg = $"Error: {ex.Message}" };
            }
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
                    Exercises = w.ExerciseInstances.Select(ei => new ExerciseDetailDTO
                    {
                        Id = ei.Id,
                        ExerciseId = ei.ExerciseId,
                        Name = ei.Exercise.Name,
                        PlannedReps = (int)ei.PlannedReps,
                        PlannedWeight = (int)ei.PlannedWeight,
                        Notes = ei.Notes,
                        Sets = ei.Sets.Select(ws => new WorkoutSetDTO
                        {
                            SetId = ws.Id,
                            SetNumber = ws.SetNumber,
                            Reps = ws.Reps,
                            Weight = ws.Weight,
                            RestSeconds = ws.RestSeconds,
                            Notes = ws.Notes
                        }).ToList()
                    }).ToList()
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

        // ***************** Helpers & Private Methods *****************

        public Task<Guid> GetWorkoutUserID(Guid WorkoutID)
        {
            throw new NotImplementedException();
        }
        private async Task<SettersResponse> WorkoutSetManagement(IEnumerable<WorkoutSetManagementDTO> workoutSets)
        {
            try
            {
                bool changesHappened = false;
                foreach(var setDto in workoutSets)
                {
                    if(setDto == null || string.IsNullOrEmpty(setDto.requestType) || !requestType.IsValid(setDto.requestType))
                    {
                        _logger.LogWarning("empty dto given");
                        continue; // Skip empty DTOs
                    }
                    if (setDto.requestType.ToLower() == requestType.Create)
                    {
                        var newWorkoutSet = new WorkoutSet
                        {
                            ExerciseInstanceId = setDto.ExerciseInstanceId,
                            SetNumber = setDto.SetNumber,
                            Reps = setDto.Reps,
                            Weight = setDto.Weight,
                            RestSeconds = setDto.RestSeconds,
                            Notes = setDto.Notes,
                            IsCompleted = setDto.IsCompleted,
                            ActualReps = setDto.ActualReps,
                            ActualWeight = setDto.ActualWeight
                        };
                        _logger.LogInformation($"Creating new workout set for ExerciseInstanceId {setDto.SetId}");
                        changesHappened = true;
                        await _unitOfWork.WorkoutSet.Create(newWorkoutSet);
                        continue; // Skip to next iteration after creation
                    }
                    var workoutSet = await _unitOfWork.WorkoutSet.GetById(setDto.SetId);
                    if(setDto.requestType.ToLower() == requestType.Update)
                    {
                        if (workoutSet != null)
                        {
                            workoutSet.Reps = setDto.Reps;
                            workoutSet.Weight = setDto.Weight;
                            workoutSet.RestSeconds = setDto.RestSeconds;
                            workoutSet.Notes = setDto.Notes;
                            workoutSet.IsCompleted = setDto.IsCompleted;
                            workoutSet.ActualReps = setDto.ActualReps;
                            workoutSet.ActualWeight = setDto.ActualWeight;
                            _logger.LogInformation($"Updating workout set with ID {setDto.SetId}");
                            changesHappened = true;
                            await _unitOfWork.WorkoutSet.Update(workoutSet);
                        }
                    }
                    else if(setDto.requestType.ToLower() == requestType.Delete)
                    {
                        if (workoutSet != null)
                        {
                            _logger.LogInformation($"Deleting workout set with ID {setDto.SetId}");
                            changesHappened = true;
                            await _unitOfWork.WorkoutSet.Delete(workoutSet.Id);
                        }
                        else 
                        {
                            _logger.LogInformation($"Workout set with ID {setDto.SetId} not found for deletion");
                        }
                    }
                }
                if (changesHappened) 
                {
                    await _unitOfWork.SaveChangesAsync();
                    return new SettersResponse { status = 2, msg = "Workout sets managed successfully" };
                }
                return new SettersResponse { status = 1, msg = "No change happened" };// 1 here equals no change happened
                
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error managing workout sets: {ex.Message}");
                return new SettersResponse { status = 0, msg = $"Error: {ex.Message}" };
            }
        }
        private async Task DetectAndCreatePersonalRecordAsync(
            Guid userId,
            ExerciseInstance exerciseInstance,
            WorkoutSet workoutSet)
        {
            try
            {
                if (!workoutSet.ActualWeight.HasValue || !workoutSet.ActualReps.HasValue)
                    return;

                var exercise = await _unitOfWork.Exercises.GetById(exerciseInstance.ExerciseId);
                if (exercise == null)
                    return;

                // Get user's current PR for this exercise
                var currentPR = await _unitOfWork.PersonalRecords.GetUserExercisePRAsync(
                    userId,
                    exerciseInstance.ExerciseId);

                // Check if new PR achieved
                bool isPR = false;

                if (currentPR == null)
                {
                    // First time doing this exercise
                    isPR = true;
                }
                else if (workoutSet.ActualWeight > currentPR.Weight)
                {
                    // More weight lifted
                    isPR = true;
                }
                else if (workoutSet.ActualWeight == currentPR.Weight &&
                         workoutSet.ActualReps > currentPR.Reps)
                {
                    // Same weight, more reps
                    isPR = true;
                }

                if (isPR)
                {
                    var newPR = new PersonalRecord
                    {
                        UserId = userId,
                        ExerciseId = exerciseInstance.ExerciseId,
                        Weight = workoutSet.ActualWeight.Value,
                        Reps = workoutSet.ActualReps.Value,
                        AchievedDate = DateTime.UtcNow,
                        WorkoutSetId = workoutSet.Id,
                        NotificationSent = false
                    };

                    _unitOfWork.PersonalRecords.Create(newPR);
                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation(
                        $"New PR detected for user {userId} on exercise {exercise.Name}: " +
                        $"{workoutSet.ActualReps} reps @ {workoutSet.ActualWeight}kg");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error detecting PR: {ex.Message}");
            }
        }

        
    }
}
