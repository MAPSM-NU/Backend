using Gym_App.Domain.Entities.Users;
using System.ComponentModel.DataAnnotations;

namespace Gym_App.Domain.Entities.Sessions
{
    public class DoctorTraineeSession : Session
    {
        public Doctor? Doctor { get; set; }
        public Trainee? Trainee { get; set; }
    }
}
