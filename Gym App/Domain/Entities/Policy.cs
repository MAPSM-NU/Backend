using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain.Entities
{
    public class Policy
    {
        [Key]
        public int PolicyID { get; set; }
        [Column(TypeName ="varchar(20)")]
        public string PolicyName { get; set; } = string.Empty;

        //Relationships
        public ICollection<User> Users { get; set; }

    }
    public enum PolicyType
    {
        Admin = 1,
        UserOnly = 2
    }
}
