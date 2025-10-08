using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain.Entities
{
    public class Notification
    {
        [Key]
        public Guid NotificationID { get; set; }
        public DateTime Date { get; set; }
        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Title { get; set; }
        [Required]
        [Column(TypeName = "varchar(1000)")]
        public string? Content { get; set; }
        
        //Relationships
        public ICollection<User>? User { get; set; } = new List<User>();
    }
}
