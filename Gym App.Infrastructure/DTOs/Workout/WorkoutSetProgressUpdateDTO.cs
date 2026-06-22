
namespace Gym_App.Infrastructure.DTOs.Workout
{
    public class WorkoutSetProgressUpdateDTO
    {
        public Guid SetId { get; set; }
        public bool IsCompleted { get; set; }
        public required int ActualReps { get; set; }
        public required float ActualWeight { get; set; }
        public int KCaloriesBurned {  get; set; }
        public string? Notes { get; set; }
    }
}
