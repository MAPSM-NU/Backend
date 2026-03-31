using Gym_App.Domain;

namespace Gym_App.Infastructure.Interfaces.Repositries
{
    public interface IExerciseRepositry : IBaseRepositry<Exercise>
    {
        Task<IEnumerable<Exercise>> GetExercisesByIds(List<Guid> exerciseIDsToRemove);
        public Task<bool> isExerciseExist(Guid exerciseID);
        public Task<bool> isExerciseNameExist(string name);
    }
}
