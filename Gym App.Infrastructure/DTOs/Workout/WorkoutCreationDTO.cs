
using Gym_App.Infrastructure.DTOs.Exercise;
using Gym_App.Infrastructure.DTOs.Workout;

namespace Gym_App.Infastructure.DTOs.WorkoutDTOs
{
    public class WorkoutCreationDTO
    {
        public Guid UserID { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public string? Type { get; set; }
        public int Duration { get; set; }
        public string? Difficulty { get; set; }
        public required string Day { get; set; }
        public string? ScheduledStartTime { get; set; }
        public IEnumerable<ExerciseDetailDTO> ExerciseDetails { get; set; }
    }
}
