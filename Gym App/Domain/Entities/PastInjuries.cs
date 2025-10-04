using Gym_App.Domain.Entities.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain.Entities
{
    public class PastInjuries
    {
        [Key]
        public Guid InjuryID { get; set; }
        public DateTime InjuryDate { get; set; }
        [Required]
        [Column(TypeName = "varchar(1000)")]
        public string Description { get; set; }

        //Relationships
        public User? User { get; set; }
    }
}
