using Gym_App.Application.Authorization;
using Gym_App.Application.Hubs;
using Gym_App.Application.Services;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infrastructure.DTOs.Exercise;
using Gym_App.Infrastructure.DTOs.Workout;
using Gym_App.Infrastructure.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace GymApp.Tests.WorkoutTests
{
    public class WorkoutManagementTests : TestBase
    {
        private readonly IWorkoutService _workoutService;
        private readonly Mock<ICachedAuthorizationService> _authorizationService;
        private readonly Mock<IWorkoutNotificationSink> _notificationService;
        private readonly Mock<ILogger<WorkoutService>> _logger;
        private readonly Mock<IUserStatsService> _stats;
        public WorkoutManagementTests() : base("WorkoutTestsDatabase")
        {
            _authorizationService = new Mock<ICachedAuthorizationService>();
            _notificationService = new Mock<IWorkoutNotificationSink>();
            _logger = new Mock<ILogger<WorkoutService>>();
            _stats = new Mock<IUserStatsService>();
            _workoutService = new WorkoutService(_unitOfWork, _authorizationService.Object, _notificationService.Object, _stats.Object, _logger.Object);
        }
        //need to do tests that check for the order of the exercises
        [Fact]
        public async Task WorkoutExerciseUpdate()
        {
            var user = CreateTestUser(CreateTestRole());
            var exercise = CreateTestExercise();
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var sets = new List<WorkoutSetManagementDTO>();
            sets.Add(new WorkoutSetManagementDTO
            {
                SetId = workout.ExerciseInstances.First().Sets.First().Id,
                requestType = ""
            });
            var exercisedto = new ExerciseManagementDTO
            {
                ExerciseId = workout.ExerciseInstances.First().Id,
                requestType = "update",
                IsCompleted = false,
                Sets = sets,
                Name = "",
                PlannedReps = 20,
                PlannedWeight = 20
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.ManageWorkoutExerciseAsync(workout.Id, exercisedto);
            Assert.NotNull(result);
            Assert.Equal("Workout exercise managed successfully", result.msg);

            var workoutresult = await _unitOfWork.Workouts.GetWorkoutById(workout.Id);
            Assert.NotNull(workoutresult);
            Assert.Equal(20, workoutresult.ExerciseInstances.First().PlannedReps);
            Assert.Equal(20, workoutresult.ExerciseInstances.First().PlannedWeight);
        }
        [Fact]
        public async Task WorkoutNotFound()
        {
            var user = CreateTestUser(CreateTestRole());
            var exercise = CreateTestExercise();
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var sets = new List<WorkoutSetManagementDTO>();
            sets.Add(new WorkoutSetManagementDTO
            {
                SetId = workout.ExerciseInstances.First().Sets.First().Id,
                requestType = ""
            });
            var exercisedto = new ExerciseManagementDTO
            {
                ExerciseId = workout.ExerciseInstances.First().Id,
                requestType = "update",
                IsCompleted = false,
                Sets = sets,
                Name = "",
                PlannedReps = 20,
                PlannedWeight = 20
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.ManageWorkoutExerciseAsync(Guid.NewGuid(), exercisedto);
            Assert.NotNull(result);
            Assert.Equal("Workout not found", result.msg);
        }
        [Fact]
        public async Task WorkoutButUserNotAuthorized()
        {
            var user = CreateTestUser(CreateTestRole());
            var exercise = CreateTestExercise();
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var sets = new List<WorkoutSetManagementDTO>();
            sets.Add(new WorkoutSetManagementDTO
            {
                SetId = workout.ExerciseInstances.First().Sets.First().Id,
                requestType = ""
            });
            var exercisedto = new ExerciseManagementDTO
            {
                ExerciseId = workout.ExerciseInstances.First().Id,
                requestType = "update",
                IsCompleted = false,
                Sets = sets,
                Name = "",
                PlannedReps = 20,
                PlannedWeight = 20
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(false);
            var result = await _workoutService.ManageWorkoutExerciseAsync(workout.Id, exercisedto);
            Assert.NotNull(result);
            Assert.Equal("Unauthorized", result.msg);
        }
        [Fact]
        public async Task WorkoutExerciseCreation()
        {
            var user = CreateTestUser(CreateTestRole());
            var exercise = CreateTestExercise("Testing");
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var sets = new List<WorkoutSetManagementDTO>();
            sets.Add(new WorkoutSetManagementDTO
            {
                requestType = "",
                SetNumber = 1,
                RestSeconds = 10,
                ActualReps = 20,
                ActualWeight = 20,
                IsCompleted = true
            });
            var exercisedto = new ExerciseManagementDTO
            {
                ExerciseId = exercise.Id,
                requestType = "create",
                IsCompleted = false,
                Sets = sets,
                Name = "",
                PlannedReps = 20,
                PlannedWeight = 20
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.ManageWorkoutExerciseAsync(workout.Id, exercisedto);
            Assert.NotNull(result);
            Assert.Equal("Workout exercise managed successfully", result.msg);

            var workoutresult = await _unitOfWork.Workouts.GetWorkoutById(workout.Id);
            Assert.NotNull(workoutresult);
            Assert.Equal(20, workoutresult.ExerciseInstances.FirstOrDefault(x => x.Exercise.Name == "Testing").PlannedReps);
            Assert.Equal(20, workoutresult.ExerciseInstances.FirstOrDefault(x => x.Exercise.Name == "Testing").PlannedReps);
        }
        [Fact]
        public async Task ExerciseInvalidRequestType()
        {
            var user = CreateTestUser(CreateTestRole());
            var exercise = CreateTestExercise("Testing");
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var sets = new List<WorkoutSetManagementDTO>();
            sets.Add(new WorkoutSetManagementDTO
            {
                requestType = "",
                SetNumber = 1,
                RestSeconds = 10,
                ActualReps = 20,
                ActualWeight = 20,
                IsCompleted = true
            });
            var exercisedto = new ExerciseManagementDTO
            {
                ExerciseId = Guid.NewGuid(),
                requestType = "Invalid",
                IsCompleted = false,
                Sets = sets,
                Name = "",
                PlannedReps = 20,
                PlannedWeight = 20
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.ManageWorkoutExerciseAsync(workout.Id, exercisedto);
        }
        [Fact]
        public async Task ExerciseCreationButExerciseNotFound()
        {
            var user = CreateTestUser(CreateTestRole());
            var exercise = CreateTestExercise("Testing");
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var sets = new List<WorkoutSetManagementDTO>();
            sets.Add(new WorkoutSetManagementDTO
            {
                requestType = "",
                SetNumber = 1,
                RestSeconds = 10,
                ActualReps = 20,
                ActualWeight = 20,
                IsCompleted = true
            });
            var exercisedto = new ExerciseManagementDTO
            {
                ExerciseId = Guid.NewGuid(),
                requestType = "create",
                IsCompleted = false,
                Sets = sets,
                Name = "",
                PlannedReps = 20,
                PlannedWeight = 20
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.ManageWorkoutExerciseAsync(workout.Id, exercisedto);
            Assert.NotNull(result);
            Assert.Equal("Exercise not found", result.msg);
        }
        [Fact]
        public async Task ExerciseUpdateExerciseNotFound()
        {
            var user = CreateTestUser(CreateTestRole());
            var exercise = CreateTestExercise("Testing");
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var sets = new List<WorkoutSetManagementDTO>();
            sets.Add(new WorkoutSetManagementDTO
            {
                requestType = "",
                SetNumber = 1,
                RestSeconds = 10,
                ActualReps = 20,
                ActualWeight = 20,
                IsCompleted = true
            });
            var exercisedto = new ExerciseManagementDTO
            {
                ExerciseId = Guid.NewGuid(),
                requestType = "update",
                IsCompleted = false,
                Sets = sets,
                Name = "",
                PlannedReps = 20,
                PlannedWeight = 20
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.ManageWorkoutExerciseAsync(workout.Id, exercisedto);
            Assert.NotNull(result);
            Assert.Equal("Exercise instance not found in this workout", result.msg);
        }
        [Fact]
        public async Task ExerciseUpdateSetCreation()
        {
            var user = CreateTestUser(CreateTestRole());
            var exercise = CreateTestExercise("Testing");
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var sets = new List<WorkoutSetManagementDTO>();
            sets.Add(new WorkoutSetManagementDTO
            {
                Notes = "created",
                requestType = "create",
                SetNumber = 1,
                RestSeconds = 10,
                ActualReps = 20,
                ActualWeight = 20,
                IsCompleted = true
            });
            var exercisedto = new ExerciseManagementDTO
            {
                ExerciseId = workout.ExerciseInstances.First().Id,
                requestType = "update",
                IsCompleted = false,
                Sets = sets,
                Name = "",
                PlannedReps = 20,
                PlannedWeight = 20,
                Notes = "created"
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.ManageWorkoutExerciseAsync(workout.Id, exercisedto);
            Assert.NotNull(result);
            Assert.Equal("Workout exercise managed successfully", result.msg);

            var resultWorkout = await _unitOfWork.Workouts.GetWorkoutById(workout.Id);
            Assert.NotNull(resultWorkout);
            var exerciseInstance = resultWorkout.ExerciseInstances.FirstOrDefault(x => x.Notes == "created");
            Assert.NotNull(exerciseInstance);
            //foreach(var set in exerciseInstance.Sets)
            //{
            //    //Assert.NotNull(set);
            //    //if(set.Notes == "created")
            //    //{
            //    //    Assert.Equal(20, set.ActualReps);
            //    //    Assert.Equal(20, set.ActualWeight);
            //    //}
            //    Assert.Equal("Test Notes 1", set.Notes);
            //}
            var set = exerciseInstance.Sets.FirstOrDefault(x => x.Notes == "created");
            Assert.NotNull(set);
            Assert.Equal(20, set.ActualReps);
            Assert.Equal(20, set.ActualWeight);
        }
        [Fact]
        public async Task ExerciseUpdateSetUpdate()
        {
            var user = CreateTestUser(CreateTestRole());
            var exercise = CreateTestExercise("Testing");
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var set = workout.ExerciseInstances.First().Sets.First();
            var sets = new List<WorkoutSetManagementDTO>();
            sets.Add(new WorkoutSetManagementDTO
            {
                SetId = set.Id,
                Notes = "updated",
                requestType = "update",
                SetNumber = 1,
                RestSeconds = 10,
                ActualReps = 20,
                ActualWeight = 20,
                Reps = 40,
                Weight = 40,
                IsCompleted = true
            });
            var exercisedto = new ExerciseManagementDTO
            {
                ExerciseId = workout.ExerciseInstances.First().Id,
                requestType = "update",
                IsCompleted = false,
                Sets = sets,
                Name = "",
                PlannedReps = 20,
                PlannedWeight = 20,
                Notes = "updated"
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.ManageWorkoutExerciseAsync(workout.Id, exercisedto);
            Assert.NotNull(result);
            Assert.Equal("Workout exercise managed successfully", result.msg);
            var resultWorkout = await _unitOfWork.Workouts.GetWorkoutById(workout.Id);
            Assert.NotNull(resultWorkout);
            var exerciseInstance = resultWorkout.ExerciseInstances.FirstOrDefault(x => x.Notes == "updated");
            Assert.NotNull(exerciseInstance);
            var updatedSet = exerciseInstance.Sets.FirstOrDefault(x => x.Id == set.Id);
            Assert.NotNull(updatedSet);
            Assert.Equal(20, updatedSet.ActualReps);
            Assert.Equal(20, updatedSet.ActualWeight);
            Assert.Equal(40, updatedSet.Reps);
            Assert.Equal(40, updatedSet.Weight);
        }
        [Fact]
        public async Task ExerciseSetButSetNotFound()
        {
            var user = CreateTestUser(CreateTestRole());
            var exercise = CreateTestExercise("Testing");
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var sets = new List<WorkoutSetManagementDTO>();
            sets.Add(new WorkoutSetManagementDTO
            {
                SetId = Guid.NewGuid(),
                Notes = "updated",
                requestType = "update",
                SetNumber = 1,
                RestSeconds = 10,
                ActualReps = 20,
                ActualWeight = 20,
                Reps = 40,
                Weight = 40,
                IsCompleted = true
            });
            var exercisedto = new ExerciseManagementDTO
            {
                ExerciseId = workout.ExerciseInstances.First().Id,
                requestType = "update",
                IsCompleted = false,
                Sets = sets,
                Name = "",// leave empty to not update it
                PlannedReps = 0,// leave empty to not update it
                PlannedWeight = 0,// leave empty to not update it
                Notes = ""// leave empty to not update it
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.ManageWorkoutExerciseAsync(workout.Id, exercisedto);
            Assert.NotNull(result);
            Assert.Equal("No changes were made to the workout exercise", result.msg);
        }
        [Fact]
        public async Task ExerciseUpdateButNoChangesMade()
        {
            var user = CreateTestUser(CreateTestRole());
            var exercise = CreateTestExercise("Testing");
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var sets = new List<WorkoutSetManagementDTO>();
            sets.Add(new WorkoutSetManagementDTO
            {
                SetId = workout.ExerciseInstances.First().Sets.First().Id,
                Notes = workout.ExerciseInstances.First().Sets.First().Notes,
                requestType = "update",
                SetNumber = workout.ExerciseInstances.First().Sets.First().SetNumber,
                RestSeconds = workout.ExerciseInstances.First().Sets.First().RestSeconds,
                ActualReps = workout.ExerciseInstances.First().Sets.First().ActualReps,
                ActualWeight = workout.ExerciseInstances.First().Sets.First().ActualWeight,
                Reps = workout.ExerciseInstances.First().Sets.First().Reps,
                Weight = workout.ExerciseInstances.First().Sets.First().Weight,
                IsCompleted = workout.ExerciseInstances.First().Sets.First().IsCompleted
            });
            var exercisedto = new ExerciseManagementDTO
            {
                ExerciseId = workout.ExerciseInstances.First().Id,
                requestType = "update",
                IsCompleted = workout.ExerciseInstances.First().IsCompleted,
                Sets = sets,
                Name = workout.ExerciseInstances.First().Exercise.Name,
                PlannedReps = (int)workout.ExerciseInstances.First().PlannedReps,
                PlannedWeight = (int)workout.ExerciseInstances.First().PlannedWeight,
                Notes = workout.ExerciseInstances.First().Notes
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.ManageWorkoutExerciseAsync(workout.Id, exercisedto);
            Assert.NotNull(result);
            Assert.Equal("No changes were made to the workout exercise", result.msg);
        }
        [Fact]
        public async Task ExerciseSetDelete()
        {
            var user = CreateTestUser(CreateTestRole());
            var exercise = CreateTestExercise("Testing");
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var sets = new List<WorkoutSetManagementDTO>();
            var setId = workout.ExerciseInstances.First().Sets.First().Id;
            sets.Add(new WorkoutSetManagementDTO
            {
                SetId = setId,
                Notes = "",
                requestType = "delete",
                SetNumber = 1,
                RestSeconds = 10,
                ActualReps = 20,
                ActualWeight = 20,
                Reps = 40,
                Weight = 40,
                IsCompleted = true
            });
            var exercisedto = new ExerciseManagementDTO
            {
                ExerciseId = workout.ExerciseInstances.First().Id,
                requestType = "update",
                IsCompleted = false,
                Sets = sets,
                Name = "",// leave empty to not update it
                PlannedReps = 0,// leave empty to not update it
                PlannedWeight = 0,// leave empty to not update it
                Notes = ""// leave empty to not update it
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.ManageWorkoutExerciseAsync(workout.Id, exercisedto);
            Assert.NotNull(result);
            Assert.Equal("Workout exercise managed successfully", result.msg);

            var resultWorkout = await _unitOfWork.Workouts.GetWorkoutById(workout.Id);
            Assert.NotNull(resultWorkout);
            var exerciseInstance = resultWorkout.ExerciseInstances.FirstOrDefault(x => x.Id == workout.ExerciseInstances.First().Id);
            Assert.NotNull(exerciseInstance);
            var isSetDeleted = !exerciseInstance.Sets.Any(x => x.Id == setId);
            Assert.True(isSetDeleted);
        }
    }
}
