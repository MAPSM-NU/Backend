using Gym_App.Domain.Entities;

namespace Gym_App.Domain.DTOs
{
    public class ExerciseMusclesListDTO // used to either add muscles to an exercise or remove muscles from an exercise
    {
        public Guid ExerciseID { get; set; }
        public List<Guid> Muscles { get; set; } = new List<Guid>();
    }
}
