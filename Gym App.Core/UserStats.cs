using Gym_App.Domain;

namespace Gym_App.Core
{
    public class UserStats : BaseEntity
    {
        public Guid userId { get; set; }
        public required User user { get; set; }
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
