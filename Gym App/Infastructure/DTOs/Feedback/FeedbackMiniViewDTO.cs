namespace Gym_App.Infastructure.DTOs.Feedback
{
    public class FeedbackMiniViewDTO
    {
        public Guid? WorkoutID { get; set; }
        public Guid FeedbackID { get; set; }
        public required string Title { get; set; }
        public required string Type { get; set; }
        public required string FeedbackText { get; set; }
        public int CaloriesBurned { get; set; }
        public int DurationMinutes { get; set; }
        public DateTime Date { get; set; }
    }
}

