using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Infastructure.DTOs
{
    public class FeedbackDTO
    {
        public Guid UserID { get; set; }
        public Guid? WorkoutID { get; set; }
        public Guid FeedbackID { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string FeedbackText { get; set; }
        public int CaloriesBurned { get; set; }
        public int DurationMinutes { get; set; }
    }
}
