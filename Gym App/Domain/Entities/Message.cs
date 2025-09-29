using Gym_App.Domain.Entities.Sessions;
using Gym_App.Domain.Entities.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain.Entities
{
    public class Message
    {
        [Key]
        public Guid MessageId { get; set; }
        [Required]
        [Column(TypeName = "varchar(5000)")]
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; } = false;

        // Relationships
        [Required]
        public User Sender { get; set; }
        [Required]
        public Session Session { get; set; }
        }
}
