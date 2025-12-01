using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Infastructure.DTOs
{
    public class WorkoutDTO
    {
        public Guid UserID { get; set; }
        public Guid WorkoutID { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public string Difficulty { get; set; }
        public string Day { get; set; }
    }
}
