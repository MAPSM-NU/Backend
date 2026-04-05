using Gym_App.Application.Services;
using Gym_App.Domain;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.DTOs.Exercise;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infastructure.Repositries;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GymApp.Tests;

public class ExerciseTests
{
    private readonly DbBase _db;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExerciseService _exerciseService;

    public ExerciseTests()
    {
        var options = new DbContextOptionsBuilder<DbBase>()
            .UseInMemoryDatabase(databaseName: $"ExerciseTestDatabase-{Guid.NewGuid()}")
            .Options;

        _db = new DbBase(options);
        _unitOfWork = new UnitOfWork(_db);
        _exerciseService = new ExerciseService(_unitOfWork);
    }

    // ========================================
    // CREATE EXERCISE TESTS
    // ========================================

    /// <summary>
    /// Test Case 1: Successfully create a new exercise
    /// Scenario: Valid exercise data provided
    /// Expected: Exercise is created successfully (status = 2)
    /// </summary>
    [Fact]
    public async Task CreateExercise_WithValidData_ShouldSucceed()
    {
        // ARRANGE - Set up test data
        var exerciseDto = new ExerciseCreationDTO
        {
            Name = "Push Ups",
            Description = "Upper body strength exercise",
            Difficulty = "Easy",
            VideoUrl = "https://example.com/pushups.mp4",
            Category = "Chest",
            Grip = "Wide"
        };

        // ACT - Execute the method being tested
        var result = await _exerciseService.CreateExercise(exerciseDto);

        // ASSERT - Verify the result
        Assert.NotNull(result);
        Assert.Equal(2, result.status);  // Status 2 means Success
        Assert.Equal("Exercise created successfully", result.msg);

        // Verify the exercise was actually saved to the database
        var savedExercise = await _unitOfWork.Exercises.GetAll()
            .FirstOrDefaultAsync(e => e.Name == "Push Ups");
        Assert.NotNull(savedExercise);
        Assert.Equal("Chest", savedExercise.Category);
    }

    /// <summary>
    /// Test Case 2: Create exercise with null input
    /// Scenario: Null or empty exercise data provided
    /// Expected: Should fail with status 0 (Bad Request)
    /// </summary>
    [Fact]
    public async Task CreateExercise_WithNullInput_ShouldFail()
    {
        // ARRANGE
        ExerciseCreationDTO nullExercise = null;

        // ACT
        var result = await _exerciseService.CreateExercise(nullExercise);

        // ASSERT
        Assert.NotNull(result);
        Assert.Equal(0, result.status);  // Status 0 means error/bad request
        Assert.Equal("Invalid exercise data", result.msg);
    }

    /// <summary>
    /// Test Case 3: Create exercise with empty name
    /// Scenario: Exercise name is null or whitespace
    /// Expected: Should fail with status 0
    /// </summary>
    [Fact]
    public async Task CreateExercise_WithEmptyName_ShouldFail()
    {
        // ARRANGE
        var exerciseDto = new ExerciseCreationDTO
        {
            Name = "  ",  // Empty/whitespace name
            Description = "Test"
        };

        // ACT
        var result = await _exerciseService.CreateExercise(exerciseDto);

        // ASSERT
        Assert.Equal(0, result.status);
        Assert.Equal("Invalid exercise data", result.msg);
    }

    /// <summary>
    /// Test Case 4: Create duplicate exercise
    /// Scenario: Exercise with same name already exists
    /// Expected: Should fail with status 0
    /// </summary>
    [Fact]
    public async Task CreateExercise_WithDuplicateName_ShouldFail()
    {
        // ARRANGE - Create first exercise
        var firstExercise = new ExerciseCreationDTO
        {
            Name = "Bench Press",
            Description = "Chest exercise"
        };

        // Create the first exercise successfully
        await _exerciseService.CreateExercise(firstExercise);

        // Try to create duplicate
        var duplicateExercise = new ExerciseCreationDTO
        {
            Name = "Bench Press",  // Same name
            Description = "Different description"
        };

        // ACT
        var result = await _exerciseService.CreateExercise(duplicateExercise);

        // ASSERT
        Assert.Equal(0, result.status);
        Assert.Equal("Exercise already exists", result.msg);
    }

    // ========================================
    // UPDATE EXERCISE TESTS
    // ========================================

