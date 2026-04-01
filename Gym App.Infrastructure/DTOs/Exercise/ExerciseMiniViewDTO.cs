namespace Gym_App.Infastructure.DTOs.Exercise
{
    public class ExerciseMiniViewDTO
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Difficulty { get; set; }
        public required string VideoUrl { get; set; }
        public required string Category { get; set; }
        public required string Grip { get; set; }
    }
}
