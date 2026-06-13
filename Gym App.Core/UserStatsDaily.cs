
using Gym_App.Domain;

namespace Gym_App.Core
{
    public class UserStatsDaily : BaseEntity
    {
        public Guid userId { get; set; }
        public required User user { get; set; }
        public int totalExercisesCompleted { get; set; } = 0;
        public double totalHours { get; set; } = 0;
        public DateOnly date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public string dayOfWeek { get; set; } = DateTime.Now.DayOfWeek.ToString();
        public int year { get; set; } = DateTime.Now.Year;
        public double KcaloriesBurned { get; set; } = 0;
    }
}
