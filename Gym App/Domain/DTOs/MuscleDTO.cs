using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain.DTOs
{
    public class MuscleDTO
    {
        public Guid MusclesID { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
