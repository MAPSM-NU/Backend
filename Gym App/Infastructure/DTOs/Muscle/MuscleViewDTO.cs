using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Infastructure.DTOs.Muscle
{
    public class MuscleViewDTO
    {
        public Guid MusclesID { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
    }
}
