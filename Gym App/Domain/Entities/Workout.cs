using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain.Entities
{
    public class Workout
    {
        [Key]
        public Guid WorkoutID { get; set; }
        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; }
        [Column(TypeName = "varchar(1000)")]
        public string? Description { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        [Column(TypeName = "varchar(20)")]
        public string Difficulty { get; set; }
        [Required]
        [Column(TypeName = "varchar(15)")]
        public string Day { get; set; }

        //Relationships

        public Schedule? Schedule { get; set; }
        public ICollection<Exercise>? Exercises { get; set; } = new List<Exercise>();
        public User User { get; set; }
        public Feedback? Feedback { get; set; }
        }
    }
