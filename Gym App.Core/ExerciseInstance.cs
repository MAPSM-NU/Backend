using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain
{
    /// <summary>
    /// Represents a specific instance of an exercise within a workout with tracked sets
    /// </summary>
    public class ExerciseInstance : BaseEntity
    {
        public required Guid ExerciseId { get; set; }
        public Exercise? Exercise { get; set; }
        public required Guid WorkoutId { get; set; }
        public Workout? Workout { get; set; }
        public int ExerciseOrder { get; set; }
        public int? PlannedReps { get; set; }
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 999.99, ErrorMessage = "Weight must be between 0 and 999.99.")]
        public decimal? PlannedWeight { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsCompleted { get; set; } = false;

        [Column(TypeName = "varchar(500)")]
        public string? Notes { get; set; }

        // Relationships
        public ICollection<WorkoutSet>? Sets { get; set; } = new List<WorkoutSet>();
    }
}
