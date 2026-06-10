
namespace Gym_App.Infrastructure.DTOs.User
{
    public class UserStatsDTO
    {
        public Guid userId { get; set; }
        public int totalWorkouts { get; set; }
        public int totalExercises { get; set; }
        public double totalHours { get; set; } = 0;
        public int workoutStreak { get; set; } = 0;
        public int longestStreak { get; set; } = 0;
        public int goalsCompleted { get; set; }
        public int goalsFailed { get; set; }
        public double goalCompletionRate { get; set; }
    }
}
