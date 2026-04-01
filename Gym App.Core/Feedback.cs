using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain
{
    public class Feedback : BaseEntity
    {
        [Column(TypeName = "varchar(100)")]
        public required string Title { get; set; }
        [Column(TypeName = "varchar(20)")]
        public required string Type { get; set; }
        [Column(TypeName = "varchar(2000)")]
        public required string FeedbackText { get; set; }
        public int? CaloriesBurned { get; set; }
        public int? DurationMinutes { get; set; }


        //Relationships
        public required User User { get; set; }
        public required Workout Workout { get; set; }
        public Guid WorkoutID { get; set; }
    }
}
