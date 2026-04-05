

using Gym_App.Application.Services;
using Gym_App.Domain;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.DTOs.WorkoutDTOs;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infastructure.Repositries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;

namespace GymApp.Tests
{
    public class WorkoutTests : TestBase
    {
        private readonly IWorkoutService _workoutService;
        private readonly Mock<IAuthorizationService> _authorizationService;
        public WorkoutTests() : base("WorkoutTestsDatabase")
        {
            _authorizationService = new Mock<IAuthorizationService>();
            _workoutService = new WorkoutService(_unitOfWork, _authorizationService.Object);
        }
        [Fact]
        public async Task CreateWorkoutTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = new WorkoutCreationDTO
            {
                Name = "Test Workout",
                Description = "This is a test workout",
                UserID = user.Id,
                Date = DateTime.Now,
                Day = "Monday",
                Difficulty = "Medium"
            };
            _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());
            var result = await _workoutService.CreateWorkout(new ClaimsPrincipal(), workout);
            Assert.NotNull(result);
            Assert.Equal("Workout created successfully", result.msg);
            Assert.Equal(2, result.status);
        }
        [Fact]
        public async Task UpdateWorkoutTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workoutId = Guid.NewGuid();
            var workout = new Workout
            {
                Name = "Test Workout",
                Description = "This is a test workout",
                User = user,
                Date = DateTime.Now,
                Day = "Monday",
                Difficulty = "Medium",
                UpdatedAt = DateTime.Now,
            };
            await _unitOfWork.Workouts.Create(workout);
            await _unitOfWork.SaveChangesAsync();
            var workoutUpdate = new WorkoutUpdateDTO
            {
                Name = "Updated Workout",
                Description = "This is an updated test workout",
                Date = DateTime.Now.AddDays(1),
                Day = "Tuesday",
                Difficulty = "Hard",
            };
            _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());
            var result = await _workoutService.UpdateWorkout(new ClaimsPrincipal(), workout.Id, workoutUpdate);
            Assert.NotNull(result);
            Assert.Equal("Workout updated successfully", result.msg);
            Assert.Equal(2, result.status);
        }
        [Fact]
        public async Task DeleteWorkoutTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = new Workout
            {
                Name = "Test Workout",
                Description = "This is a test workout",
                User = user,
                Date = DateTime.Now,
                Day = "Monday",
                Difficulty = "Medium",
                UpdatedAt = DateTime.Now,
            };
            await _unitOfWork.Workouts.Create(workout);
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());
            var result = await _workoutService.DeleteWorkout(new ClaimsPrincipal(), workout.Id);
            Assert.NotNull(result);
            Assert.Equal("Workout deleted successfully", result.msg);
            Assert.Equal(2, result.status);
        }
        [Fact]
        public async Task AddExercisesToWorkoutTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = new Workout
            {
                Name = "Test Workout",
                Description = "This is a test workout",
                User = user,
                Date = DateTime.Now,
                Day = "Monday",
                Difficulty = "Medium",
                UpdatedAt = DateTime.Now,
            };
            await _unitOfWork.Workouts.Create(workout);
            await _unitOfWork.SaveChangesAsync();

            List<Exercise> exercises = new List<Exercise>();
            for (int i = 0; i < 5; i++)
            {
                exercises.Add( new Exercise
                {
                    Name = $"Test Exercise {i}",
                    Description = $"This is a test exercise {i}",
                    VideoUrl = $"https://www.youtube.com/watch?v=example{i}",
                    UpdatedAt = DateTime.Now,
                    Category = "Strength",
                    Id = Guid.NewGuid(),
                    Difficulty = "Medium",
                    Grip = "Overhand",
                });
                await _unitOfWork.Exercises!.Create(exercises[i]);
            }
            await _unitOfWork.SaveChangesAsync();

            var exerciesIds = exercises.Select(e => e.Id).ToList();
            var workoutExercises = new WorkoutExerciseDTO
            {
                ExercisesID = exerciesIds
            };
             _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());

            var result = await _workoutService.AddExercisesToWorkout(new ClaimsPrincipal(), workout.Id, workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("Exercises added successfully", result.msg);
            Assert.Equal(2, result.status);

            var workoutFromDb = await _unitOfWork.Workouts.GetWorkoutById(workout.Id);
            Assert.NotNull(workoutFromDb);
            Assert.NotNull(workoutFromDb.Exercises);
            Assert.Equal(5, workoutFromDb.Exercises.Count);
        }
        [Fact]
        public async Task SetExercisesToWorkoutTest()// the mission of the set function is to replace all the workout's exercises with the new given ones
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = new Workout
            {
                Name = "Test Workout",
                Description = "This is a test workout",
                User = user,
                Date = DateTime.Now,
                Day = "Monday",
                Difficulty = "Medium",
                UpdatedAt = DateTime.Now,
            };
            await _unitOfWork.Workouts.Create(workout);
            workout.Exercises!.Add(new Exercise
            {
                Name = $"Test Exercise",
                Description = $"This is a test exercise",
                VideoUrl = $"https://www.youtube.com/watch?v=example",
                UpdatedAt = DateTime.Now,
                Category = "Strength",
                Id = Guid.NewGuid(),
                Difficulty = "Medium",
                Grip = "Overhand",
            });
            await _unitOfWork.SaveChangesAsync();
            List<Exercise> exercises = new List<Exercise>();
            for (int i = 0; i < 5; i++)
            {
                exercises.Add(new Exercise
                {
                    Name = $"Test Exercise {i}",
                    Description = $"This is a test exercise {i}",
                    VideoUrl = $"https://www.youtube.com/watch?v=example{i}",
                    UpdatedAt = DateTime.Now,
                    Category = "Strength",
                    Id = Guid.NewGuid(),
                    Difficulty = "Medium",
                    Grip = "Overhand",
                });
                await _unitOfWork.Exercises!.Create(exercises[i]);
            }
            await _unitOfWork.SaveChangesAsync();
            var exerciesIds = exercises.Select(e => e.Id).ToList();
            var workoutExercises = new WorkoutExerciseDTO
            {
                ExercisesID = exerciesIds
            };
             _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());
            var result = await _workoutService.SetExercisesOfWorkout(new ClaimsPrincipal(), workout.Id, workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("Exercises set successfully", result.msg);
            Assert.Equal(2, result.status);
            var workoutFromDb = await _unitOfWork.Workouts.GetWorkoutById(workout.Id);
            Assert.NotNull(workoutFromDb);
            Assert.NotNull(workoutFromDb.Exercises);
            Assert.Equal(5, workoutFromDb.Exercises.Count);
        }
        [Fact]
        public async Task RemoveExerciseFromWorkoutTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = new Workout
            {
                Name = "Test Workout",
                Description = "This is a test workout",
                User = user,
                Date = DateTime.Now,
                Day = "Monday",
                Difficulty = "Medium",
                UpdatedAt = DateTime.Now,
            };
            await _unitOfWork.Workouts.Create(workout);
            //await _unitOfWork.SaveChangesAsync();

            List<Exercise> exercises = new List<Exercise>();
            for (int i = 0; i < 5; i++)
            {
                exercises.Add(new Exercise
                {
                    Name = $"Test Exercise {i}",
                    Description = $"This is a test exercise {i}",
                    VideoUrl = $"https://www.youtube.com/watch?v=example{i}",
                    UpdatedAt = DateTime.Now,
                    Category = "Strength",
                    Id = Guid.NewGuid(),
                    Difficulty = "Medium",
                    Grip = "Overhand",
                });
                await _unitOfWork.Exercises!.Create(exercises[i]);
            }
            await _unitOfWork.SaveChangesAsync();

            _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());
            var workoutExercises = new WorkoutExerciseDTO
            {
                ExercisesID = exercises.Select(e => e.Id).ToList()
            };
            var addResult = await _workoutService.AddExercisesToWorkout(new ClaimsPrincipal(), workout.Id, workoutExercises);
            var deleteResult = await _workoutService.DeleteExercisesFromWorkout(new ClaimsPrincipal(), workout.Id, workoutExercises);
            Assert.NotNull(deleteResult);
            Assert.Equal("Exercises removed successfully", deleteResult.msg);
            Assert.Equal(2, deleteResult.status);
            var workoutFromDb = await _unitOfWork.Workouts.GetWorkoutById(workout.Id);
            Assert.NotNull(workoutFromDb);
            Assert.NotNull(workoutFromDb.Exercises);
            Assert.Empty(workoutFromDb.Exercises);
        }
        [Fact]
        public async Task GetWorkoutByIdTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = new Workout
            {
                Name = "Test Workout",
                Description = "This is a test workout",
                User = user,
                Date = DateTime.Now,
                Day = "Monday",
                Difficulty = "Medium",
                UpdatedAt = DateTime.Now,
            };
            await _unitOfWork.Workouts.Create(workout);
            await _unitOfWork.SaveChangesAsync();
            var result = await _workoutService.GetWorkoutByID(workout.Id);
            Assert.NotNull(result);
            Assert.Equal(workout.Name, result.Value!.Name);
            Assert.Equal(workout.Description, result.Value.Description);
            Assert.Equal(workout.User.Id, result.Value.UserID);
            Assert.Equal(workout.Date, result.Value.Date);
            Assert.Equal(workout.Day, result.Value.Day);
            Assert.Equal(workout.Difficulty, result.Value.Difficulty);
        }
        [Fact]
        public async Task GetAllWorkoutsTest()
        {
            var user = CreateTestUser(CreateTestRole());
            for (int i = 0; i < 5; i++)
            {
                var workout = new Workout
                {
                    Name = $"Test Workout {i}",
                    Description = $"This is a test workout {i}",
                    User = user,
                    Date = DateTime.Now.AddDays(i),
                    Day = "Monday",
                    Difficulty = "Medium",
                    UpdatedAt = DateTime.Now,
                };
                await _unitOfWork.Workouts.Create(workout);
            }
            await _unitOfWork.SaveChangesAsync();
            var result = await _workoutService.GetAllWorkouts(1,10);
            Assert.NotNull(result);
            Assert.Equal(5, result.Data!.TotalCount);
        }
    }
}
