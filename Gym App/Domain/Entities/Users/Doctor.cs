using Gym_App.Domain.Entities.Sessions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain.Entities.Users
{
    public class Doctor : User
    {
        [Column(TypeName = "varchar(100)")]
        public string? Specialty { get; set; }
        public int? ExperienceYears { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string? Certifications { get; set; }

        //Relationships
        public ICollection<DoctorTraineeSession>? DoctorTraineeSessions { get; set; } = new List<DoctorTraineeSession>();
    }
}
