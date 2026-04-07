using Gym_App.Application.Services;
using Gym_App.Domain;
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
    public class ExerciseMusclesManagementTests : TestBase
    {
        private readonly IExerciseService _exerciseService;
        public ExerciseMusclesManagementTests() : base("ExerciseTestsDatabase")
        {
            _exerciseService = new ExerciseService(_unitOfWork);
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
    }
}
