using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain.Entities
{
    public class Session
    {
        [Key]
        public Guid SessionID { get; set; }
        public DateTime StartTime { get; set; }
        //public string Title { get; set; }
        [Required]
        [Column(TypeName = "varchar(21)")]
        public string SessionType { get; set; } // Discriminator column

        //Relationships
        public ICollection<Message>? Messages { get; set; } = new List<Message>();
        public ICollection<User> Users { get; set;} = new List<User>();
    }
}
