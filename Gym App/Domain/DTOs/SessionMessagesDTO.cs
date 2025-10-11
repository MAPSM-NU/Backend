namespace Gym_App.Domain.DTOs
{
    public class SessionMessagesDTO
    {
        public Guid SenderID { get; set; }
        public Guid SessionID { get; set; }
        public List<Guid>? Messages { get; set; } = new List<Guid>();
    }
}
