using Gym_App.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym_App.Infrastructure.DTOs.UserStats
{
    public class UserStatsDailyDTO
    {
        public Guid userId { get; set; }
        public required User user { get; set; }
        public int totalExercisesCompleted { get; set; } = 0;
        public double totalHours { get; set; } = 0;
        public DateOnly date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public string dayOfWeek { get; set; } = DateTime.Now.DayOfWeek.ToString();
        public int year { get; set; } = DateTime.Now.Year;
    }
}