    /// <summary>
    /// Test Case 5: Successfully update an existing exercise
    /// Scenario: Valid exercise ID and update data provided
    /// Expected: Exercise is updated successfully
    /// </summary>
    [Fact]
    public async Task UpdateExercise_WithValidData_ShouldSucceed()
    {
        // ARRANGE - Create an exercise first
        var exerciseDto = new ExerciseCreationDTO
        {
            Name = "Squats",
            Description = "Leg exercise",
            Difficulty = "Medium"
        };

        await _exerciseService.CreateExercise(exerciseDto);

        // Get the created exercise ID
        var createdExercise = await _unitOfWork.Exercises.GetAll()
            .FirstOrDefaultAsync(e => e.Name == "Squats");
        var exerciseId = createdExercise.Id;

        // Prepare update data
        var updateDto = new ExerciseCreationDTO
        {
            Name = "Squats Updated",
            Description = "Advanced leg exercise",
            Difficulty = "Hard"
        };

        // ACT
        var result = await _exerciseService.UpdateExercise(exerciseId, updateDto);

        // ASSERT
        Assert.Equal(2, result.status);
        Assert.Equal("Exercise updated successfully", result.msg);

        // Verify the update in database
        var updatedExercise = await _unitOfWork.Exercises.GetAll()
            .FirstOrDefaultAsync(e => e.Id == exerciseId);
        Assert.Equal("Squats Updated", updatedExercise.Name);
        Assert.Equal("Hard", updatedExercise.Difficulty);
    }

    /// <summary>
    /// Test Case 6: Update non-existent exercise
    /// Scenario: Exercise ID doesn't exist
    /// Expected: Should fail with status 0
    /// </summary>
    [Fact]
    public async Task UpdateExercise_WithNonExistentId_ShouldFail()
    {
        // ARRANGE
        var nonExistentId = Guid.NewGuid();
        var updateDto = new ExerciseCreationDTO
        {
            Name = "Updated Exercise"
        };

        // ACT
        var result = await _exerciseService.UpdateExercise(nonExistentId, updateDto);

        // ASSERT
        Assert.Equal(0, result.status);
        Assert.Equal("Exercise not found", result.msg);
    }

    /// <summary>
    /// Test Case 7: Update exercise with invalid data
    /// Scenario: Null update DTO provided
    /// Expected: Should fail with status 0
    /// </summary>
    [Fact]
    public async Task UpdateExercise_WithNullInput_ShouldFail()
    {
        // ARRANGE
        var exerciseId = Guid.NewGuid();

        // ACT
        var result = await _exerciseService.UpdateExercise(exerciseId, null);

        // ASSERT
        Assert.Equal(0, result.status);
        Assert.Equal("Invalid exercise data", result.msg);
    }

    /// <summary>
    /// Test Case 8: Update exercise with duplicate name
    /// Scenario: New name already exists in another exercise
    /// Expected: Should fail with status 0
    /// </summary>
    [Fact]
    public async Task UpdateExercise_WithDuplicateName_ShouldFail()
    {
        // ARRANGE - Create two exercises
        var exercise1 = new ExerciseCreationDTO { Name = "Exercise One" };
        var exercise2 = new ExerciseCreationDTO { Name = "Exercise Two" };

        await _exerciseService.CreateExercise(exercise1);
        await _exerciseService.CreateExercise(exercise2);

        var exercise2Data = await _unitOfWork.Exercises.GetAll()
            .FirstOrDefaultAsync(e => e.Name == "Exercise Two");

        // Try to update Exercise Two with name of Exercise One
        var updateDto = new ExerciseCreationDTO { Name = "Exercise One" };

        // ACT
        var result = await _exerciseService.UpdateExercise(exercise2Data.Id, updateDto);

        // ASSERT
        Assert.Equal(0, result.status);
        Assert.Equal("Exercise with this name already exists", result.msg);
    }

    // ========================================
    // DELETE EXERCISE TESTS
    // ========================================

    /// <summary>
    /// Test Case 9: Successfully delete an exercise
    /// Scenario: Valid exercise ID provided
    /// Expected: Exercise is deleted successfully
    /// </summary>
    [Fact]
    public async Task DeleteExercise_WithValidId_ShouldSucceed()
    {
        // ARRANGE - Create an exercise first
        var exerciseDto = new ExerciseCreationDTO
        {
            Name = "Dumbbell Row",
            Description = "Back exercise"
        };

        await _exerciseService.CreateExercise(exerciseDto);

        var createdExercise = await _unitOfWork.Exercises.GetAll()
            .FirstOrDefaultAsync(e => e.Name == "Dumbbell Row");
        var exerciseId = createdExercise.Id;

        // ACT
        var result = await _exerciseService.DeleteExercise(exerciseId);

        // ASSERT
        Assert.Equal(2, result.status);
        Assert.Equal("Exercise deleted successfully", result.msg);

        // Verify the exercise is actually deleted
        var deletedExercise = await _unitOfWork.Exercises.GetAll()
            .FirstOrDefaultAsync(e => e.Id == exerciseId);
        Assert.Null(deletedExercise);
    }

