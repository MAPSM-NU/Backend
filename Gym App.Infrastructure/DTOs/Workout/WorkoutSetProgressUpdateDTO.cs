
namespace Gym_App.Infrastructure.DTOs.Workout
{
    public class WorkoutSetProgressUpdateDTO
    {
        public Guid SetId { get; set; }
        public bool IsCompleted { get; set; }
        public int? ActualReps { get; set; }
        public int? ActualWeight { get; set; }
        public string? Notes { get; set; }
    }
}
