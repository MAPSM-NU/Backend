using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.DTOs.Muscle;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Application.Interfaces
{
    public interface IMuscleService
    {
        public Task<int> CreateMuscle(MuscleCreationAndEditDTO muscle);
        public Task<int> UpdateMuscle(Guid muscleID, MuscleCreationAndEditDTO muscle);
        public Task<int> DeleteMuscle(Guid muscleID);
        public Task<PagedList<MuscleViewDTO>> GetAllMuscles(int page=1, int pageSize=32);
    }
}
