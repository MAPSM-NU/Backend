using Gym_App.Domain;

namespace Gym_App.Core
{
    public class UserStatsMonthly : BaseEntity
    {
        public Guid userId { get; set; }
        public required User user { get; set; }
        public int totalWorkoutsCompleted { get; set; } = 0;
        public int totalWorkoutsMissed { get; set; } = 0;
        public int totalExercisesCompleted { get; set; } = 0;
        public double totalHours { get; set; } = 0;
        public int workoutStreak { get; set; } = 0;
        public int longestWorkoutStreak { get; set; } = 0;
        public double workoutCompletionRate { get; set; }
        public int activeDays { get; set; }
        public int totalSetsCompleted { get; set; }
        public int totalRepsCompleted { get; set; }
        public double totalWeightLifted { get; set; }
        public int personalRecordsBroken { get; set; }
        public bool weeklyGoalAchieved { get; set; }
        public double KcaloriesBurned { get; set; } = 0;
        public DateOnly monthDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public string monthName { get; set; } = DateTime.Now.ToString("MMMM");
        public int monthNumber { get; set; } = DateTime.Now.Month;
        public int year { get; set; } = DateTime.Now.Year;
        public ICollection<UserStatsWeekly> userStatsWeekly { get; set; } = new List<UserStatsWeekly>();
    }
}
