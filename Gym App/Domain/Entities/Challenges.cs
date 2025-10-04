using Gym_App.Domain.Entities.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain.Entities
{
    public class Challenges
    {
        [Key]
        public Guid ChallengeId { get; set; }
        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; }
        [Required]
        [Column(TypeName = "nvarchar(100)")]
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [Required]
        [Column(TypeName = "varchar(1000)")]
        public string Description { get; set; }
        [Required]
        public int RewardPoints { get; set; }

        // Relationships
        [Required]
        public ICollection<User> Participants { get; set; } = new List<User>();
    }
}
