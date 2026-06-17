using Gym_App.Application.Authorization;
using Gym_App.Application.Hubs;
using Gym_App.Domain;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.DTOs.WorkoutDTOs;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infastructure.Transfer_Classes;
using Gym_App.Infrastructure.DTOs.Exercise;
using Gym_App.Infrastructure.DTOs.Workout;
using Gym_App.Infrastructure.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gym_App.Application.Services  
{
    public class WorkoutService : IWorkoutService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICachedAuthorizationService _authorizationService;
        private readonly IWorkoutNotificationSink _notificationService;
        private readonly IUserStatsService _stats;
        private readonly ILogger<WorkoutService> _logger;

        public WorkoutService(IUnitOfWork unitOfWork, ICachedAuthorizationService authorizationService, IWorkoutNotificationSink notificationService, IUserStatsService stats, ILogger<WorkoutService> logger)
        {
            _unitOfWork = unitOfWork;
            _authorizationService = authorizationService;
            _notificationService = notificationService;
            _stats = stats;
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
                    if(DateTime.TryParse(createWorkoutDto.ScheduledStartTime, out DateTime scheduledStart))
                    {
                        workout.ScheduledStartTime = scheduledStart;
                    }
                    else
                    {
                        _logger.LogWarning($"Invalid scheduled start time format: {createWorkoutDto.ScheduledStartTime}");
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
                        PlannedWeight = (decimal?)exerciseDto.PlannedWeight,
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
                            Weight = (decimal?)setDto.Weight,
                            RestSeconds = setDto.RestSeconds,
                            Notes = setDto.Notes,
                            IsCompleted = false,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            ActualReps = 0,
                            ActualWeight = 0,
                            KCaloriesBurned = 0,
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
                if(requestType.Create.Contains(workoutExercises.requestType.ToLower()))
                {
                    var exercise = await _unitOfWork.Exercises!.GetById(workoutExercises.ExerciseId);
                    if (exercise == null)
                        return new SettersResponse { status = 0, msg = "Exercise not found" };
                    var exerciseInstance = new ExerciseInstance
                    {
                        WorkoutId = workout.Id,
                        ExerciseId = workoutExercises.ExerciseId,
                        Exercise = exercise,
                        PlannedReps = workoutExercises.PlannedReps,
                        PlannedWeight = workoutExercises.PlannedWeight,
                        Notes = workoutExercises.Notes,
                        ExerciseOrder = workoutExercises.ExerciseOrder != 0 ? workoutExercises.ExerciseOrder : workout.ExerciseInstances!.Count + 1,
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
                            ActualReps = 0,
                            ActualWeight = 0,
                            KCaloriesBurned = 0,
                        };
                        await _unitOfWork.WorkoutSet.Create(workoutSet);
                    }
                    anyChange = true;
                }
                else if(requestType.Update.Contains(workoutExercises.requestType.ToLower()))
                {
                    
                    var exerciseInstance = await _unitOfWork.ExerciseInstance.GetById(workoutExercises.ExerciseId);
                    if (exerciseInstance == null)
                        return new SettersResponse { status = 0, msg = "Exercise instance not found in this workout" };

                    if (workoutExercises.PlannedReps != 0 && workoutExercises.PlannedReps != exerciseInstance.PlannedReps)
                    {
                        exerciseInstance.PlannedReps = workoutExercises.PlannedReps;
                        anyChange = true;
                    }
                    if (workoutExercises.PlannedWeight != 0 && workoutExercises.PlannedWeight != exerciseInstance.PlannedWeight)
                    {
                        exerciseInstance.PlannedWeight = workoutExercises.PlannedWeight;
                        anyChange = true;
                    }
                    if (exerciseInstance.ExerciseOrder != 0 && exerciseInstance.ExerciseOrder != workoutExercises.ExerciseOrder)
                    {
                        exerciseInstance.ExerciseOrder = workoutExercises.ExerciseOrder;
                        anyChange = true;
                    }
                    if (!string.IsNullOrEmpty(workoutExercises.Notes) && workoutExercises.Notes != exerciseInstance.Notes)
                    {
                        exerciseInstance.Notes = workoutExercises.Notes;
                        anyChange = true;
                    }
                    // Update sets for this exercise
                    var setsResult = await WorkoutSetManagement(exerciseInstance.Id, workoutExercises.Sets);
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
                    
                else if(requestType.Delete.Contains(workoutExercises.requestType.ToLower()))
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
                var workout = await _unitOfWork.Workouts.GetWorkoutById(workoutId);
                if (workout == null)
                    return new SettersResponse { status = 0, msg = "Workout not found" };

                var authResult = await _authorizationService.IsUserAsync(userId);
                if (!authResult)
                    return new SettersResponse { status = 1, msg = "Unauthorized" };

                if (workout.hasStarted)
                    return new SettersResponse { status = 0, msg = "Workout has already started" };

                if(workout.ExerciseInstances.Count <= 2)
                {
                    _logger.LogInformation($"Workout {workoutId} cannot be started as it has only {workout.ExerciseInstances.Count} exercises");
                    return new SettersResponse { status = 0, msg = "Workout cannot be started with less than 3 exercises" };
                }

                workout.ActualStartTime = DateTime.UtcNow;
                workout.hasStarted = true;
                await _unitOfWork.Workouts.Update(workout);
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
                if(progressDto == null || progressDto.WorkoutId == Guid.Empty || progressDto.Exercises == null)
                {
                    if (progressDto == null) _logger.LogInformation("progressDto is empty");
                    return new SettersResponse { status = 0, msg = "Invalid progress data" };
                }
                    

                var workout = await _unitOfWork.Workouts.GetWorkoutById(progressDto.WorkoutId);
                if (workout == null)
                    return new SettersResponse { status = 0, msg = "Workout not found" };

                var authResult = await _authorizationService.IsUserAsync(userId);
                if (!authResult)
                    return new SettersResponse { status = 1, msg = "Unauthorized" };


                if (!workout.hasStarted)
                    return new SettersResponse { status = 0, msg = "Workout hasn't started" };

                if(workout.IsCompleted)
                    return new SettersResponse { status = 0, msg = "Workout is already done" };

                List<ExerciseInstance> exercisesDone = new List<ExerciseInstance>();
                List<WorkoutSet> setsDone = new List<WorkoutSet>();
                int exerciseRepeatedValues = 0;
                int setRepeatedValues = 0;
                
                // Update exercise instances and sets
                foreach (var exerciseProgress in progressDto.Exercises)
                {
                    var exerciseInstance = await _unitOfWork.ExerciseInstance.GetWithSetsAsync(
                        exerciseProgress.ExerciseInstanceId);

                    if (exerciseInstance != null)
                    {
                        if (exercisesDone.Contains(exerciseInstance))
                        {
                            _logger.LogWarning($"user {userId} send the same exact exercise instance {exerciseInstance.Id} - {exerciseInstance.Exercise.Name} to be updated multiple times");
                            exerciseRepeatedValues++;
                            continue;
                        }
                        if(exerciseProgress.StartedAt != null)
                        {
                            _logger.LogInformation($"Starting exercise instance {exerciseInstance.Id} as part of workout progress update");
                            exerciseInstance.StartedAt = exerciseProgress.StartedAt;
                        }

                        if(exerciseProgress.IsCompleted == true)
                        {
                            _logger.LogInformation($"Updating completion status for exercise instance {exerciseInstance.Id} as part of workout progress update");
                            exerciseInstance.IsCompleted = exerciseProgress.IsCompleted;
                        }

                        if(exerciseInstance.CompletedAt != null)
                        {
                            _logger.LogInformation($"Completing exercise instance {exerciseInstance.Id} as part of workout progress update");
                            exerciseInstance.CompletedAt = exerciseProgress.CompletedAt;
                        }

                        exercisesDone.Add(exerciseInstance);
                        await _unitOfWork.ExerciseInstance.Update(exerciseInstance);

                        if(exerciseProgress.Sets == null)
                        {
                            _logger.LogInformation($"No sets provided for exercise instance {exerciseInstance.Id} in workout progress update");
                            continue;
                        }
                        // Update sets and detect PRs
                        foreach (var setProgress in exerciseProgress.Sets)
                        {
                            var workoutSet = await _unitOfWork.WorkoutSet.GetById(setProgress.SetId);

                            if (setsDone.Contains(workoutSet))
                            {
                                _logger.LogWarning($"user {userId} send the same exact exercise instance {exerciseInstance.Id} - {exerciseInstance.Exercise.Name} to be updated multiple times");
                                setRepeatedValues++;
                                continue;
                            }
                            if (workoutSet != null)
                            {
                                workoutSet.IsCompleted = setProgress.IsCompleted;
                                workoutSet.ActualReps = setProgress.ActualReps;
                                workoutSet.ActualWeight = setProgress.ActualWeight;
                                workoutSet.Notes = setProgress.Notes;
                                workoutSet.KCaloriesBurned = setProgress.KCaloriesBurned;

                                await _unitOfWork.WorkoutSet.Update(workoutSet);

                                // Detect if this is a new PR
                                
                                await DetectAndCreatePersonalRecordAsync(
                                    userId,
                                    exerciseInstance,
                                    workoutSet);
                            }
                        }
                    }
                }
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Workout progress updated for {progressDto.WorkoutId}");

                if(exerciseRepeatedValues > 0 || setRepeatedValues > 0)
                {
                    _logger.LogInformation($"There has been a total repetition of {exerciseRepeatedValues} for exercises and {setRepeatedValues} for sets");
                    return new SettersResponse { status = 2, msg = "There are some repeated values given but nevertheless progress have been updated" };
                }

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
                if (workoutId == Guid.Empty || userId == Guid.Empty)
                    return new SettersResponse { status = 0, msg = "Invalid data" };

                var workout = await _unitOfWork.Workouts.GetWorkoutById(workoutId);
                if (workout == null)
                    return new SettersResponse { status = 0, msg = "Workout not found" };

                var authResult = await _authorizationService.IsUserAsync(userId);
                if (!authResult)
                    return new SettersResponse { status = 1, msg = "Unauthorized" };

                if (!workout.hasStarted)
                    return new SettersResponse { status = 0, msg = "Workout hasn't even started yet" };

                if (workout.IsCompleted)
                    return new SettersResponse { status = 0, msg = "Workout is already done" };

                workout.IsCompleted = true;
                workout.ActualEndTime = DateTime.UtcNow;

                int exercisesCompleted = 0;
                foreach(var exercise in workout.ExerciseInstances)
                {
                    if (exercise.IsCompleted)
                        exercisesCompleted++;
                }
                if(exercisesCompleted < workout.ExerciseInstances.Count / 2 || workout.ExerciseInstances.Count < 3)
                {
                    _logger.LogError($"User {workout.User.Id} has not finished atleast 50% of the exercises in workout {workout.Id}");
                    return new SettersResponse { msg = "Either you didn't finish atleast 50% of the exercises in the workout or there were less than 3", status = 0 };
                }


                var result = await _stats.AddDailyStats(workout);
                _logger.LogInformation(result.msg);

                result = await _stats.AddWeeklyStats(user: workout.User);
                _logger.LogInformation(result.msg);

                result = await _stats.AddMonthlyStats(user: workout.User);
                _logger.LogInformation(result.msg);

                result = await _stats.AddAllTimeStats(workout);
                _logger.LogInformation(result.msg);

                await _unitOfWork.Workouts.Update(workout);
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
                added = true;
                var exerciseInstance = new ExerciseInstance
                {
                   ExerciseId = exerciseEntity.Id,
                   WorkoutId = workout.Id,
                   PlannedReps = exercise.PlannedReps,
                   PlannedWeight = (decimal?)exercise.PlannedWeight,
                   Notes = exercise.Notes,
                   ExerciseOrder = exercise.ExerciseOrder != 0 ? exercise.ExerciseOrder : workout.ExerciseInstances.Count + 1,
                };
                await _unitOfWork.ExerciseInstance.Create(exerciseInstance);
                foreach (var setDto in exercise.Sets)
                {
                   var workoutSet = new WorkoutSet
                   {
                       ExerciseInstanceId = exerciseInstance.Id,
                       SetNumber = setDto.SetNumber,
                       Reps = setDto.Reps,
                       Weight = (decimal?)setDto.Weight,
                       RestSeconds = setDto.RestSeconds,
                       Notes = setDto.Notes,
                       IsCompleted = false,
                       ActualReps = 0,
                       ActualWeight = 0,
                       KCaloriesBurned = 0,
                   };

                   await _unitOfWork.WorkoutSet.Create(workoutSet);
                }
            }
            if(added)await _unitOfWork.SaveChangesAsync();
            if(added && nonAdded)
                return new SettersResponse { status = 2, msg = "Some exercises were added successfully, but some were not added because they were already in the workout or their ids were wrong" };
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
                return new SettersResponse { status = 1, msg = "Unauthorized" };

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
                            Weight = (decimal?)set.Weight,
                            RestSeconds = set.RestSeconds,
                            Notes = set.Notes,
                            IsCompleted = false,
                            SetNumber = set.SetNumber,
                            ActualReps = 0,
                            ActualWeight = 0,
                            KCaloriesBurned = 0,
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
                return new SettersResponse { status = 2, msg = "Some exercises were set successfully, but some were not set because their exercise ids were wrong" };
            else if (added)
                return new SettersResponse { status = 2, msg = "Exercises set successfully" };
            else
                return new SettersResponse { status = 0, msg = "No exercises were set, all ids were wrong" };

        }

        public async Task<SettersResponse> DeleteExercisesFromWorkout(Guid workoutID, List<Guid> exerciseInstanceIds)
        {
            //checking the Validity of the DTO
            if (exerciseInstanceIds == null || workoutID == Guid.Empty || !exerciseInstanceIds.Any())
                return new SettersResponse { status = 0, msg = "Invalid data" };

            //Searching for the Workout
            var isWorkoutExist = await _unitOfWork.Workouts.GetWorkoutById(workoutID);
            if (isWorkoutExist == null)
                return new SettersResponse { status = 0, msg = "Workout not found" };

            //Authentication
            var authResult = await _authorizationService.IsUserAsync(isWorkoutExist.User.Id);
            if (!authResult)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            bool deleted, nonDeleted; deleted = nonDeleted = false;
            foreach(var exerciseInstanceId in exerciseInstanceIds)
            {
                var exerciseInstance = isWorkoutExist.ExerciseInstances.FirstOrDefault(e => e.Id == exerciseInstanceId);
                if (exerciseInstance != null)
                {
                    isWorkoutExist.ExerciseInstances.Remove(exerciseInstance);
                    await _unitOfWork.ExerciseInstance.Delete(exerciseInstance.Id);
                    deleted = true;
                }
                else
                {
                    nonDeleted = true;
                    _logger.LogInformation($"Exercise with ID {exerciseInstanceId} not found in workout {workoutID}");
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

        //                    *********** Getters ***********

        //-----------------------------------------------------------------------
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
                        Muscles = ei.Exercise.Muscles!.Select(muscle => muscle.Name),
                        Sets = ei.Sets!.Select(ws => new WorkoutSetDTO
                        {
                            SetId = ws.Id,
                            SetNumber = ws.SetNumber,
                            Reps = ws.Reps,
                            Weight = (float?)ws.Weight,
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
                    Muscles = ei.Exercise.Muscles!.Select(muscle => muscle.Name),
                    Sets = ei.Sets!.Select(ws => new WorkoutSetDTO
                    {
                        SetId = ws.Id,
                        SetNumber = ws.SetNumber,
                        Reps = ws.Reps,
                        Weight = (float?)ws.Weight,
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
        public async Task<GettersResponse<WorkoutViewDTO>> GetUsersWorkouts(Guid userId, string searchTerm, string sortColumn, string OrderBy, int page = 1, int pageSize = 10)
        {
            if(userId == Guid.Empty)
            {
                _logger.LogError("User tried to access their workouts but the userId was empty");
                return new GettersResponse<WorkoutViewDTO> { msg = "Invalid userId", status = 0 };
            }

            var authResult = await _authorizationService.IsUserAsync(userId);
            if (!authResult)
                return new GettersResponse<WorkoutViewDTO> { status = 1, msg = "Unauthorized" };

            var workouts = _unitOfWork.Workouts.GetAll().Where(w => w.User.Id == userId);
            
            if(!string.IsNullOrEmpty(searchTerm))
                workouts = _unitOfWork.Workouts.Search(searchTerm, workouts);

            if(!string.IsNullOrEmpty(sortColumn))
                workouts = _unitOfWork.Workouts.FilterSortColumn(sortColumn, OrderBy, workouts);

            var workoutsDto =  workouts.Select(w => new WorkoutViewDTO
             {
                 Day = w.Day,
                 Date = w.Date,
                 ScheduledStartTime = w.ScheduledStartTime,
                 Description = w.Description,
                 Difficulty = w.Difficulty,
                 Exercises = w.ExerciseInstances.Select(e => new ExerciseDetailDTO
                 {
                     ExerciseId = e.ExerciseId,
                     StartedAt = e.StartedAt,
                     CompletedAt = e.CompletedAt,
                     ExerciseOrder = e.ExerciseOrder,
                     Id = e.Id,
                     IsCompleted = e.IsCompleted,
                     Muscles = e.Exercise.Muscles.Select(m => m.Name),
                     Name = e.Exercise.Name,
                     Notes = e.Notes,
                     PlannedReps = (int)e.PlannedReps,
                     PlannedWeight = (int)e.PlannedReps,
                     Sets = e.Sets.Select(s => new WorkoutSetDTO
                     {
                         SetId = s.Id,
                         SetNumber = s.SetNumber,
                         RestSeconds = s.RestSeconds,
                         ActualReps = s.ActualReps,
                         ActualWeight = (float)s.ActualWeight!,
                         IsCompleted = s.IsCompleted,
                         KcaloriesBurned = s.KCaloriesBurned,
                         Notes = s.Notes,
                         Reps = s.Reps,
                         Weight = (float)s.Weight!
                     })
                 }),
                 Name = w.Name,
                 UserID = userId,
                 WorkoutID = w.Id
             });
            if (workouts.Count() == 0)
            {
                _logger.LogError( $"User tried to access their workouts with id {userId} but no exercises returned");
                return new GettersResponse<WorkoutViewDTO> { msg = "No exercises", status = 0 };
            }
            var pagedWorkouts = await PagedList<WorkoutViewDTO>.CreateAsync(workoutsDto, page, pageSize);

            return new GettersResponse<WorkoutViewDTO>
            {
                status = 2,
                msg = "Successful",
                Data = pagedWorkouts
            };
        }
        public async Task<GettersResponse<ExerciseDetailDTO>> GetExercisesOfWorkout(Guid WorkoutID, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize)
        {
            //Getting the workout and its exercises from repository
            var workout = await _unitOfWork.Workouts.GetWorkoutById(WorkoutID);

            if (workout == null)
                return new GettersResponse<ExerciseDetailDTO>
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
                    (e.Exercise!.Name != null && e.Exercise.Name.ToLower().Contains(searchTerm)) ||
                    (e.Exercise.Description != null && e.Exercise.Description.ToLower().Contains(searchTerm)) ||
                    (e.Exercise.Difficulty != null && e.Exercise.Difficulty.ToLower().Contains(searchTerm)) ||
                    (e.Notes != null && e.Notes.ToLower().Contains(searchTerm))
                );
            }

            //If the sortColumn is not null, sort the data
            if (!string.IsNullOrEmpty(sortColumn))
            {
                var orderLower = (OrderBy ?? string.Empty).ToLowerInvariant();
                bool descending = orderLower == "desc" || orderLower == "descending" || orderLower == "descend" || orderLower == "d";
                
                exercises = sortColumn.ToLower() switch
                {
                    "name" or "n" => descending 
                        ? exercises.OrderByDescending(e => e.Exercise!.Name)
                        : exercises.OrderBy(e => e.Exercise!.Name),
                    "difficulty" or "dif" => descending
                        ? exercises.OrderByDescending(e => e.Exercise!.Difficulty)
                        : exercises.OrderBy(e => e.Exercise!.Difficulty),
                    "description" or "desc" => descending
                        ? exercises.OrderByDescending(e => e.Exercise!.Description)
                        : exercises.OrderBy(e => e.Exercise!.Description),
                    "category" or "cat" => descending
                        ? exercises.OrderByDescending(e => e.Exercise!.Category)
                        : exercises.OrderBy(e => e.Exercise!.Category),
                    "date" or "createdat" or "created" => descending
                        ? exercises.OrderByDescending(e => e.Exercise!.CreatedAt)
                        : exercises.OrderBy(e => e.Exercise!.CreatedAt),
                    _ => exercises
                };
            }

            //Projecting the resultant exercises to ExerciseDetailDTO
            var allExercises = exercises
                .Select(e => new ExerciseDetailDTO
                {
                    ExerciseId = e.ExerciseId,
                    Name = e.Exercise.Name,
                    StartedAt = e.StartedAt,
                    CompletedAt = e.CompletedAt,
                    Id = e.Id,
                    IsCompleted = e.IsCompleted,
                    Notes = e.Notes,
                    PlannedReps = (int)e.PlannedReps!,
                    PlannedWeight = (int)e.PlannedWeight!,
                    ExerciseOrder = e.ExerciseOrder,
                    Muscles = e.Exercise.Muscles!.Select(muscle => muscle.Name),
                    Sets = e.Sets.Select(s => new WorkoutSetDTO
                    {
                        SetId = s.Id,
                        SetNumber = s.SetNumber,
                        Reps = s.Reps,
                        Weight = (float?)s.Weight,
                        RestSeconds = s.RestSeconds,
                        Notes = s.Notes
                    }).ToList(),
                })
                .ToList(); // Materialize to list

            //Manual paging for in-memory data
            int totalCount = allExercises.Count;
            var pagedItems = allExercises
                .Skip(pageSize * (page - 1))
                .Take(pageSize)
                .ToList();

            var pagedExercises = new PagedList<ExerciseDetailDTO>(pagedItems, page, pageSize, totalCount);
            
            return new GettersResponse<ExerciseDetailDTO>
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
                    ScheduledStartTime = w.ScheduledStartTime,
                    Exercises = w.ExerciseInstances.Select(ei => new ExerciseDetailDTO
                    {
                        Id = ei.Id,
                        ExerciseId = ei.ExerciseId,
                        Name = ei.Exercise.Name,
                        PlannedReps = (int)ei.PlannedReps,
                        PlannedWeight = (int)ei.PlannedWeight,
                        Notes = ei.Notes,
                        Muscles = ei.Exercise.Muscles!.Select(muscle => muscle.Name),
                        Sets = ei.Sets.Select(ws => new WorkoutSetDTO
                        {
                            SetId = ws.Id,
                            SetNumber = ws.SetNumber,
                            Reps = ws.Reps,
                            Weight = (float?)ws.Weight,
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
        private async Task<SettersResponse> WorkoutSetManagement(Guid exerciseInstanceId, IEnumerable<WorkoutSetManagementDTO> workoutSets)
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
                    if (requestType.Create.Contains(setDto.requestType.ToLower()))
                    {
                        var newWorkoutSet = new WorkoutSet
                        {
                            ExerciseInstanceId = exerciseInstanceId,
                            SetNumber = setDto.SetNumber,
                            Reps = setDto.Reps,
                            Weight = setDto.Weight,
                            RestSeconds = setDto.RestSeconds,
                            Notes = setDto.Notes,
                            IsCompleted = setDto.IsCompleted,
                            ActualReps = 0,
                            ActualWeight = 0,
                            KCaloriesBurned = 0,
                        };
                        _logger.LogInformation($"Creating new workout set for ExerciseInstanceId {setDto.SetId}");
                        changesHappened = true;
                        await _unitOfWork.WorkoutSet.Create(newWorkoutSet);
                        continue; // Skip to next iteration after creation
                    }
                    var workoutSet = await _unitOfWork.WorkoutSet.GetById(setDto.SetId);
                    if(workoutSet == null)
                    {
                        _logger.LogInformation($"Workout set with ID {setDto.SetId}");
                        continue;
                    }
                    if(requestType.Update.Contains(setDto.requestType.ToLower()))
                    {
                        if (setDto.Reps != 0 && setDto.Reps != workoutSet.Reps)
                        {
                            workoutSet.Reps = setDto.Reps;
                            changesHappened = true;
                        }
                                

                        if (setDto.Weight != 0 && workoutSet.Weight != setDto.Weight)
                        {
                            workoutSet.Weight = setDto.Weight;
                            changesHappened = true;
                        }

                        if (setDto.RestSeconds != 0 && workoutSet.RestSeconds != setDto.RestSeconds)
                        {
                            workoutSet.RestSeconds = setDto.RestSeconds;
                            changesHappened = true;
                        }

                        if (!string.IsNullOrEmpty(setDto.Notes) && workoutSet.Notes != setDto.Notes)
                        {
                            workoutSet.Notes = setDto.Notes;
                            changesHappened = true;
                        }
                        if (workoutSet.IsCompleted != setDto.IsCompleted)
                        {
                            workoutSet.IsCompleted = setDto.IsCompleted;
                            changesHappened = true;
                        }

                        _logger.LogInformation($"Updating workout set with ID {setDto.SetId}" + (changesHappened ? " with changes" : " without changes"));
                        await _unitOfWork.WorkoutSet.Update(workoutSet);
                    }
                    else if(requestType.Delete.Contains(setDto.requestType.ToLower()))
                    {
                        _logger.LogInformation($"Deleting workout set with ID {setDto.SetId}");
                        changesHappened = true;
                        await _unitOfWork.WorkoutSet.Delete(workoutSet.Id);
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
                        Weight = workoutSet.ActualWeight,
                        Reps = workoutSet.ActualReps,
                        AchievedDate = DateTime.UtcNow,
                        WorkoutSetId = workoutSet.Id,
                        NotificationSent = false
                    };
                    await _notificationService.PushAsync(newPR);
                    await _unitOfWork.PersonalRecords.Create(newPR);
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
