using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain
{
    public class Schedule
    {
        [Key]
        public Guid ScheduleID { get; set; }
        [Column(TypeName = "varchar(100)")]
        public required string Name { get; set; }
        [Column(TypeName = "varchar(50)")]
        public required string Type { get; set; }
        public DateTime CreatedAt { get; set; }

        //Relationships
        public ICollection<Workout>? Workouts { get; set; } = new List<Workout>();
        public required User User { get; set; }
    }
}
