using Gym_App.Infrastructure.DTOs.Workout;

namespace Gym_App.Infrastructure.DTOs.Exercise
{
    public class ExerciseUpdateProgressDTO
    {
        public Guid ExerciseId { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsCompleted { get; set; }
        public IEnumerable<WorkoutSetProgressUpdateDTO> Sets { get; set; }
    }
}
