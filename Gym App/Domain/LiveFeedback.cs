using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain
{
    public class LiveFeedback
    {
        [Key]
        public Guid LiveID { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Column(TypeName = "varchar(20)")]
        public required string FeedbackText { get; set; }

        //Relationships
        [Required]
        public required User User { get; set; }
    }
}
