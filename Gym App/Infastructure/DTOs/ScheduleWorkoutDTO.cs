namespace Gym_App.Infastructure.DTOs
{
    public class ScheduleWorkoutDTO
    {
        public Guid ScheduleID { get; set; }
        public List<Guid> WorkoutsID { get; set; } = new List<Guid>();
    }
}
