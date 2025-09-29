using Gym_App.Domain.Entities.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain.Entities
{
    public class Feedback
    {
        [Key]
        public int FeedbackID { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        [Column(TypeName = "varchar(20)")]
        public string Type { get; set; }
        [Required]
        [Column(TypeName = "varchar(2000)")]
        public string FeedbackText { get; set; }

        //Relationships
        [Required]
        public User User { get; set; }
    }
}
