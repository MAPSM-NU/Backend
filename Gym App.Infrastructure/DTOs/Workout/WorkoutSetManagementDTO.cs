using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym_App.Infrastructure.DTOs.Workout
{
    public class WorkoutSetManagementDTO
    {
        public string requestType { get; set; } = string.Empty;
        public Guid SetId { get; set; }
        public int Reps { get; set; }
        public int SetNumber { get; set; }
        public decimal? Weight { get; set; }
        public int? RestSeconds { get; set; }
        public string? Notes { get; set; }
        public bool IsCompleted { get; set; } = false;
    }
}