    /// <summary>
    /// Test Case 10: Delete non-existent exercise
    /// Scenario: Exercise ID doesn't exist
    /// Expected: Should fail with status 0
    /// </summary>
    [Fact]
    public async Task DeleteExercise_WithNonExistentId_ShouldFail()
    {
        // ARRANGE
        var nonExistentId = Guid.NewGuid();

        // ACT
        var result = await _exerciseService.DeleteExercise(nonExistentId);

        // ASSERT
        Assert.Equal(0, result.status);
        Assert.Equal("Exercise not found", result.msg);
    }

    // ========================================
    // ADD MUSCLES TO EXERCISE TESTS
    // ========================================

    /// <summary>
    /// Test Case 11: Successfully add muscles to exercise
    /// Scenario: Valid exercise ID and muscle IDs provided
    /// Expected: Muscles are added successfully
    /// </summary>
    [Fact]
    public async Task AddMusclesToExercise_WithValidData_ShouldSucceed()
    {
        // ARRANGE - Create exercise and muscles
        var exerciseDto = new ExerciseCreationDTO { Name = "Pull Ups" };
        await _exerciseService.CreateExercise(exerciseDto);

        var exercise = await _unitOfWork.Exercises.GetAll()
            .Include(e => e.Muscles)
            .FirstOrDefaultAsync(e => e.Name == "Pull Ups");

        // Create muscles
        var muscle1 = new Muscles { Id = Guid.NewGuid(), Name = "Lats" };
        var muscle2 = new Muscles { Id = Guid.NewGuid(), Name = "Back" };

        await _unitOfWork.Muscles.Create(muscle1);
        await _unitOfWork.Muscles.Create(muscle2);
        await _unitOfWork.SaveChangesAsync();

        var muscleDto = new ExerciseMusclesDTO
        {
            Muscles = new List<Guid> { muscle1.Id, muscle2.Id }
        };

        // ACT
        var result = await _exerciseService.AddMusclesToExercise(exercise.Id, muscleDto);

        // ASSERT
        Assert.Equal(2, result.status);
        Assert.Equal("Muscles added successfully", result.msg);

        // Verify muscles were added
        var updatedExercise = await _unitOfWork.Exercises.GetAll()
            .Include(e => e.Muscles)
            .FirstOrDefaultAsync(e => e.Id == exercise.Id);

        Assert.NotNull(updatedExercise.Muscles);
        Assert.Equal(2, updatedExercise.Muscles.Count);
    }

    /// <summary>
    /// Test Case 12: Add muscles with null or empty list
    /// Scenario: Invalid DTO provided
    /// Expected: Should fail with status 0
    /// </summary>
    [Fact]
    public async Task AddMusclesToExercise_WithNullMusclelist_ShouldFail()
    {
        // ARRANGE
        var exerciseId = Guid.NewGuid();
        var invalidDto = new ExerciseMusclesDTO { Muscles = null };

        // ACT
        var result = await _exerciseService.AddMusclesToExercise(exerciseId, invalidDto);

        // ASSERT
        Assert.Equal(0, result.status);
        Assert.Equal("Invalid DTO", result.msg);
    }

    /// <summary>
    /// Test Case 13: Add muscles to non-existent exercise
    /// Scenario: Exercise ID doesn't exist
    /// Expected: Should fail with status 0
    /// </summary>
    [Fact]
    public async Task AddMusclesToExercise_WithNonExistentExercise_ShouldFail()
    {
        // ARRANGE
        var nonExistentExerciseId = Guid.NewGuid();
        var muscle = new Muscles { Id = Guid.NewGuid(), Name = "Biceps" };

        var muscleDto = new ExerciseMusclesDTO
        {
            Muscles = new List<Guid> { muscle.Id }
        };

        // ACT
        var result = await _exerciseService.AddMusclesToExercise(nonExistentExerciseId, muscleDto);

        // ASSERT
        Assert.Equal(0, result.status);
        Assert.Equal("Exercise not found", result.msg);
    }

