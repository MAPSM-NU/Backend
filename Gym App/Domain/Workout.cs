using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain
{
    public class Workout
    {
        [Key]
        public Guid WorkoutID { get; set; }
        [Column(TypeName = "varchar(100)")]
        public required string Name { get; set; }
        [Column(TypeName = "varchar(1000)")]
        public string? Description { get; set; }
        public required DateTime Date { get; set; }
        [Column(TypeName = "varchar(20)")]
        public string? Difficulty { get; set; }
        [Column(TypeName = "varchar(15)")]
        public required string Day { get; set; }
        public DateTime CreatAt { get; set; }

        //Relationships

        public Schedule? Schedule { get; set; }
        public ICollection<Exercise>? Exercises { get; set; } = new List<Exercise>();
        public required User User { get; set; }
        public Feedback? Feedback { get; set; }
        }
    }
