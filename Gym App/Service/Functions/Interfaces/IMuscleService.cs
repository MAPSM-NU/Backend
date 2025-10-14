using Gym_App.Domain.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Gym_App.Service.Functions.Interfaces
{
    public interface IMuscleService
    {
        public Task<int> CreateMuscle(MuscleDTO muscle);
        public Task<int> UpdateMuscle(MuscleDTO muscle);
        public Task<int> DeleteMuscle(Guid muscleID);
        public Task<List<MuscleDTO>> GetAllMuscles();
    }
}
