using Gym_App.Application.Authorization;
using Gym_App.Application.Services;
using Gym_App.Domain;
using Gym_App.Infastructure.DTOs.WorkoutDTOs;
using Gym_App.Infastructure.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Moq;
using System.Security.Claims;

namespace GymApp.Tests.WorkoutTests
{
    public class WorkoutQueryTests : TestBase
    {
        private readonly IWorkoutService _workoutService;
        private readonly Mock<ICachedAuthorizationService> _authorizationService;
        public WorkoutQueryTests() : base("WorkoutTestsDatabase")
        {
            _authorizationService = new Mock<ICachedAuthorizationService>();
            _workoutService = new WorkoutService(_unitOfWork, _authorizationService.Object);
        }
        [Fact]
        public async Task GetWorkoutByIdTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
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
            var workout = CreateTestWorkout(user);
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
            await _workoutService.AddExercisesToWorkout(workout.Id, workoutExercises);
            var NameResult = await _workoutService.GetExercisesOfWorkout(workout.Id, 1, "", "", "Test Exercise 3", 10);
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
            var workout = CreateTestWorkout(user);
            List<Exercise> exercises = new List<Exercise>();
            for (int i = 0; i < 5; i++)
            {
                exercises.Add(CreateTestExercise(
                    $"Test Exercise {i}", $"This is a test exercise {i}",DateTime.Now.AddDays(i)));
            }
            await _unitOfWork.SaveChangesAsync();
            var workoutExercises = new WorkoutExerciseDTO
            {
                ExercisesID = exercises.Select(e => e.Id).ToList()
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            await _workoutService.AddExercisesToWorkout(workout.Id, workoutExercises);

            var NameAscResult = await _workoutService.GetExercisesOfWorkout(workout.Id, 1, "Name", "asc", "", 10);
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
                var workout = CreateTestWorkout(user, $"Test Workout {i}", $"This is a test workout {i}");
            }
            await _unitOfWork.SaveChangesAsync();
            var result = await _workoutService.GetAllWorkouts(1, 10);
            Assert.NotNull(result);
            Assert.Equal(5, result.Data!.TotalCount);
        }
    }
}
