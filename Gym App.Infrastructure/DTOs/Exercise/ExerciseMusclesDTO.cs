
namespace Gym_App.Infastructure.DTOs.Exercise
{
    public class ExerciseMusclesDTO // used to either add muscles to an exercise or remove muscles from an exercise
    {
        public List<Guid> Muscles { get; set; } = new List<Guid>();
    }
}
