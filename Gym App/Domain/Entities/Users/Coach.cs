using Gym_App.Domain.Entities.Sessions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gym_App.Domain.Entities.Users
{
    public class Coach : User
    {
        [Column(TypeName = "varchar(100)")]
        public string? Specialty { get; set; }
        public int? ExperienceYears { get; set; }
        [Column(TypeName = "varchar(500)")]
        public string? Certifications { get; set; }

        //Relationships
        public ICollection<CoachTraineeSession>? CoachTraineeSessions { get; set; } = new List<CoachTraineeSession>();
    }
}
