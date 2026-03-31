using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain
{
    public class Challenges : BaseEntity
    {
        [Column(TypeName = "varchar(100)")]
        public required string Name { get; set; }
        [Column(TypeName = "varchar(100)")]
        public required string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [Column(TypeName = "varchar(1000)")]
        public required string Description { get; set; }
        public required int RewardPoints { get; set; }

        // Relationships
        [Required]
        public ICollection<User> Participants { get; set; } = new List<User>();
    }
}
