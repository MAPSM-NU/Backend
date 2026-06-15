using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym_App.Infrastructure.DTOs.Workout
{
    public class WorkoutSetDTO
    {
        public Guid SetId { get; set; }
        public int Reps { get; set; }
        public int SetNumber { get; set; }
        public float? Weight { get; set; }
        public int? RestSeconds { get; set; }
        public string? Notes { get; set; }
        public bool IsCompleted { get; set; } = false;
        public int? ActualReps { get; set; }
        public float? ActualWeight { get; set; }
        public int KcaloriesBurned {  get; set; }

    }
}
