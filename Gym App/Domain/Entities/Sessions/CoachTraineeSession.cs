using Gym_App.Domain.Entities.Users;
using System.ComponentModel.DataAnnotations;

namespace Gym_App.Domain.Entities.Sessions
{
    public class CoachTraineeSession : Session
    {
        public Coach? Coach { get; set; }
        public Trainee? Trainee { get; set; }
    }
}
