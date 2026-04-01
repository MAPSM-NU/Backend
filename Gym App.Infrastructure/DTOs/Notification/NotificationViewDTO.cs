using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Infastructure.DTOs.Notification
{
    public class NotificationViewDTO
    {
        public Guid UserID { get; set; }
        public Guid NotificationID { get; set; }
        public DateTime Date { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
    }
}
