using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.DTOs.Muscle;
using Gym_App.Infastructure.Transfer_Classes;
using System.Threading;

namespace Gym_App.Infastructure.Interfaces.Services
{
    public interface IMuscleService
    {
        public Task<SettersResponse> CreateMuscle(MuscleCreationAndEditDTO muscle, CancellationToken cancellationToken = default);
        public Task<SettersResponse> UpdateMuscle(Guid muscleID, MuscleCreationAndEditDTO muscle, CancellationToken cancellationToken = default);
        public Task<SettersResponse> DeleteMuscle(Guid muscleID, CancellationToken cancellationToken = default);
        public Task<GettersResponse<MuscleViewDTO>> GetAllMuscles(int page = 1, int pageSize = 32, CancellationToken cancellationToken = default);
    }
}
