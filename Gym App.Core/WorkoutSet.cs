using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Gym_App.Domain
{
    public class WorkoutSet : BaseEntity
    {

        [Range(1, int.MaxValue, ErrorMessage = "Set number must be at least 1.")]
        public required int SetNumber { get; set; }

        [Required]
        public int Reps { get; set; }
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 999.99, ErrorMessage = "Weight must be between 0 and 999.99.")]
        public decimal? Weight { get; set; }
        [Range(0, 9999, ErrorMessage = "Rest must be between 0 and 9999.")]
        public int? RestSeconds { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string? Notes { get; set; }
        public bool IsCompleted { get; set; } = false;
        [Range(0, 999, ErrorMessage = "Reps must be between 0 and 999")]
        //Reps done
        public int? ActualReps { get; set; }
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 999.99, ErrorMessage = "Weight must be between 0 and 999.99.")]
        public decimal? ActualWeight { get; set; }
        public int KCaloriesBurned {  get; set; }

        // Relationships
        public required Guid ExerciseInstanceId { get; set; }
        public ExerciseInstance? ExerciseInstance { get; set; }
    }
}
