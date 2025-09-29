using Gym_App.Domain.Entities.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain.Entities
{
    public class Schedule
    {
        [Key]
        public int ScheduleID { get; set; }
        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; }
        [Required]
        [Column(TypeName = "varchar(50)")]
        public string Type { get; set; }

        //Relationships
        [Required]
        public ICollection<Workout> Workouts { get; set; } = new List<Workout>();
        [Required]
        public User User { get; set; }
    }
}
