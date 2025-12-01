using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain
{
    public class Notification
    {
        [Key]
        public Guid NotificationID { get; set; }
        public DateTime Date { get; set; }
        [Column(TypeName = "varchar(100)")]
        public required string Title { get; set; }
        [Column(TypeName = "varchar(1000)")]
        public required string? Content { get; set; }
        
        //Relationships
        public required User User { get; set; }
    }
}
