namespace Gym_App.Infastructure.DTOs.Notification
{
    public class NotificationMiniViewDTO
    {
        public Guid NotificationID { get; set; }
        public DateTime Date { get; set; }
        public required string Title { get; set; }
        public string? Content { get; set; }
    }
}
