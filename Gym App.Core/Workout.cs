using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain
{
    public class Workout : BaseEntity
    {
        [Column(TypeName = "varchar(100)")]
        public required string Name { get; set; }
        [Column(TypeName = "varchar(1000)")]
        public string? Description { get; set; }
        public required DateTime Date { get; set; }
        [Column(TypeName = "varchar(20)")]
        public string? Difficulty { get; set; }
        [Column(TypeName = "varchar(15)")]
        public required string Day { get; set; }
        [Column(TypeName = "varchar(20)")]
        public required string Type { get; set; }
        public int CaloriesBurned { get; set; }
        public int Duration { get; set; } // Duration in minutes
        public int DurationRemaining { get; set; }
        public bool hasStarted { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? ScheduledStartTime { get; set; }
        public DateTime? ScheduledEndTime { get; set; }
        public DateTime? ActualStartTime { get; set; }
        public DateTime? ActualEndTime { get; set; }
        public bool NotificationSent { get; set; } = false;
        public bool ReminderSent { get; set; } = false;
        public bool isMissed { get; set; } = false;

        //Relationships

        public Schedule? Schedule { get; set; }
        public ICollection<ExerciseInstance>? ExerciseInstances { get; set; } = new List<ExerciseInstance>();
        public required User User { get; set; }
        public Feedback? Feedback { get; set; }
        }
    }
