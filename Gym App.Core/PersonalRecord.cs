using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain
{
    /// <summary>
    /// Tracks personal records for each exercise
    /// </summary>
    public class PersonalRecord : BaseEntity
    {
        public required Guid UserId { get; set; }
        public User? User { get; set; }
        public required Guid ExerciseId { get; set; }
        public Exercise? Exercise { get; set; }
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 999.99, ErrorMessage = "Weight must be between 0 and 999.99.")]
        public required decimal Weight { get; set; }
        public required int Reps { get; set; }
        public required DateTime AchievedDate { get; set; }
        public Guid? WorkoutSetId { get; set; }
        public WorkoutSet? WorkoutSet { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string? Notes { get; set; }
        public bool NotificationSent { get; set; } = false;
    }
}
