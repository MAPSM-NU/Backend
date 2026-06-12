
namespace Gym_App.Infrastructure.DTOs.User
{
    public class UserStatsDTO
    {
        public Guid userId { get; set; }
        public int totalWorkoutsCompleted { get; set; }
        public int totalWorkoutsMissed { get; set; }
        public int totalExercisesCompleted { get; set; }
        public double totalHours { get; set; } = 0;
        public int workoutStreak { get; set; } = 0;
        public int longestStreak { get; set; } = 0;
        public double workoutCompletionRate { get; set; }
    }
}
