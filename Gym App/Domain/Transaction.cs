using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain
{
    public class Transaction
    {
        [Key]
        public Guid PaymentID { get; set; }
        public required DateTime Date { get; set; }
        public decimal Amount { get; set; }
        [Column(TypeName = "varchar(1000)")]
        public required string Description { get; set; }
        [Column(TypeName = "varchar(50)")]
        public required string PaymentType { get; set; }
    }
}
