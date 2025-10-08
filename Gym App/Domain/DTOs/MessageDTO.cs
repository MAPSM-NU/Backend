using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain.DTOs
{
    public class MessageDTO
    {
        public Guid SenderID { get; set; }
        public Guid SessionID { get; set; }
        public Guid MessageID { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; } = false;
    }
}
