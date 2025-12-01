using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain
{
    public class Role
    {
        [Key]
        public int RoleID { get; set; }
        [Column(TypeName ="varchar(20)")]
        public string RoleName { get; set; } = string.Empty;

        //Relationships
        public ICollection<User> Users { get; set; } = new List<User>();

    }
}
