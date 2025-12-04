using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Infastructure.DTOs.UserDTOs
{
    public class UserTypeDTO
    {
        public Guid UserID { get; set; }
        public string UserType { get; set; }
        public string? Specialty { get; set; }
        public int? ExperienceYears { get; set; }
        public string? Certifications { get; set; }
    }
}
