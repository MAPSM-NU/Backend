namespace Gym_App.Infastructure.DTOs.Message
{
    public class MessageMiniViewDTO
    {
        public Guid SenderID { get; set; }
        public Guid MessageID { get; set; }
        public required string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; }
    }
}
