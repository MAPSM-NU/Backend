using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain
{
    public class RefreshTokens
    {
        [Key]
        public Guid RefreshTokenID { get; set; }
        public Guid UserID { get; set; }
        [Column(TypeName = "varchar(500)")]
        public required string RefreshToken { get; set; }
        public DateTime Expires { get; set; }

        //Relationships
        public User? User { get; set; }
    }
}
