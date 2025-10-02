namespace Gym_App.Domain.DTOs
{
    public class ScheduleWorkoutDTO
    {
        public Guid ScheduleID { get; set; }
        public List<Guid> WorkoutsID { get; set; } = new List<Guid>();
    }
}
