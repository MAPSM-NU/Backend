using Gym_App.Domain.Entities.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain.Entities
{
    public class LiveFeedback
    {
        [Key]
        public int LiveID { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        [Column(TypeName = "varchar(20)")]
        public string FeedbackText { get; set; }

        //Relationships
        [Required]
        public User User { get; set; }
    }
}
