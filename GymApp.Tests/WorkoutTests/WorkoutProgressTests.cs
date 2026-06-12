using Gym_App.Application.Authorization;
using Gym_App.Application.Hubs;
using Gym_App.Application.Services;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infrastructure.DTOs.Exercise;
using Gym_App.Infrastructure.DTOs.Workout;
using Microsoft.Extensions.Logging;
using Moq;

namespace GymApp.Tests.WorkoutTests
{
    public class WorkoutProgressTests : TestBase
    {
        private readonly IWorkoutService _workoutService;
        private readonly Mock<ICachedAuthorizationService> _authorizationService;
        private readonly Mock<IWorkoutNotificationSink> _notificationService;
        private readonly Mock<ILogger<WorkoutService>> _logger;
        public WorkoutProgressTests() : base("WorkoutTestsDatabase")
        {
            _authorizationService = new Mock<ICachedAuthorizationService>();
            _notificationService = new Mock<IWorkoutNotificationSink>();
            _logger = new Mock<ILogger<WorkoutService>>();
            _workoutService = new WorkoutService(_unitOfWork, _authorizationService.Object, _notificationService.Object, _logger.Object);
        }
        [Fact]
        public async Task StartWorkoutSuccessfully()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.StartWorkoutAsync(workout.Id, user.Id);
            Assert.NotNull(result);
            Assert.Equal("Workout started", result.msg);
            Assert.Equal(2, result.status);
        }
        [Fact]
        public async Task StartWorkoutUnauthorized()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(false);
            var result = await _workoutService.StartWorkoutAsync(workout.Id, user.Id);
            Assert.NotNull(result);
            Assert.Equal("Unauthorized", result.msg);
            Assert.Equal(1, result.status);
        }
        [Fact]
        public async Task StartWorkoutButWrongWorkoutId()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.StartWorkoutAsync(Guid.NewGuid(), user.Id);
            Assert.NotNull(result);
            Assert.Equal("Workout not found", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task StartWorkoutButAlreadyStarted()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            workout.hasStarted = true;
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.StartWorkoutAsync(workout.Id, user.Id);
            Assert.NotNull(result);
            Assert.Equal("Workout has already started", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task UpdateWorkoutProgressSuccessfully()
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
                        ExerciseId = workout.ExerciseInstances.First().Id,
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
            var result = await _workoutService.UpdateWorkoutProgressAsync(user.Id, progressDto);
            Assert.NotNull(result);
            Assert.Equal("Progress updated successfully", result.msg);
            Assert.Equal(2, result.status);
        }
        [Fact]
        public async Task UpdateWorkoutProgressButInvalidData()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            workout.hasStarted = true;
            await _unitOfWork.SaveChangesAsync();
            var progressDto = new WorkoutUpdateProgressDTO
            {
                WorkoutId = Guid.Empty,
                Exercises = new List<ExerciseUpdateProgressDTO>
                {
                    new ExerciseUpdateProgressDTO
                    {
                        ExerciseId = workout.ExerciseInstances.First().Id,
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
            var result = await _workoutService.UpdateWorkoutProgressAsync(user.Id, progressDto);
            Assert.NotNull(result);
            Assert.Equal("Invalid progress data", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task UpdateWorkoutProgressbUtWrongWorkoutId()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            workout.hasStarted = true;
            await _unitOfWork.SaveChangesAsync();
            var progressDto = new WorkoutUpdateProgressDTO
            {
                WorkoutId = Guid.NewGuid(),
                Exercises = new List<ExerciseUpdateProgressDTO>
                {
                    new ExerciseUpdateProgressDTO
                    {
                        ExerciseId = workout.ExerciseInstances.First().Id,
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
            var result = await _workoutService.UpdateWorkoutProgressAsync(user.Id, progressDto);
            Assert.NotNull(result);
            Assert.Equal("Workout not found", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task UpdateWorkoutProgressButUnauthorized()
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
                        ExerciseId = workout.ExerciseInstances.First().Id,
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
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(false);
            var result = await _workoutService.UpdateWorkoutProgressAsync(user.Id, progressDto);
            Assert.NotNull(result);
            Assert.Equal("Unauthorized", result.msg);
            Assert.Equal(1, result.status);
        }
        [Fact]
        public async Task UpdateWorkoutProgressButWorkoutHasntStarted()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var progressDto = new WorkoutUpdateProgressDTO
            {
                WorkoutId = workout.Id,
                Exercises = new List<ExerciseUpdateProgressDTO>
                {
                    new ExerciseUpdateProgressDTO
                    {
                        ExerciseId = workout.ExerciseInstances.First().Id,
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
            var result = await _workoutService.UpdateWorkoutProgressAsync(user.Id, progressDto);
            Assert.NotNull(result);
            Assert.Equal("Workout hasn't started", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task CompleteWorkoutSuccessfully()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            workout.hasStarted = true;
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            foreach(var exercise in workout.ExerciseInstances)
            {
                exercise.IsCompleted = true;
                foreach (var set in exercise.Sets)
                {
                    set.IsCompleted = true;
                }
            }
            var result = await _workoutService.CompleteWorkoutAsync(workout.Id, user.Id);
            Assert.NotNull(result);
            Assert.Equal("Workout completed", result.msg);
            Assert.Equal(2, result.status);
        }
        [Fact]
        public async Task CompleteWorkoutButInvalidData()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            workout.hasStarted = true;
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.CompleteWorkoutAsync(Guid.Empty, user.Id);
            Assert.NotNull(result);
            Assert.Equal("Invalid data", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task CompleteWorkoutButWorkoutNotFound()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            workout.hasStarted = true;
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.CompleteWorkoutAsync(Guid.NewGuid(), user.Id);
            Assert.NotNull(result);
            Assert.Equal("Workout not found", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task CompleteWorkoutUnauthroized()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            workout.hasStarted = true;
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(false);
            var result = await _workoutService.CompleteWorkoutAsync(workout.Id, user.Id);
            Assert.NotNull(result);
            Assert.Equal("Unauthorized", result.msg);
            Assert.Equal(1, result.status);
        }
        [Fact]
        public async Task CompleteWorkoutButWorkoutNotStarted()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.CompleteWorkoutAsync(workout.Id, user.Id);
            Assert.NotNull(result);
            Assert.Equal("Workout hasn't even started yet", result.msg);
            Assert.Equal(0, result.status);
        }
    }
}
