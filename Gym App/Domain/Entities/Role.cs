using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain.Entities
{
    public class Role
    {
        [Key]
        public int RoleID { get; set; }
        [Column(TypeName ="varchar(20)")]
        public string RoleName { get; set; } = string.Empty;

        //Relationships
        public ICollection<User> Users { get; set; }

    }
    public enum RoleType
    {
        Admin = 1,
        User = 2
    }
}
