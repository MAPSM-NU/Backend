namespace Gym_App.Domain.DTOs
{
    public class SessionDTO
    {
        public Guid SessionID { get; set; }
        public DateTime StartTime { get; set; }
        public List<Guid> UserIDs { get; set; } = new List<Guid>();
    }
}
