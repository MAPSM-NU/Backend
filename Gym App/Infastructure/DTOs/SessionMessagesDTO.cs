namespace Gym_App.Infastructure.DTOs
{
    public class SessionMessagesDTO
    {
        public Guid SenderID { get; set; }
        public Guid SessionID { get; set; }
        public List<Guid>? Messages { get; set; } = new List<Guid>();
    }
}
