using Gym_App.Application.Authorization;
using Gym_App.Application.Services;
using Gym_App.Domain;
using Gym_App.Infastructure.DTOs.WorkoutDTOs;
using Gym_App.Infastructure.Interfaces.Services;
using Moq;

namespace GymApp.Tests.WorkoutTests
{
    public class WorkoutExerciseManagementTests : TestBase
    {
        private readonly IWorkoutService _workoutService;
        private readonly Mock<ICachedAuthorizationService> _authorizationService;
        public WorkoutExerciseManagementTests() : base("WorkoutTestsDatabase")
        {
            _authorizationService = new Mock<ICachedAuthorizationService>();
            _workoutService = new WorkoutService(_unitOfWork, _authorizationService.Object);
        }
        //Adding workout's exercises with all their possibilities of failure
        [Fact]
        public async Task AddExercisesToWorkoutTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();

            List<Exercise> exercises = new List<Exercise>();
            for (int i = 0; i < 5; i++)
            {
                exercises.Add(CreateTestExercise(
                    $"Test Exercise {i}", $"This is a test exercise {i}"));
            }
            await _unitOfWork.SaveChangesAsync();

            var exerciesIds = exercises.Select(e => e.Id).ToList();
            var workoutExercises = new WorkoutExerciseDTO
            {
                ExercisesID = exerciesIds
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            var result = await _workoutService.AddExercisesToWorkout(workout.Id, workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("Exercises added successfully", result.msg);
            Assert.Equal(2, result.status);

            var workoutFromDb = await _unitOfWork.Workouts.GetWorkoutById(workout.Id);
            Assert.NotNull(workoutFromDb);
            Assert.NotNull(workoutFromDb.Exercises);
            Assert.Equal(5, workoutFromDb.Exercises.Count);
        }
        [Fact]
        public async Task AddExercisesToWorkoutWithNonExistingWorkoutTest()
        {
            List<Exercise> exercises = new List<Exercise>();
            for (int i = 0; i < 5; i++)
            {
                exercises.Add(CreateTestExercise(
                    $"Test Exercise {i}", $"This is a test exercise {i}"));
            }
            await _unitOfWork.SaveChangesAsync();
            var exerciesIds = exercises.Select(e => e.Id).ToList();
            var workoutExercises = new WorkoutExerciseDTO
            {
                ExercisesID = exerciesIds
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.AddExercisesToWorkout(Guid.NewGuid(), workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("Workout not found", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task AddExercisesToWorkoutWithUnauthorizedUserTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            List<Exercise> exercises = new List<Exercise>();
            for (int i = 0; i < 5; i++)
            {
                exercises.Add(CreateTestExercise(
                    $"Test Exercise {i}", $"This is a test exercise {i}"));
            }
            await _unitOfWork.SaveChangesAsync();
            var exerciesIds = exercises.Select(e => e.Id).ToList();
            var workoutExercises = new WorkoutExerciseDTO
            {
                ExercisesID = exerciesIds
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(false);
            var result = await _workoutService.AddExercisesToWorkout(workout.Id, workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("Forbidden from access", result.msg);
            Assert.Equal(1, result.status);
        }
        [Fact]
        public async Task AddExercisesToWorkoutWithInvalidDataTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var workoutExercises = new WorkoutExerciseDTO
            {
                ExercisesID = null
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.AddExercisesToWorkout(Guid.Empty, workoutExercises);//Passing an empty id to check If it will pass validation checks
            Assert.NotNull(result);
            Assert.Equal("Invalid data", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task AddExercisesThatAreAlreadyInWorkoutTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.Workouts.Create(workout);
            await _unitOfWork.SaveChangesAsync();
            List<Exercise> exercises = new List<Exercise>();
            for (int i = 0; i < 5; i++)
            {
                exercises.Add(CreateTestExercise(
                    $"Test Exercise {i}", $"This is a test exercise {i}"));
            }
            await _unitOfWork.SaveChangesAsync();
            var exerciesIds = exercises.Select(e => e.Id).ToList();
            var workoutExercises = new WorkoutExerciseDTO
            {
                ExercisesID = exerciesIds
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            await _workoutService.AddExercisesToWorkout(workout.Id, workoutExercises);
            var result = await _workoutService.AddExercisesToWorkout(workout.Id, workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("No new exercises to add", result.msg);
            Assert.Equal(0, result.status);
        }

        //setting exercies with all posibilities of failure
        [Fact]
        public async Task SetExercisesToWorkoutTest()// the mission of the set function is to replace all the workout's exercises with the new given ones
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.Workouts.Create(workout);
            workout.Exercises!.Add(CreateTestExercise());
            await _unitOfWork.SaveChangesAsync();
            List<Exercise> exercises = new List<Exercise>();
            for (int i = 0; i < 5; i++)
            {
                exercises.Add(CreateTestExercise(
                    $"Test Exercise {i}", $"This is a test exercise {i}"));
            }
            await _unitOfWork.SaveChangesAsync();
            var exerciesIds = exercises.Select(e => e.Id).ToList();
            var workoutExercises = new WorkoutExerciseDTO
            {
                ExercisesID = exerciesIds
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.SetExercisesOfWorkout(workout.Id, workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("Exercises set successfully", result.msg);
            Assert.Equal(2, result.status);
            var workoutFromDb = await _unitOfWork.Workouts.GetWorkoutById(workout.Id);
            Assert.NotNull(workoutFromDb);
            Assert.NotNull(workoutFromDb.Exercises);
            Assert.Equal(5, workoutFromDb.Exercises.Count);
        }
        [Fact]
        public async Task SetExercisesToWorkoutWithNonExistingWorkoutTest()
        {
            List<Exercise> exercises = new List<Exercise>();
            for (int i = 0; i < 5; i++)
            {
                exercises.Add(CreateTestExercise(
                    $"Test Exercise {i}", $"This is a test exercise {i}"));
            }
            await _unitOfWork.SaveChangesAsync();
            var exerciesIds = exercises.Select(e => e.Id).ToList();
            var workoutExercises = new WorkoutExerciseDTO
            {
                ExercisesID = exerciesIds
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.SetExercisesOfWorkout(Guid.NewGuid(), workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("Workout not found", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task SetExercisesToWorkoutWithUnauthorizedUserTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            List<Exercise> exercises = new List<Exercise>();
            for (int i = 0; i < 5; i++)
            {
                exercises.Add(CreateTestExercise(
                    $"Test Exercise {i}", $"This is a test exercise {i}"));
            }
            await _unitOfWork.SaveChangesAsync();
            var exerciesIds = exercises.Select(e => e.Id).ToList();
            var workoutExercises = new WorkoutExerciseDTO
            {
                ExercisesID = exerciesIds
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(false);
            var result = await _workoutService.SetExercisesOfWorkout(workout.Id, workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("Forbidden from access", result.msg);
            Assert.Equal(1, result.status);
        }
        [Fact]
        public async Task SetExercisesToWorkoutWithInvalidDataTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var workoutExercises = new WorkoutExerciseDTO
            {
                ExercisesID = null
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.SetExercisesOfWorkout(Guid.Empty, workoutExercises);//Passing an empty id to check If it will pass validation checks
            Assert.NotNull(result);
            Assert.Equal("Invalid data", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task SettingExerciesWithWrongIdsTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var workoutExercises = new WorkoutExerciseDTO
            {
                ExercisesID = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() } // Non-existing exercise IDs
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.SetExercisesOfWorkout(workout.Id, workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("No exercises found", result.msg);
            Assert.Equal(0, result.status);
        }

        //Deleting workout's exercises with all posibilities of failure
        [Fact]
        public async Task RemoveExerciseFromWorkoutTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);

            List<Exercise> exercises = new List<Exercise>();
            for (int i = 0; i < 5; i++)
            {
                exercises.Add(CreateTestExercise(
                    $"Test Exercise {i}", $"This is a test exercise {i}"));
            }
            await _unitOfWork.SaveChangesAsync();

            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var workoutExercises = new WorkoutExerciseDTO
            {
                ExercisesID = exercises.Select(e => e.Id).ToList()
            };
            var addResult = await _workoutService.AddExercisesToWorkout(workout.Id, workoutExercises);
            var deleteResult = await _workoutService.DeleteExercisesFromWorkout(workout.Id, workoutExercises);
            Assert.NotNull(deleteResult);
            Assert.Equal("Exercises removed successfully", deleteResult.msg);
            Assert.Equal(2, deleteResult.status);
            var workoutFromDb = await _unitOfWork.Workouts.GetWorkoutById(workout.Id);
            Assert.NotNull(workoutFromDb);
            Assert.NotNull(workoutFromDb.Exercises);
            Assert.Empty(workoutFromDb.Exercises);
        }
        [Fact]
        public async Task RemoveExercisesFromWorkoutWithNonExistingWorkoutTest()
        {
            var workoutExercises = new WorkoutExerciseDTO
            {
                ExercisesID = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() } // Non-existing exercise IDs
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.DeleteExercisesFromWorkout(Guid.NewGuid(), workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("Workout not found", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task RemoveExercisesFromWorkoutWithUnauthorizedUserTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            List<Exercise> exercises = new List<Exercise>();
            for (int i = 0; i < 5; i++)
            {
                exercises.Add(CreateTestExercise(
                    $"Test Exercise {i}", $"This is a test exercise {i}"));
            }
            await _unitOfWork.SaveChangesAsync();
            var workoutExercises = new WorkoutExerciseDTO
            {
                ExercisesID = exercises.Select(e => e.Id).ToList()
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(false);
            var result = await _workoutService.DeleteExercisesFromWorkout(workout.Id, workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("Forbidden from access", result.msg);
            Assert.Equal(1, result.status);
        }
        [Fact]
        public async Task RemoveExercisesFromWorkoutWithInvalidDataTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var workoutExercises = new WorkoutExerciseDTO
            {
                ExercisesID = null
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.DeleteExercisesFromWorkout(Guid.Empty, workoutExercises);//Passing an empty id to check If it will pass validation checks
            Assert.NotNull(result);
            Assert.Equal("Invalid data", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task RemoveExercisesThatAreNotInWorkoutTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.Workouts.Create(workout);
            await _unitOfWork.SaveChangesAsync();
            List<Exercise> exercises = new List<Exercise>();
            for (int i = 0; i < 5; i++)
            {
                exercises.Add(CreateTestExercise(
                    $"Test Exercise {i}", $"This is a test exercise {i}"));
            }
            await _unitOfWork.SaveChangesAsync();
            var workoutExercises = new WorkoutExerciseDTO
            {
                ExercisesID = exercises.Select(e => e.Id).ToList()
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.DeleteExercisesFromWorkout(workout.Id, workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("No exercises to remove", result.msg);
            Assert.Equal(0, result.status);
        }
    }
}
