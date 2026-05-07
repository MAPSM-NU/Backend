


using Gym_App.Infrastructure.DTOs.Exercise;

namespace Gym_App.Infrastructure.DTOs.Workout
{
    public class WorkoutUpdateProgressDTO
    {
        public DateTime ActualStartTime;
        public DateTime ActualEndTime;
        public bool IsCompleted;
        public Guid WorkoutId { get; set; }
        public IEnumerable<ExerciseDetailDTO> Exercises { get; set; }
    }
}
