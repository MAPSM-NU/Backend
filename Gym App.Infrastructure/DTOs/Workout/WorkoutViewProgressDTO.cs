using Gym_App.Infrastructure.DTOs.Exercise;

namespace Gym_App.Infrastructure.DTOs.Workout
{
    public class WorkoutViewProgressDTO
    {
        public Guid workoutId {  get; set; }
        public bool isCompleted { get; set; }
        public bool isMissed { get; set; }
        public DateTime? StartedAt { get; set; }
        public List<ExerciseViewProgressDTO> exercises { get; set; } = new List<ExerciseViewProgressDTO>();
    }
}
