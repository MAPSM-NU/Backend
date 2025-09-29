using Gym_App.Domain.Entities.Sessions;
using System.ComponentModel.DataAnnotations;

namespace Gym_App.Domain.Entities.Users
{
    public class Trainee : User
    {
        public int? HeightCm { get; set; }
        public int? WeightKg { get; set; }
        // Just put wtv the ai gave for now

        //Relationships
        public ICollection<CoachTraineeSession>? CoachTraineeSessions { get; set; } = new List<CoachTraineeSession>();
        public ICollection<DoctorTraineeSession>? DoctorTraineeSessions { get; set; } = new List<DoctorTraineeSession>();
    }
}
