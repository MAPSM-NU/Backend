
using Gym_App.Domain;
using System.Globalization;

namespace Gym_App.Infrastructure.DTOs.UserStats
{
    public class UserStatsWeeklyDTO
    {
        public Guid userId { get; set; }
        public int totalWorkoutsCompleted { get; set; } = 0;
        public int totalWorkoutsMissed { get; set; } = 0;
        public int totalExercisesCompleted { get; set; } = 0;
        public double totalHours { get; set; } = 0;
        public int workoutStreak { get; set; } = 0;
        public double workoutCompletionRate { get; set; }
        public int activeDays { get; set; }
        public int totalSetsCompleted { get; set; }
        public int totalRepsCompleted { get; set; }
        public double totalWeightLifted { get; set; }
        public int personalRecordsBroken { get; set; }
        public bool weeklyGoalAchieved { get; set; }
        public double KcaloriesBurned { get; set; } = 0;
        public DateOnly weekDate { get; set; }
        public int year { get; set; }
        public int weekNumber { get; set; }
    }
}
