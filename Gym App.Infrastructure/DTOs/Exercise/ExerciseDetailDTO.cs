
using Gym_App.Infrastructure.DTOs.Workout;

namespace Gym_App.Infrastructure.DTOs.Exercise
{
    public class ExerciseDetailDTO
    {
        public Guid ExerciseId { get; set; }
        public int PlannedReps { get; set; }
        public int PlannedWeight { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsCompleted { get; set; }
        public IEnumerable<WorkoutSetDTO> Sets { get; set; }
    }
}
