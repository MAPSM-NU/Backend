using Gym_App.Domain.Entities;

namespace Gym_App.Infastructure.DTOs.WorkoutDTOs
{
    public class WorkoutExerciseDTO
    {
        public List<Guid> ExercisesID { get; set; } = new List<Guid>();
    }
}
