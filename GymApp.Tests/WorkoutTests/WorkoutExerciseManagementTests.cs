using Gym_App.Application.Authorization;
using Gym_App.Application.Hubs;
using Gym_App.Application.Services;
using Gym_App.Domain;
using Gym_App.Infastructure.DTOs.WorkoutDTOs;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infrastructure.DTOs.Exercise;
using Gym_App.Infrastructure.DTOs.Workout;
using Gym_App.Infrastructure.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Runtime.CompilerServices;

namespace GymApp.Tests.WorkoutTests
{
    public class WorkoutExerciseManagementTests : TestBase
    {
        private readonly IWorkoutService _workoutService;
        private readonly Mock<ICachedAuthorizationService> _authorizationService;
        private readonly Mock<IWorkoutNotificationSink> _notificationService;
        private readonly Mock<ILogger<WorkoutService>> _logger;
        private readonly Mock<IUserStatsService> _stats;
        public WorkoutExerciseManagementTests() : base("WorkoutTestsDatabase")
        {
            _authorizationService = new Mock<ICachedAuthorizationService>();
            _notificationService = new Mock<IWorkoutNotificationSink>();
            _logger = new Mock<ILogger<WorkoutService>>();
            _stats = new Mock<IUserStatsService>();
            _workoutService = new WorkoutService(_unitOfWork, _authorizationService.Object, _notificationService.Object, _stats.Object, _logger.Object);
        }
        //Adding workout's exercises with all their possibilities of failure
        [Fact]
        public async Task AddExercisesToWorkoutTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var exercise = CreateTestExercise();
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var sets = new List<WorkoutSetDTO>
            {
                new WorkoutSetDTO
                {
                    ActualReps = 10,
                    ActualWeight = 50,
                    SetNumber = 1,
                    Notes = "This is a test set"
                },
                new WorkoutSetDTO
                {
                    ActualReps = 20,
                    ActualWeight = 40,
                    SetNumber = 2,
                    Notes = "This is a test set"
                }
            };
            var workoutExercises = new WorkoutExerciseDTO
            {
                exercisesDetails = new List<ExerciseDetailDTO>
                {
                    new ExerciseDetailDTO
                    {
                        Name = "Test Exercise 1",
                        ExerciseId = exercise.Id,
                        ExerciseOrder = 1,
                        Notes = "This is a test exercise",
                        Sets = sets
                    },
                    new ExerciseDetailDTO
                    {
                        Name = "Test Exercise 2",
                        ExerciseId = exercise.Id,
                        ExerciseOrder = 2,
                        Notes = "This is a test exercise 2",
                        Sets = sets
                    },
                    new ExerciseDetailDTO
                    {
                        Name = "Test Exercise 3",
                        ExerciseId = exercise.Id,
                        ExerciseOrder = 3,
                        Notes = "This is a test exercise 3",
                        Sets = sets
                    },
                    new ExerciseDetailDTO
                    {
                        Name = "Test Exercise 4",
                        ExerciseId = exercise.Id,
                        ExerciseOrder = 4,
                        Notes = "This is a test exercise 4",
                        Sets = sets
                    },
                    new ExerciseDetailDTO
                    {
                        Name = "Test Exercise 5",
                        ExerciseId = exercise.Id,
                        ExerciseOrder = 5,
                        Notes = "This is a test exercise 5",
                        Sets = sets
                    }
                }
            };

            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.AddExercisesToWorkout(workout.Id, workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("Exercises added successfully", result.msg);
            Assert.Equal(2, result.status);
            var workoutFromDb = await _unitOfWork.Workouts.GetWorkoutById(workout.Id);
            Assert.NotNull(workoutFromDb);
            Assert.NotNull(workoutFromDb.ExerciseInstances);
            Assert.Equal(8, workoutFromDb.ExerciseInstances.Count);//8 cause create workout adds 3 exercises

        }
        [Fact]
        public async Task AddExercisesToWorkoutWithNonExistingWorkoutTest()
        {
            var exercise = CreateTestExercise();
            await _unitOfWork.SaveChangesAsync();
            var workoutExercises = new WorkoutExerciseDTO
            {
                exercisesDetails = new List<ExerciseDetailDTO>
                {
                    new ExerciseDetailDTO
                    {
                        Name = "Test Exercise 1",
                        ExerciseId = exercise.Id,
                        ExerciseOrder = 1,
                        Notes = "This is a test exercise",
                        Sets = new List<WorkoutSetDTO>
                        {
                            new WorkoutSetDTO
                            {
                                ActualReps = 10,
                                ActualWeight = 50,
                                SetNumber = 1,
                                Notes = "This is a test set"
                            }
                        }
                    }
                }
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.AddExercisesToWorkout(Guid.NewGuid(), workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("Workout not found", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task AddExercisesButInvalidData()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var workoutExercises = new WorkoutExerciseDTO
            {
                exercisesDetails = null
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.AddExercisesToWorkout(workout.Id, workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("Invalid data", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task AddExercisesButUnauthorized()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var exercise = CreateTestExercise();
            await _unitOfWork.SaveChangesAsync();
            var workoutExercises = new WorkoutExerciseDTO
            {
                exercisesDetails = new List<ExerciseDetailDTO>
                {
                    new ExerciseDetailDTO
                    {
                        Name = "Test Exercise 1",
                        ExerciseId = exercise.Id,
                        ExerciseOrder = 1,
                        Notes = "This is a test exercise",
                        Sets = new List<WorkoutSetDTO>
                        {
                            new WorkoutSetDTO
                            {
                                ActualReps = 10,
                                ActualWeight = 50,
                                SetNumber = 1,
                                Notes = "This is a test set"
                            }
                        }
                    }
                }
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(false);
            var result = await _workoutService.AddExercisesToWorkout(workout.Id, workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("Unauthorized", result.msg);
            Assert.Equal(1, result.status);
        }
        [Fact]
        public async Task AddExercisesWithWrongIds()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var workoutExercises = new WorkoutExerciseDTO
            {
                exercisesDetails = new List<ExerciseDetailDTO>
                {
                    new ExerciseDetailDTO
                    {
                        Name = "Test Exercise 1",
                        ExerciseId = Guid.NewGuid(), // Non-existing exercise ID
                        ExerciseOrder = 1,
                        Notes = "This is a test exercise",
                        Sets = new List<WorkoutSetDTO>
                        {
                            new WorkoutSetDTO
                            {
                                ActualReps = 10,
                                ActualWeight = 50,
                                SetNumber = 1,
                                Notes = "This is a test set"
                            }
                        }
                    }
                }
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.AddExercisesToWorkout(workout.Id, workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("No new exercises were added, all were already in the workout or ids were wrong", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task AddExercisesWithSomeWrongIds()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var exercise = CreateTestExercise();
            await _unitOfWork.SaveChangesAsync();
            var workoutExercises = new WorkoutExerciseDTO
            {
                exercisesDetails = new List<ExerciseDetailDTO>
                {
                    new ExerciseDetailDTO
                    {
                        Name = "Test Exercise 1",
                        ExerciseId = exercise.Id, // Valid exercise ID
                        ExerciseOrder = 1,
                        Notes = "This is a test exercise",
                        Sets = new List<WorkoutSetDTO>
                        {
                            new WorkoutSetDTO
                            {
                                ActualReps = 10,
                                ActualWeight = 50,
                                SetNumber = 1,
                                Notes = "This is a test set"
                            }
                        }
                    },
                    new ExerciseDetailDTO
                    {
                        Name = "Test Exercise 2",
                        ExerciseId = Guid.NewGuid(), // Non-existing exercise ID
                        ExerciseOrder = 2,
                        Notes = "This is a test exercise",
                        Sets = new List<WorkoutSetDTO>
                        {
                            new WorkoutSetDTO
                            {
                                ActualReps = 15,
                                ActualWeight = 40,
                                SetNumber = 1,
                                Notes = "This is a test set"
                            }
                        }
                    }
                }
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.AddExercisesToWorkout(workout.Id, workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("Some exercises were added successfully, but some were not added because they were already in the workout or their ids were wrong", result.msg);
            Assert.Equal(2, result.status);
        }
        [Fact]
        public async Task SetExercisesSuccessfully()// since when creating a workout 3 exercises are added by default, we can test for how much exercises are there after setting which should be 2
        {
            var user = CreateTestUser(CreateTestRole());
            var exercise = CreateTestExercise();
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var sets = new List<WorkoutSetDTO>
            {
                new WorkoutSetDTO
                {
                    ActualReps = 10,
                    ActualWeight = 50,
                    SetNumber = 1,
                    Notes = "This is a test set"
                },
                new WorkoutSetDTO
                {
                    ActualReps = 20,
                    ActualWeight = 40,
                    SetNumber = 2,
                    Notes = "This is a test set"
                }
            };
            var workoutExercises = new WorkoutExerciseDTO
            {
                exercisesDetails = new List<ExerciseDetailDTO>
                {
                    new ExerciseDetailDTO
                    {
                        Name = "Test Exercise 1",
                        ExerciseId = exercise.Id,
                        ExerciseOrder = 1,
                        Notes = "This is a test exercise",
                        Sets = sets
                    },
                    new ExerciseDetailDTO
                    {
                        Name = "Test Exercise 2",
                        ExerciseId = exercise.Id,
                        ExerciseOrder = 2,
                        Notes = "This is a test exercise 2",
                        Sets = sets
                    }
                }
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.SetExercisesOfWorkout(workout.Id, workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("Exercises set successfully", result.msg);
            Assert.Equal(2, result.status);

            var workoutFromDb = await _unitOfWork.Workouts.GetWorkoutById(workout.Id);
            Assert.NotNull(workoutFromDb);
            Assert.Equal(2, workoutFromDb.ExerciseInstances!.Count);
        }
        [Fact]
        public async Task SetExercisesWithNonExistingWorkoutTest()
        {
            var exercise = CreateTestExercise();
            await _unitOfWork.SaveChangesAsync();
            var workoutExercises = new WorkoutExerciseDTO
            {
                exercisesDetails = new List<ExerciseDetailDTO>
                {
                    new ExerciseDetailDTO
                    {
                        Name = "Test Exercise 1",
                        ExerciseId = exercise.Id,
                        ExerciseOrder = 1,
                        Notes = "This is a test exercise",
                        Sets = new List<WorkoutSetDTO>
                        {
                            new WorkoutSetDTO
                            {
                                ActualReps = 10,
                                ActualWeight = 50,
                                SetNumber = 1,
                                Notes = "This is a test set"
                            }
                        }
                    }
                }
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.SetExercisesOfWorkout(Guid.NewGuid(), workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("Workout not found", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task SetExercisesButInvalidData()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var workoutExercises = new WorkoutExerciseDTO
            {
                exercisesDetails = null
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.SetExercisesOfWorkout(workout.Id, workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("Invalid data", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task SetExercisesButUnauthorized()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var exercise = CreateTestExercise();
            await _unitOfWork.SaveChangesAsync();
            var workoutExercises = new WorkoutExerciseDTO
            {
                exercisesDetails = new List<ExerciseDetailDTO>
                {
                    new ExerciseDetailDTO
                    {
                        Name = "Test Exercise 1",
                        ExerciseId = exercise.Id,
                        ExerciseOrder = 1,
                        Notes = "This is a test exercise",
                        Sets = new List<WorkoutSetDTO>
                        {
                            new WorkoutSetDTO
                            {
                                ActualReps = 10,
                                ActualWeight = 50,
                                SetNumber = 1,
                                Notes = "This is a test set"
                            }
                        }
                    }
                }
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(false);
            var result = await _workoutService.SetExercisesOfWorkout(workout.Id, workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("Unauthorized", result.msg);
            Assert.Equal(1, result.status);
        }
        [Fact]
        public async Task SetExercisesWithWrongIds()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var workoutExercises = new WorkoutExerciseDTO
            {
                exercisesDetails = new List<ExerciseDetailDTO>
                {
                    new ExerciseDetailDTO
                    {
                        Name = "Test Exercise 1",
                        ExerciseId = Guid.NewGuid(), // Non-existing exercise ID
                        ExerciseOrder = 1,
                        Notes = "This is a test exercise",
                        Sets = new List<WorkoutSetDTO>
                        {
                            new WorkoutSetDTO
                            {
                                ActualReps = 10,
                                ActualWeight = 50,
                                SetNumber = 1,
                                Notes = "This is a test set"
                            }
                        }
                    }
                }
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.SetExercisesOfWorkout(workout.Id, workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("No exercises were set, all ids were wrong", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task SetExercisesWithSomeWrongIds()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var exercise = CreateTestExercise();
            await _unitOfWork.SaveChangesAsync();
            var workoutExercises = new WorkoutExerciseDTO
            {
                exercisesDetails = new List<ExerciseDetailDTO>
                {
                    new ExerciseDetailDTO
                    {
                        Name = "Test Exercise 1",
                        ExerciseId = exercise.Id, // Valid exercise ID
                        ExerciseOrder = 1,
                        Notes = "This is a test exercise",
                        Sets = new List<WorkoutSetDTO>
                        {
                            new WorkoutSetDTO
                            {
                                ActualReps = 10,
                                ActualWeight = 50,
                                SetNumber = 1,
                                Notes = "This is a test set"
                            }
                        }
                    },
                    new ExerciseDetailDTO
                    {
                        Name = "Test Exercise 2",
                        ExerciseId = Guid.NewGuid(), // Non-existing exercise ID
                        ExerciseOrder = 2,
                        Notes = "This is a test exercise",
                        Sets = new List<WorkoutSetDTO>
                        {
                            new WorkoutSetDTO
                            {
                                ActualReps = 15,
                                ActualWeight = 40,
                                SetNumber = 1,
                                Notes = "This is a test set"
                            }
                        }
                    }
                }
            };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.SetExercisesOfWorkout(workout.Id, workoutExercises);
            Assert.NotNull(result);
            Assert.Equal("Some exercises were set successfully, but some were not set because their exercise ids were wrong", result.msg);
            Assert.Equal(2, result.status);
        }
        [Fact]
        public async Task DeleteExercisesSuccessfully()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var exerciseInstanceIdsToDelete = workout.ExerciseInstances!.Select(ei => ei.Id).Take(2).ToList();
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.DeleteExercisesFromWorkout(workout.Id, exerciseInstanceIdsToDelete);
            Assert.NotNull(result);
            Assert.Equal("Exercises deleted successfully", result.msg);
            Assert.Equal(2, result.status);
            var workoutFromDb = await _unitOfWork.Workouts.GetWorkoutById(workout.Id);
            Assert.NotNull(workoutFromDb);
            Assert.Equal(1, workoutFromDb.ExerciseInstances!.Count); // Since we created a workout with 3 exercises and deleted 2, we should have 1 left
        }
        [Fact]
        public async Task DeleteExercisesWithNonExistingWorkoutTest()
        {
            var exerciseInstanceIdsToDelete = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.DeleteExercisesFromWorkout(Guid.NewGuid(), exerciseInstanceIdsToDelete);
            Assert.NotNull(result);
            Assert.Equal("Workout not found", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task DeleteExercisesButUnauthorized()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var exerciseInstanceIdsToDelete = workout.ExerciseInstances!.Select(ei => ei.Id).Take(2).ToList();
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(false);
            var result = await _workoutService.DeleteExercisesFromWorkout(workout.Id, exerciseInstanceIdsToDelete);
            Assert.NotNull(result);
            Assert.Equal("Unauthorized", result.msg);
            Assert.Equal(1, result.status);
        }
        [Fact]
        public async Task DeleteExercisesWithWrongIds()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var exerciseInstanceIdsToDelete = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }; // Non-existing exercise instance IDs
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.DeleteExercisesFromWorkout(workout.Id, exerciseInstanceIdsToDelete);
            Assert.NotNull(result);
            Assert.Equal("No exercises were deleted, all ids were wrong", result.msg);
            Assert.Equal(0, result.status);
        }
        [Fact]
        public async Task DeleteExercisesWithSomeWrongIds()
        {
            var user = CreateTestUser(CreateTestRole());
            var workout = CreateTestWorkout(user);
            await _unitOfWork.SaveChangesAsync();
            var existingExerciseInstanceId = workout.ExerciseInstances!.First().Id;
            var exerciseInstanceIdsToDelete = new List<Guid> { existingExerciseInstanceId, Guid.NewGuid() }; // One valid and one non-existing ID
            _authorizationService.Setup(x => x.IsUserAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            var result = await _workoutService.DeleteExercisesFromWorkout(workout.Id, exerciseInstanceIdsToDelete);
            Assert.NotNull(result);
            Assert.Equal("Some exercises deleted successfully, some were not found", result.msg);
            Assert.Equal(2, result.status);
            var workoutFromDb = await _unitOfWork.Workouts.GetWorkoutById(workout.Id);
            Assert.NotNull(workoutFromDb);
            Assert.Equal(2, workoutFromDb.ExerciseInstances!.Count); // Since we created a workout with 3 exercises and deleted 1 valid and 1 invalid, we should have 2 left
        }
    }
}
