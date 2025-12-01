using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain
{
    public class PastInjuries
    {
        [Key]
        public Guid InjuryID { get; set; }
        [Column(TypeName = "varchar(1000)")]
        public required string Description { get; set; }
        public DateTime InjuryDate { get; set; }

        //Relationships
        public User? User { get; set; }
    }
}
