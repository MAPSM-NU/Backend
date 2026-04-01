using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain
{
    public class Message : BaseEntity
    {
        [Column(TypeName = "varchar(5000)")]
        public required string Content { get; set; }
        public bool IsRead { get; set; } = false;

        // Relationships
        public required User Sender { get; set; }
        public required Session Session { get; set; }
        }
}
