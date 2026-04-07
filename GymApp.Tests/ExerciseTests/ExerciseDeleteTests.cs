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
    public class ExerciseDeleteTests : TestBase
    {
        private readonly IExerciseService _exerciseService;
        public ExerciseDeleteTests() : base("ExerciseTestsDatabase")
        {
            _exerciseService = new ExerciseService(_unitOfWork);
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
    }
}
