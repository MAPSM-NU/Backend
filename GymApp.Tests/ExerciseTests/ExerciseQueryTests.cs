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
    public class ExerciseQueryTests : TestBase
    {
        private readonly IExerciseService _exerciseService;
        public ExerciseQueryTests() : base("ExerciseTestsDatabase")
        {
            _exerciseService = new ExerciseService(_unitOfWork);
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
}