    /// <summary>
    /// Test Case 14: Add muscles that don't exist
    /// Scenario: Muscle IDs don't exist in database
    /// Expected: Should fail with status 0
    /// </summary>
    [Fact]
    public async Task AddMusclesToExercise_WithNonExistentMuscles_ShouldFail()
    {
        // ARRANGE - Create exercise but not muscles
        var exerciseDto = new ExerciseCreationDTO { Name = "Curls" };
        await _exerciseService.CreateExercise(exerciseDto);

        var exercise = await _unitOfWork.Exercises.GetAll()
            .FirstOrDefaultAsync(e => e.Name == "Curls");

        var muscleDto = new ExerciseMusclesDTO
        {
            Muscles = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
        };

        // ACT
        var result = await _exerciseService.AddMusclesToExercise(exercise.Id, muscleDto);

        // ASSERT
        Assert.Equal(0, result.status);
        Assert.Equal("Muscles to add not found", result.msg);
    }

    // ========================================
    // REMOVE MUSCLES FROM EXERCISE TESTS
    // ========================================

    /// <summary>
    /// Test Case 15: Successfully remove muscles from exercise
    /// Scenario: Valid exercise ID with existing muscles to remove
    /// Expected: Muscles are removed successfully
    /// </summary>
    [Fact]
    public async Task RemoveMusclesFromExercise_WithValidData_ShouldSucceed()
    {
        // ARRANGE - Create exercise with muscles
        var exerciseDto = new ExerciseCreationDTO { Name = "Deadlifts" };
        await _exerciseService.CreateExercise(exerciseDto);

        var exercise = await _unitOfWork.Exercises.GetAll()
            .Include(e => e.Muscles)
            .FirstOrDefaultAsync(e => e.Name == "Deadlifts");

        // Create and add muscles
        var muscle1 = new Muscles { Id = Guid.NewGuid(), Name = "Hamstrings" };
        var muscle2 = new Muscles { Id = Guid.NewGuid(), Name = "Glutes" };

        await _unitOfWork.Muscles.Create(muscle1);
        await _unitOfWork.Muscles.Create(muscle2);
        await _unitOfWork.SaveChangesAsync();

        // Add muscles to exercise
        exercise.Muscles.Add(muscle1);
        exercise.Muscles.Add(muscle2);
        await _unitOfWork.Exercises.Update(exercise);
        await _unitOfWork.SaveChangesAsync();

        // Remove one muscle
        var muscleDto = new ExerciseMusclesDTO
        {
            Muscles = new List<Guid> { muscle1.Id }
        };

        // ACT
        var result = await _exerciseService.RemoveMusclesFromExercise(exercise.Id, muscleDto);

        // ASSERT
        Assert.Equal(2, result.status);
        Assert.Equal("Muscles removed successfully", result.msg);

        // Verify muscle was removed
        var updatedExercise = await _unitOfWork.Exercises.GetAll()
            .Include(e => e.Muscles)
            .FirstOrDefaultAsync(e => e.Id == exercise.Id);

        Assert.Single(updatedExercise.Muscles);
        Assert.Equal("Glutes", updatedExercise.Muscles.First().Name);
    }

    /// <summary>
    /// Test Case 16: Remove muscles with invalid input
    /// Scenario: Null or empty muscle list
    /// Expected: Should fail with status 0
    /// </summary>
    [Fact]
    public async Task RemoveMusclesFromExercise_WithNullMuscleList_ShouldFail()
    {
        // ARRANGE
        var exerciseId = Guid.NewGuid();
        var invalidDto = new ExerciseMusclesDTO { Muscles = new List<Guid>() };

        // ACT
        var result = await _exerciseService.RemoveMusclesFromExercise(exerciseId, invalidDto);

        // ASSERT
        Assert.Equal(0, result.status);
        Assert.Equal("Invalid DTO", result.msg);
    }

    /// <summary>
    /// Test Case 17: Remove muscles from non-existent exercise
    /// Scenario: Exercise ID doesn't exist
    /// Expected: Should fail with status 0
    /// </summary>
    [Fact]
    public async Task RemoveMusclesFromExercise_WithNonExistentExercise_ShouldFail()
    {
        // ARRANGE
        var muscleDto = new ExerciseMusclesDTO
        {
            Muscles = new List<Guid> { Guid.NewGuid() }
        };

        // ACT
        var result = await _exerciseService.RemoveMusclesFromExercise(Guid.NewGuid(), muscleDto);

        // ASSERT
        Assert.Equal(0, result.status);
        Assert.Equal("Exercise not found", result.msg);
    }

    // ========================================
    // GET EXERCISE TESTS
    // ========================================

