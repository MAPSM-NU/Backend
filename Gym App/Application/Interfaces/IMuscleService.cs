using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Application.Interfaces
{
    public interface IMuscleService
    {
        public Task<int> CreateMuscle(MuscleDTO muscle);
        public Task<int> UpdateMuscle(MuscleDTO muscle);
        public Task<int> DeleteMuscle(Guid muscleID);
        public Task<PagedList<MuscleDTO>> GetAllMuscles(int page=1, int pageSize=32);
    }
}
