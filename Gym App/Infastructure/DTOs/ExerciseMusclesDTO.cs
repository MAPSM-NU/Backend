using Gym_App.Domain.Entities;

namespace Gym_App.Infastructure.DTOs
{
    public class ExerciseMusclesDTO // used to either add muscles to an exercise or remove muscles from an exercise
    {
        public Guid ExerciseID { get; set; }
        public List<Guid> Muscles { get; set; } = new List<Guid>();
    }
}
