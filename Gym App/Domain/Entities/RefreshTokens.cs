using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain.Entities
{
    public class RefreshTokens
    {
        [Key]
        public Guid RefreshTokenID { get; set; }
        public Guid UserID { get; set; }
        [Required]
        [Column(TypeName = "varchar(500)")]
        public string RefreshToken { get; set; }
        public DateTime Expires { get; set; }
        //Relationships
        [Required]
        public User User { get; set; }
    }
}
