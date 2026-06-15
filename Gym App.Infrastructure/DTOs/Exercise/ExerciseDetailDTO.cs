
using Gym_App.Infrastructure.DTOs.Workout;

namespace Gym_App.Infrastructure.DTOs.Exercise
{
    public class ExerciseDetailDTO
    {
        public Guid Id { get; set; }
        public Guid ExerciseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PlannedReps { get; set; }
        public float PlannedWeight { get; set; }
        public string Notes { get; set; } = string.Empty;
        public int ExerciseOrder { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsCompleted { get; set; }
        public required IEnumerable<string> Muscles { get; set; }
        public IEnumerable<WorkoutSetDTO> Sets { get; set; }
    }
}
