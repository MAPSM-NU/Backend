using System.ComponentModel.DataAnnotations;

namespace Gym_App.Domain.Entities.Sessions
{
    public class Session
    {
        [Key]
        public Guid SessionID { get; set; }
        public DateTime StartTime { get; set; }

        //Relationships
        public ICollection<Message>? Messages { get; set; } = new List<Message>();
    }
}
