using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain
{
    public class Session : BaseEntity
    {
        //public string Title { get; set; }
        [Column(TypeName = "varchar(21)")]
        public required string SessionType { get; set; } // Discriminator column

        //Relationships
        public ICollection<Message>? Messages { get; set; } = new List<Message>();
        public ICollection<User> Users { get; set;} = new List<User>();
    }
}
