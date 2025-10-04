using Gym_App.Domain.DTOs;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Service.Controllers
{
    public class ExerciseController : Controller
    {
        private readonly IExerciseService _exerciseService;
        public ExerciseController(IExerciseService exerciseService)
        {
            _exerciseService = exerciseService;
        }

        [HttpPost]
        [Route("/Add Exercise")]
        public async Task<IActionResult> AddExercise([FromBody] ExerciseDTO exercise)
        {
            var result = await _exerciseService.CreateExercise(exercise);
            if (result > 0)
            {
                return Ok(new { Message = "Exercise added successfully", Result = result });
            }
            return BadRequest(new { Message = "Failed to add exercise" });
        }

        [HttpDelete]
        [Route("/Delete Exercise")]
        public async Task<IActionResult> DeleteExercise([FromBody] Guid exerciseId)
        {
            var result = await _exerciseService.DeleteExercise(exerciseId);
            if (result > 0)
            {
                return Ok(new { Message = "Exercise deleted successfully", Result = result });
            }
            return BadRequest(new { Message = "Failed to delete exercise" });
        }

        [HttpPost]
        [Route("/Modify Exercise")]
        public async Task<IActionResult> ModifyExercise([FromBody] ExerciseDTO exercise)
        {
            var result = await _exerciseService.UpdateExercise(exercise);
            if(result > 0)
            {
                return Ok(new { Message = "Exercise modified successfully", Result = result });
            }
            return BadRequest(new { Message = "Failed to Modify Exercise" });
        }

        [HttpPost]
        [Route("/Add Muscles to Exercise")]
        public async Task<IActionResult> AddMusclesToExercise([FromBody] ExerciseMusclesDTO Muscles)
        {
            var result = await _exerciseService.AddMusclesToExercise(Muscles.ExerciseID, Muscles.Muscles);
            if (result > 0)
            {
                return Ok(new { Message = "Muscles added to exercise successfully", Result = result });
            }
            return BadRequest(new { Message = "Failed to add muscles to exercise" });
        }

        [HttpDelete]
        [Route("/Remove Muscles from Exercise")]
        public async Task<IActionResult> RemoveMusclesFromExercise([FromBody] ExerciseMusclesDTO Muscles)//fe 8alta hena
        {
            var result = await _exerciseService.RemoveMusclesFromExercise(Muscles.ExerciseID, Muscles.Muscles);
            if (result > 0)
            {
                return Ok(new { Message = "Muscles removed from exercise successfully", Result = result });
            }
            return BadRequest(new { Message = "Failed to remove muscles from exercise" });
        }
        [HttpPost]
        [Route("Get Exercise By Name")]
        public async Task<IActionResult> GetExerciseByName([FromBody] string name)
        {
            var result = await _exerciseService.GetExerciseByName(name);
            if (result == null) return BadRequest(new { Message = "Exercise not found" });
            return Ok(result);
        }
        [HttpPost]
        [Route("Get Exercise By Muscle")]
        public async Task<IActionResult> GetWorkoutsByMuscle([FromBody] ExerciseListDTO muscles)
        {
            var result = await _exerciseService.GetExercisesByMuscle(muscles);
            if(result == null) return BadRequest(new { Message = "Either you entered the muscles wrong or no Exercise was found." });
            return Ok(result);
        }
        [HttpGet]
        [Route("/Get All Exercises")]
        public async Task<IActionResult> GetAllExercises()
        {
            var result = await _exerciseService.GetAllExercises();
            return Ok(result);
        }
    }
}
