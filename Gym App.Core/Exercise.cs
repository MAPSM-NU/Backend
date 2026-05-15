using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain
{
    public class Exercise : BaseEntity
    {
        [Column(TypeName = "varchar(100)")]
        public required string Name { get; set; }
        [Column(TypeName = "varchar(8000)")]
        public string? Description { get; set; }
        //[Required]
        //[Column(TypeName = "varchar(100)")]
        //public string MuscleGroup { get; set; }
        [Column(TypeName = "varchar(20)")]
        public string? Difficulty { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string? VideoUrl { get; set; }
        [Column(TypeName = "varchar(40)")]
        public string? Category { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string? Grip { get; set; }

        //Relationships
        public ICollection<Muscles>? Muscles { get; set; } = new List<Muscles>();
    }
}
