using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Infastructure.DTOs
{
    public class ExerciseDTO
    {
        public Guid ExerciseID { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Difficulty { get; set; }
        public string? VideoUrl { get; set; }
        public string? Category { get; set; }
        public string? Grip { get; set; }
    }
}
