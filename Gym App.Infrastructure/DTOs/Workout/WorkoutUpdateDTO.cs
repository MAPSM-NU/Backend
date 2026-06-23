namespace Gym_App.Infastructure.DTOs.WorkoutDTOs
{
    public class WorkoutUpdateDTO
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public DateTime? Date { get; set; }
        public string? Difficulty { get; set; }
        public required string Day { get; set; }
    }
}
