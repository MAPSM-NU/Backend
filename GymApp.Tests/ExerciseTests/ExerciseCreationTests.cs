using Gym_App.Application.Services;
using Gym_App.Infastructure.DTOs.Exercise;
using Gym_App.Infastructure.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymApp.Tests.ExerciseTests
{
    public class ExerciseCreationTests : TestBase
    {
        private readonly IExerciseService _exerciseService;
        public ExerciseCreationTests() : base("ExerciseTestsDatabase")
        {
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
    }
}
