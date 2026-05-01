namespace Gym_App.Infastructure.DTOs.Message
{
    public class MessageViewDTO
    {
        public Guid SenderID { get; set; }
        public Guid SessionID { get; set; }
        public Guid MessageID { get; set; }
        public required string Name { get; set; }
        public required string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; }
    }
}
