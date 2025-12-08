namespace Gym_App.Infastructure.DTOs.Message
{
    public class MessageCreationDTO
    {
        public Guid SessionID { get; set; }
        public required string Content { get; set; }
        public bool IsRead { get; set; }
    }
}
