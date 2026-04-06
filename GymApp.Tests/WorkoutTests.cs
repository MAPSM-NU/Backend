

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

        //Creating workout with all its possibilities of failure
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
        public async Task CreateWorkoutWithNonExistingUserTest()
        {
            var workout = new WorkoutCreationDTO
            {
                Name = "Test Workout",
                Description = "This is a test workout",
                UserID = Guid.NewGuid(), // Non-existing user ID
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
            Assert.Equal("User not found", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task CreateWorkoutWithUnauthorizedUserTest()
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
                .ReturnsAsync(AuthorizationResult.Failed());
            var result = await _workoutService.CreateWorkout(new ClaimsPrincipal(), workout);
            Assert.NotNull(result);
            Assert.Equal("Forbidden from access", result.msg);
            Assert.Equal(1, result.status);
        }
        [Fact]
        public async Task CreateWorkoutWithInvalidDataTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = new WorkoutCreationDTO
            {
                Name = "", // Invalid name
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
            Assert.Equal("Invalid workout data", result.msg);
            Assert.Equal(0, result.status);
        }

        //Updating workout with all its possibilities of failure
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
        public async Task UpdateNonExistingWorkoutTest()
        {
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
            var result = await _workoutService.UpdateWorkout(new ClaimsPrincipal(), Guid.NewGuid(), workoutUpdate);
            Assert.NotNull(result);
            Assert.Equal("Workout not found", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task UpdateWorkoutWithUnauthorizedUserTest()
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
                .ReturnsAsync(AuthorizationResult.Failed());
            var result = await _workoutService.UpdateWorkout(new ClaimsPrincipal(), workout.Id, workoutUpdate);
            Assert.NotNull(result);
            Assert.Equal("Forbidden from access", result.msg);
            Assert.Equal(1, result.status);
        }
        [Fact]
        public async Task UpdateWorkoutWithInvalidDataTest()
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
            var workoutUpdate = new WorkoutUpdateDTO
            {
                Name = "test name", 
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
            var result = await _workoutService.UpdateWorkout(new ClaimsPrincipal(), Guid.Empty, workoutUpdate);//Passing an empty id to check If it will pass validation checks
            Assert.NotNull(result);
            Assert.Equal("Invalid workout data", result.msg);
            Assert.Equal(0, result.status);
        }

        //Deleting workout with all its possibilities of failure
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
        public async Task DeleteNonExistingWorkoutTest()
        {
            _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());
            var result = await _workoutService.DeleteWorkout(new ClaimsPrincipal(), Guid.NewGuid());
            Assert.NotNull(result);
            Assert.Equal("Workout not found", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task DeleteWorkoutWithUnauthorizedUserTest()
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
                .ReturnsAsync(AuthorizationResult.Failed());
            var result = await _workoutService.DeleteWorkout(new ClaimsPrincipal(), workout.Id);
            Assert.NotNull(result);
            Assert.Equal("Forbidden from access", result.msg);
            Assert.Equal(1, result.status);
        }
        [Fact]
        public async Task DeleteWorkoutWithInvalidIdTest()
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
            var result = await _workoutService.DeleteWorkout(new ClaimsPrincipal(), Guid.Empty);//Passing an empty id to check If it will pass validation checks
            Assert.NotNull(result);
            Assert.Equal("Invalid workout ID", result.msg);
            Assert.Equal(0, result.status);
        }

        //Adding workout's exercises with all their possibilities of failure
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
        public async Task AddExercisesToWorkoutWithNonExistingWorkoutTest()
        {
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
            var result = await _workoutService.AddExercisesToWorkout(new ClaimsPrincipal(), Guid.NewGuid(), workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("Workout not found", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task AddExercisesToWorkoutWithUnauthorizedUserTest()
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
                .ReturnsAsync(AuthorizationResult.Failed());
            var result = await _workoutService.AddExercisesToWorkout(new ClaimsPrincipal(), workout.Id, workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("Forbidden from access", result.msg);
            Assert.Equal(1, result.status);
        }
        [Fact]
        public async Task AddExercisesToWorkoutWithInvalidDataTest()
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
            var workoutExercises = new WorkoutExerciseDTO
            {
                ExercisesID = null 
            };
             _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());
            var result = await _workoutService.AddExercisesToWorkout(new ClaimsPrincipal(), Guid.Empty, workoutExercises);//Passing an empty id to check If it will pass validation checks
            Assert.NotNull(result);
            Assert.Equal("Invalid data", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task AddExercisesThatAreAlreadyInWorkoutTest()
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
            await _workoutService.AddExercisesToWorkout(new ClaimsPrincipal(), workout.Id, workoutExercises);
            var result = await _workoutService.AddExercisesToWorkout(new ClaimsPrincipal(), workout.Id, workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("No new exercises to add", result.msg);
            Assert.Equal(0, result.status);
        }
       
        //setting exercies with all posibilities of failure
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
        public async Task SetExercisesToWorkoutWithNonExistingWorkoutTest()
        {
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
            var result = await _workoutService.SetExercisesOfWorkout(new ClaimsPrincipal(), Guid.NewGuid(), workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("Workout not found", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task SetExercisesToWorkoutWithUnauthorizedUserTest()
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
                .ReturnsAsync(AuthorizationResult.Failed());
            var result = await _workoutService.SetExercisesOfWorkout(new ClaimsPrincipal(), workout.Id, workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("Forbidden from access", result.msg);
            Assert.Equal(1, result.status);
        }
        public async Task SetExercisesToWorkoutWithInvalidDataTest()
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
            var workoutExercises = new WorkoutExerciseDTO
            {
                ExercisesID = null 
            };
             _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());
            var result = await _workoutService.SetExercisesOfWorkout(new ClaimsPrincipal(), Guid.Empty, workoutExercises);//Passing an empty id to check If it will pass validation checks
            Assert.NotNull(result);
            Assert.Equal("Invalid data", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task SettingExerciesWithWrongIdsTest()
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
            var workoutExercises = new WorkoutExerciseDTO
            {
                ExercisesID = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() } // Non-existing exercise IDs
            };
             _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());
            var result = await _workoutService.SetExercisesOfWorkout(new ClaimsPrincipal(), workout.Id, workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("No exercises found", result.msg);
            Assert.Equal(0, result.status);
        }

        //Deleting workout's exercises with all posibilities of failure
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
        public async Task RemoveExercisesFromWorkoutWithNonExistingWorkoutTest()
        {
            var workoutExercises = new WorkoutExerciseDTO
            {
                ExercisesID = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() } // Non-existing exercise IDs
            };
             _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());
            var result = await _workoutService.DeleteExercisesFromWorkout(new ClaimsPrincipal(), Guid.NewGuid(), workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("Workout not found", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task RemoveExercisesFromWorkoutWithUnauthorizedUserTest()
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
            var workoutExercises = new WorkoutExerciseDTO
            {
                ExercisesID = exercises.Select(e => e.Id).ToList()
            };
             _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Failed());
            var result = await _workoutService.DeleteExercisesFromWorkout(new ClaimsPrincipal(), workout.Id, workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("Forbidden from access", result.msg);
            Assert.Equal(1, result.status);
        }
        public async Task RemoveExercisesFromWorkoutWithInvalidDataTest()
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
            var workoutExercises = new WorkoutExerciseDTO
            {
                ExercisesID = null 
            };
             _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());
            var result = await _workoutService.DeleteExercisesFromWorkout(new ClaimsPrincipal(), Guid.Empty, workoutExercises);//Passing an empty id to check If it will pass validation checks
            Assert.NotNull(result);
            Assert.Equal("Invalid data", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task RemoveExercisesThatAreNotInWorkoutTest()
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
            var workoutExercises = new WorkoutExerciseDTO
            {
                ExercisesID = exercises.Select(e => e.Id).ToList()
            };
             _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());
            var result = await _workoutService.DeleteExercisesFromWorkout(new ClaimsPrincipal(), workout.Id, workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("No exercises to remove", result.msg);
            Assert.Equal(0, result.status);
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
        public async Task GetWorkoutByIdWithInvalidIdTest()
        {
            var result = await _workoutService.GetWorkoutByID(Guid.NewGuid());
            Assert.NotNull(result);
            Assert.Null(result.Value);
            Assert.Equal("Not Found", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task GetExercisesOfWorkoutSearchTest()
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
            var workoutExercises = new WorkoutExerciseDTO
            {
                ExercisesID = exercises.Select(e => e.Id).ToList()
            };
             _authorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());
            await _workoutService.AddExercisesToWorkout(new ClaimsPrincipal(), workout.Id, workoutExercises);
            var NameResult = await _workoutService.GetExercisesOfWorkout(workout.Id, 1,"","","Test Exercise 3", 10);
            Assert.NotNull(NameResult);
            Assert.Equal(1, NameResult.Data!.TotalCount);

            var DescriptionResult = await _workoutService.GetExercisesOfWorkout(workout.Id, 1, "", "", "test exercise", 10);
            Assert.NotNull(DescriptionResult);
            Assert.Equal(5, DescriptionResult.Data!.TotalCount);
            
            var CategoryResult = await _workoutService.GetExercisesOfWorkout(workout.Id, 1, "", "Stren", "", 10);
            Assert.NotNull(CategoryResult);
            Assert.Equal(5, CategoryResult.Data!.TotalCount);
            
            var DifficultyResult = await _workoutService.GetExercisesOfWorkout(workout.Id, 1, "Medium", "", "", 10);
            Assert.NotNull(DifficultyResult);
            Assert.Equal(5, DifficultyResult.Data!.TotalCount);
        }
        [Fact]
        public async Task GetExerciesOfWorkoutWithSortingTest()
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
                    CreatedAt = DateTime.Now.AddDays(i) // Setting CreatedAt to different values for sorting by date
                });
                await _unitOfWork.Exercises!.Create(exercises[i]);
            }
            await _unitOfWork.SaveChangesAsync();
            var workoutExercises = new WorkoutExerciseDTO
            {
                ExercisesID = exercises.Select(e => e.Id).ToList()
            };
            _authorizationService.Setup(x => x.AuthorizeAsync(
               It.IsAny<ClaimsPrincipal>(),
               It.IsAny<object>(),
               It.IsAny<string>()))
               .ReturnsAsync(AuthorizationResult.Success());
            await _workoutService.AddExercisesToWorkout(new ClaimsPrincipal(), workout.Id, workoutExercises);
            
            var NameAscResult = await _workoutService.GetExercisesOfWorkout(workout.Id, 1,"Name", "asc", "", 10);
            Assert.NotNull(NameAscResult);
            Assert.Equal("Test Exercise 0", NameAscResult.Data!.Items[0].Name);
            Assert.Equal(5, NameAscResult.Data!.TotalCount);

            var NameDescResult = await _workoutService.GetExercisesOfWorkout(workout.Id, 1, "Name", "desc", "", 10);
            Assert.NotNull(NameDescResult);
            Assert.Equal("Test Exercise 4", NameDescResult.Data!.Items[0].Name);
            Assert.Equal(5, NameDescResult.Data!.TotalCount);

            var DateAscResult = await _workoutService.GetExercisesOfWorkout(workout.Id, 1, "Date", "asc", "", 10);
            Assert.NotNull(DateAscResult);
            Assert.Equal("Test Exercise 0", DateAscResult.Data!.Items[0].Name);
            Assert.Equal(5, DateAscResult.Data!.TotalCount);

            var DateDescResult = await _workoutService.GetExercisesOfWorkout(workout.Id, 1, "Date", "d", "", 10);
            Assert.NotNull(DateDescResult);
            Assert.Equal("Test Exercise 4", DateDescResult.Data!.Items[0].Name);
            Assert.Equal(5, DateDescResult.Data!.TotalCount);
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
