using Gym_App.Infrastructure.DTOs.Workout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gym_App.Infrastructure.DTOs.Exercise
{
    public class ExerciseManagementDTO
    {
        public string requestType { get; set; } = string.Empty;
        public Guid ExerciseId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PlannedReps { get; set; }
        public int PlannedWeight { get; set; }
        public int ExerciseOrder { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsCompleted { get; set; }
        public IEnumerable<WorkoutSetManagementDTO> Sets { get; set; } = new List<WorkoutSetManagementDTO>();
    }
    public enum ExerciseState
    {
        NotStarted,
        InProgress,
        Completed
    }
    public static class requestType
    {
        public const string Create = "create";
        public const string Update = "update";
        public const string Delete = "delete";
        public static bool IsValid(string type)
        {
            return Create.Contains(type) || Update.Contains(type) || Delete.Contains(type);
        }
    }
}
