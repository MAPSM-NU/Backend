namespace Gym_App.Infastructure.DTOs
{
    public class SessionDTO
    {
        public Guid SessionID { get; set; }
        public DateTime StartTime { get; set; }
        public List<Guid> UserIDs { get; set; } = new List<Guid>();
    }
}
