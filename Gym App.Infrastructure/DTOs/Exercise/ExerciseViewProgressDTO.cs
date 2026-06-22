
using Gym_App.Infrastructure.DTOs.Workout;

namespace Gym_App.Infrastructure.DTOs.Exercise
{
    public class ExerciseViewProgressDTO
    {
        public Guid ExerciseInstanceId { get; set; }
        public Guid ExerciseID { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Difficulty { get; set; }
        public string? VideoUrl { get; set; }
        public string? Category { get; set; }
        public string? Grip { get; set; }
        public required IEnumerable<string> Muscles { get; set; }
        public DateTime? StartedAt { get; set; }
        public bool IsCompleted { get; set; }
        public List<WorkoutSetProgressUpdateDTO> Sets { get; set; } = new List<WorkoutSetProgressUpdateDTO>();
    }
}
