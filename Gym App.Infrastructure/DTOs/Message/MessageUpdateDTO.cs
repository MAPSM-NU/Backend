namespace Gym_App.Infastructure.DTOs.Message
{
    public class MessageUpdateDTO
    {
        public required string Content { get; set; }
        public bool IsRead { get; set; }
    }
}
