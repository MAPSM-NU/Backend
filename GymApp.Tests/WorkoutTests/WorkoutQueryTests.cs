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
    public class WorkoutQueryTests : TestBase
    {
        private readonly IWorkoutService _workoutService;
        private readonly Mock<ICachedAuthorizationService> _authorizationService;
        private readonly Mock<IWorkoutNotificationSink> _notificationService;
        private readonly Mock<ILogger<WorkoutService>> _logger;
        private readonly Mock<IUserStatsService> _stats;
        public WorkoutQueryTests() : base("WorkoutTestsDatabase")
        {
            _authorizationService = new Mock<ICachedAuthorizationService>();
            _notificationService = new Mock<IWorkoutNotificationSink>();
            _logger = new Mock<ILogger<WorkoutService>>();
            _stats = new Mock<IUserStatsService>();
            _workoutService = new WorkoutService(_unitOfWork, _authorizationService.Object, _notificationService.Object, _stats.Object, _logger.Object);
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
            Assert.Equal(workout.ExerciseInstances.Count, result.Value.Exercises.Count());
            Assert.Equal(workout.ExerciseInstances.First().Sets.Count, result.Value.Exercises.First().Sets.Count());
        }
        [Fact]
        public async Task GetInProgressData()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            workout.hasStarted = true;
            await _unitOfWork.SaveChangesAsync();
            var progressDto = new WorkoutUpdateProgressDTO
            {
                WorkoutId = workout.Id,
                Exercises = new List<ExerciseUpdateProgressDTO>
                {
                    new ExerciseUpdateProgressDTO
                    {
                        ExerciseInstanceId = workout.ExerciseInstances.First().Id,
                        StartedAt = DateTime.UtcNow,
                        IsCompleted = true,
                        Sets = new List<WorkoutSetProgressUpdateDTO>
                        {
                            new WorkoutSetProgressUpdateDTO
                            {
                                SetId = workout.ExerciseInstances.First().Sets.First().Id,
                                ActualReps = 30,
                                ActualWeight = 30,
                                IsCompleted = true,
                                Notes = "Testing"
                            }
                        }
                    }
                },
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var setResult = await _workoutService.UpdateWorkoutProgressAsync(user.Id, progressDto);
            Assert.NotNull(setResult);
            Assert.Equal("Progress updated successfully", setResult.msg);
            Assert.Equal(2, setResult.status);

            var getResult = await _workoutService.GetWorkoutCurrentProgress(workout.Id);
            Assert.NotNull(getResult);
            Assert.Equal(2, getResult.status);
            var exercise = workout.ExerciseInstances.First();
            var set = exercise.Sets.First();
            var getExercise = getResult.Value.exercises.First();
            var getSet = getExercise.Sets.First();

            //exercise instance assertion
            Assert.Equal(exercise.Exercise.Name, getExercise.Name);
            Assert.Equal(exercise.Exercise.Muscles.Select(m => m.Name).ToList(), getExercise.Muscles);
            Assert.Equal(exercise.IsCompleted, getExercise.IsCompleted);
            Assert.Equal(exercise.StartedAt, getExercise.StartedAt);

            //sets assertion
            Assert.Equal(set.ActualReps, getSet.ActualReps);
            Assert.Equal((float)set.ActualWeight, getSet.ActualWeight);
            Assert.Equal(set.IsCompleted, getSet.IsCompleted);
            Assert.Equal(set.KCaloriesBurned, getSet.KCaloriesBurned);
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
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var NameResult = await _workoutService.GetExercisesOfWorkout(workout.Id, 1, "", "", "Test Exercise 3", 10);
            Assert.NotNull(NameResult);
            Assert.Equal(1, NameResult.Data!.TotalCount);

            var DescriptionResult = await _workoutService.GetExercisesOfWorkout(workout.Id, 1, "", "", "test exercise", 10);
            Assert.NotNull(DescriptionResult);
            Assert.Equal(3, DescriptionResult.Data!.TotalCount);

            var CategoryResult = await _workoutService.GetExercisesOfWorkout(workout.Id, 1, "", "Stren", "", 10);
            Assert.NotNull(CategoryResult);
            Assert.Equal(3, CategoryResult.Data!.TotalCount);

            var DifficultyResult = await _workoutService.GetExercisesOfWorkout(workout.Id, 1, "Medium", "", "", 10);
            Assert.NotNull(DifficultyResult);
            Assert.Equal(3, DifficultyResult.Data!.TotalCount);
        }
        [Fact]
        public async Task GetExerciesOfWorkoutWithSortingTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            var NameAscResult = await _workoutService.GetExercisesOfWorkout(workout.Id, 1, "Name", "asc", "", 10);
            Assert.NotNull(NameAscResult);
            Assert.Equal("Test Exercise 1", NameAscResult.Data!.Items[0].Name);
            Assert.Equal(3, NameAscResult.Data!.TotalCount);

            var NameDescResult = await _workoutService.GetExercisesOfWorkout(workout.Id, 1, "Name", "desc", "", 10);
            Assert.NotNull(NameDescResult);
            Assert.Equal("Test Exercise 3", NameDescResult.Data!.Items[0].Name);
            Assert.Equal(3, NameDescResult.Data!.TotalCount);

            var DateAscResult = await _workoutService.GetExercisesOfWorkout(workout.Id, 1, "Date", "asc", "", 10);
            Assert.NotNull(DateAscResult);
            Assert.Equal("Test Exercise 1", DateAscResult.Data!.Items[0].Name);
            Assert.Equal(3, DateAscResult.Data!.TotalCount);

            var DateDescResult = await _workoutService.GetExercisesOfWorkout(workout.Id, 1, "Date", "d", "", 10);
            Assert.NotNull(DateDescResult);
            Assert.Equal("Test Exercise 3", DateDescResult.Data!.Items[0].Name);
            Assert.Equal(3, DateDescResult.Data!.TotalCount);
        }
    }
}
//        [Fact]
//        public async Task GetAllWorkoutsTest()
//        {
//            var user = CreateTestUser(CreateTestRole());
//            for (int i = 0; i < 5; i++)
//            {
//                var workout = CreateTestWorkout(user, $"Test Workout {i}", $"This is a test workout {i}");
//            }
//            await _unitOfWork.SaveChangesAsync();
//            var result = await _workoutService.GetAllWorkouts(1, 10);
//            Assert.NotNull(result);
//            Assert.Equal(5, result.Data!.TotalCount);
//        }
//    }
//}
