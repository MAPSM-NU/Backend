

using Gym_App.Domain;

namespace Gym_App.Infastructure.Interfaces.Repositries
{
    public interface IMuscleRepositry : IBaseRepositry<Muscles>
    {
        public Task<bool> isMuscleExist(string name, CancellationToken cancellationToken = default);
        public Task<Muscles> getMuscleByName(string name, CancellationToken cancellationToken = default);
    }
}
