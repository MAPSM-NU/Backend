using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain.Entities
{
    public class Feedback
    {
        [Key]
        public Guid FeedbackID { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required] 
        [Column(TypeName = "varchar(100)")]
        public string Title { get; set; }
        [Required]
        [Column(TypeName = "varchar(20)")]
        public string Type { get; set; }
        [Required]
        [Column(TypeName = "varchar(2000)")]
        public string FeedbackText { get; set; }
        public int? CaloriesBurned { get; set; }
        public int? DurationMinutes { get; set; }


        //Relationships
        [Required]
        public User User { get; set; }
        [Required]
        public Workout Workout { get; set; }
        public Guid WorkoutID { get; set; }
    }
}
