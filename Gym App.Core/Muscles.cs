using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain
{
    public class Muscles : BaseEntity
    {
        [Column(TypeName = "varchar(100)")]
        public required string Name { get; set; }
        [Column(TypeName = "varchar(1000)")]
        public string? Description { get; set; }

        // Relationships
        public ICollection<Exercise>? Exercises { get; set; } = new List<Exercise>();
    }
}
