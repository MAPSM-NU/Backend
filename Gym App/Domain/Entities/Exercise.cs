using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain.Entities
{
    public class Exercise
    {
        [Key]
        public Guid ExerciseID { get; set; }
        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; }
        [Column(TypeName = "varchar(1000)")]
        public string? Description { get; set; }
        //[Required]
        //[Column(TypeName = "varchar(100)")]
        //public string MuscleGroup { get; set; }
        [Column(TypeName = "varchar(20)")]
        public string? Difficulty { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string? VideoUrl { get; set; }

        //Relationships
        public ICollection<Muscles>? Muscles { get; set; } = new List<Muscles>();
        public ICollection<Workout>? Workouts { get; set; } = new List<Workout>();
    }
}
