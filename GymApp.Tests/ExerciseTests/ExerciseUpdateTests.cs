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
    public class ExerciseUpdateTests : TestBase
    {
        private readonly IExerciseService _exerciseService;
        public ExerciseUpdateTests() : base("ExerciseTestsDatabase")
        {
            _exerciseService = new ExerciseService(_unitOfWork);
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
    }
}
