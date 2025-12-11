using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.DTOs.Muscle;
using Gym_App.Infastructure.Transfer_Classes;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Application.Interfaces
{
    public interface IMuscleService
    {
        public Task<SettersResponse> CreateMuscle(MuscleCreationAndEditDTO muscle);
        public Task<SettersResponse> UpdateMuscle(Guid muscleID, MuscleCreationAndEditDTO muscle);
        public Task<SettersResponse> DeleteMuscle(Guid muscleID);
        public Task<PagedList<MuscleViewDTO>> GetAllMuscles(int page=1, int pageSize=32);
    }
}
