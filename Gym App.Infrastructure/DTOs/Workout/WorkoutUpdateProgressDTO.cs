


using Gym_App.Infrastructure.DTOs.Exercise;

namespace Gym_App.Infrastructure.DTOs.Workout
{
    public class WorkoutUpdateProgressDTO
    {
        public Guid WorkoutId { get; set; }
        public DateTime? ActualStartTime { get; set; }
        public DateTime? ActualEndTime { get; set; }
        public bool IsCompleted { get; set; }
        public IEnumerable<ExerciseUpdateProgressDTO> Exercises { get; set; }
    }
}
