using Gym_App.Domain.Entities;

namespace Gym_App.Domain.DTOs
{
    public class WorkoutExerciseDTO
    {
        public Guid WorkoutID { get; set; }
        public List<Guid> ExercisesID { get; set; } = new List<Guid>();
    }
}
