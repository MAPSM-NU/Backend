using Gym_App.Domain;
using System;
using System.Collections.Generic;

namespace Gym_App.Infrastructure.DTOs.UserStats
{
    public class UserStatsMonthlyDTO
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
        public double IKcaloriesBurned { get; set; } = 0;
        public DateOnly monthDate { get; set; }
        public string monthName { get; set; }
        public int monthNumber {  get; set; }
        public int year { get; set; }
    }
}