    /// <summary>
    /// Test Case 18: Get exercise by name - Found
    /// Scenario: Exercise exists with matching name
    /// Expected: Exercise is returned with status 2
    /// </summary>
    [Fact]
    public async Task GetExerciseByName_WhenExists_ShouldReturnExercise()
    {
        // ARRANGE
        var exerciseDto = new ExerciseCreationDTO
        {
            Name = "Tricep Dips",
            Description = "Arms exercise",
            Difficulty = "Medium"
        };

        await _exerciseService.CreateExercise(exerciseDto);

        // ACT
        var result = await _exerciseService.GetExerciseByName("Tricep Dips");

        // ASSERT
        Assert.Equal(2, result.status);
        Assert.Equal("Successful", result.msg);
        Assert.NotNull(result.Value);
        Assert.Equal("Tricep Dips", result.Value.Name);
    }

    /// <summary>
    /// Test Case 19: Get exercise by name - Not Found
    /// Scenario: Exercise doesn't exist
    /// Expected: Returns null with status 0
    /// </summary>
    [Fact]
    public async Task GetExerciseByName_WhenNotExists_ShouldFail()
    {
        // ACT
        var result = await _exerciseService.GetExerciseByName("NonExistent");

        // ASSERT
        Assert.Equal(0, result.status);
        Assert.Equal("Exercise not found", result.msg);
        Assert.Null(result.Value);
    }

    /// <summary>
    /// Test Case 20: Get exercise by ID - Found
    /// Scenario: Exercise exists with matching ID
    /// Expected: Exercise is returned with status 2
    /// </summary>
    [Fact]
    public async Task GetExerciseByID_WhenExists_ShouldReturnExercise()
    {
        // ARRANGE
        var exerciseDto = new ExerciseCreationDTO
        {
            Name = "Shoulder Press",
            Description = "Shoulder exercise",
            Category = "Shoulders"
        };

        await _exerciseService.CreateExercise(exerciseDto);

        var exercise = await _unitOfWork.Exercises.GetAll()
            .FirstOrDefaultAsync(e => e.Name == "Shoulder Press");

        // ACT
        var result = await _exerciseService.GetExerciseByID(exercise.Id);

        // ASSERT
        Assert.Equal(2, result.status);
        Assert.Equal("Successful", result.msg);
        Assert.NotNull(result.Value);
        Assert.Equal("Shoulder Press", result.Value.Name);
    }

    /// <summary>
    /// Test Case 21: Get exercise by ID - Not Found
    /// Scenario: Exercise ID doesn't exist
    /// Expected: Returns null with status 0
    /// </summary>
    [Fact]
    public async Task GetExerciseByID_WhenNotExists_ShouldFail()
    {
        // ACT
        var result = await _exerciseService.GetExerciseByID(Guid.NewGuid());

        // ASSERT
        Assert.Equal(0, result.status);
        Assert.Equal("Exercise not found", result.msg);
        Assert.Null(result.Value);
    }

    /// <summary>
    /// Test Case 22: Get exercise muscles
    /// Scenario: Exercise exists with associated muscles
    /// Expected: List of muscles is returned
    /// </summary>
    [Fact]
    public async Task GetExerciseMuscles_WithMuscles_ShouldReturnMuscleList()
    {
        // ARRANGE - Create exercise and add muscles
        var exerciseDto = new ExerciseCreationDTO { Name = "Lat Pulldown" };
        await _exerciseService.CreateExercise(exerciseDto);

        var exercise = await _unitOfWork.Exercises.GetAll()
            .Include(e => e.Muscles)
            .FirstOrDefaultAsync(e => e.Name == "Lat Pulldown");

        var muscle = new Muscles { Id = Guid.NewGuid(), Name = "Latissimus Dorsi" };
        await _unitOfWork.Muscles.Create(muscle);
        await _unitOfWork.SaveChangesAsync();

        exercise.Muscles.Add(muscle);
        await _unitOfWork.Exercises.Update(exercise);
        await _unitOfWork.SaveChangesAsync();

        // ACT
        var result = await _exerciseService.GetExerciseMuscles(exercise.Id);

        // ASSERT
        Assert.Equal(2, result.status);
        Assert.Equal("Successful", result.msg);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value);
        Assert.Equal("Latissimus Dorsi", result.Value.First().Name);
    }

    /// <summary>
    /// Test Case 23: Get muscles from non-existent exercise
    /// Scenario: Exercise ID doesn't exist
    /// Expected: Returns null with status 0
    /// </summary>
    [Fact]
    public async Task GetExerciseMuscles_WithNonExistentExercise_ShouldFail()
    {
        // ACT
        var result = await _exerciseService.GetExerciseMuscles(Guid.NewGuid());

        // ASSERT
        Assert.Equal(0, result.status);
        Assert.Equal("Exercise not found", result.msg);
        Assert.Null(result.Value);
    }
}
