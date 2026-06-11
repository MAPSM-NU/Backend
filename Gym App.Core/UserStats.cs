using Gym_App.Domain;

namespace Gym_App.Core
{
    public class UserStats : BaseEntity
    {
        public Guid userId { get; set; }
        public required User user { get; set; }
        public int totalWorkoutsCompleted { get; set; } = 0;
        public int totalWorkoutsMissed { get; set; } = 0;
        public int totalExercisesCompleted { get; set; } = 0;
        public double totalHours { get; set; } = 0;
        public int workoutStreak { get; set; } = 0;
        public int longestStreak { get; set; } = 0;
        public double workoutCompletionRate { get; set; }
    }
}
