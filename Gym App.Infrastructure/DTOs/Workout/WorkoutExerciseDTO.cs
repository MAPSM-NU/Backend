using Gym_App.Domain.Entities;
using Gym_App.Infrastructure.DTOs.Exercise;

namespace Gym_App.Infastructure.DTOs.WorkoutDTOs
{
    public class WorkoutExerciseDTO
    {
        public IEnumerable<ExerciseDetailDTO> exercisesDetails { get; set; }
    }
}
