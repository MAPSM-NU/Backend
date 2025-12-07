namespace Gym_App.Infastructure.DTOs.WorkoutDTOs
{
    public class WokoutMiniViewDTO
    {
        public Guid UserID { get; set; }
        public Guid WorkoutID { get; set; }
        public required string Name { get; set; }
    }
}
