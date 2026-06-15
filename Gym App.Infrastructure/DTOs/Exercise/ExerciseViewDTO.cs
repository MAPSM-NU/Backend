
namespace Gym_App.Infastructure.DTOs.Exercise
{
    public class ExerciseViewDTO
    {
        public Guid ExerciseID { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Difficulty { get; set; }
        public string? VideoUrl { get; set; }
        public string? Category { get; set; }
        public string? Grip { get; set; }
        public required IEnumerable<string> Muscles { get; set; }
    }
}
