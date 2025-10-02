using Gym_App.Domain.DTOs;
using Gym_App.Service.Functions.Interfaces;

namespace Gym_App.Service.Functions.The_Applied
{
    public class MuscleService : IMuscleService
    {
        private readonly DbBase _db;
        public MuscleService(DbBase db)
        {
            _db = db;
        }
        public async Task<int> CreateMuscle(MuscleDTO muscle)
        {
            if(muscle == null || string.IsNullOrEmpty(muscle.Name)) return await Task.FromResult(0);
            var isMuscleExists = (from M in _db.Muscles
                                  where M.Name.ToLower() == muscle.Name.ToLower()
                                  select M).FirstOrDefault();
            if(isMuscleExists != null) return await Task.FromResult(0);
            var newMuscle = new Domain.Entities.Muscles
            {
                MusclesID = Guid.NewGuid(),
                Name = muscle.Name,
                Description = muscle.Description
            };
            _db.Muscles.Add(newMuscle);
            await _db.SaveChangesAsync();
            return await Task.FromResult(1);
        }
        public async Task<int> DeleteMuscle(Guid muscleID)
        {
            if(muscleID == Guid.Empty) return await Task.FromResult(0);
            var isMuscleExists = (from M in _db.Muscles
                                  where M.MusclesID == muscleID
                                  select M).FirstOrDefault();
            if (isMuscleExists == null) return await Task.FromResult(0);
            _db.Muscles.Remove(isMuscleExists);
            await _db.SaveChangesAsync();
            return await Task.FromResult(1);
        }
        public async Task<IQueryable<Domain.Entities.Muscles>> GetAllMuscles()
        {
            var muscles = from m in _db.Muscles
                          select m;
            return await Task.FromResult(muscles);
        }
        public async Task<int> UpdateMuscle(MuscleDTO muscle)
        {
            var toBeUpdated = (from M in _db.Muscles
                               where M.MusclesID == muscle.MusclesID
                               select M).FirstOrDefault();
            if (toBeUpdated == null) return await Task.FromResult(0);
            if(!string.IsNullOrEmpty(muscle.Name)) toBeUpdated.Name = muscle.Name;
            if(!string.IsNullOrEmpty(muscle.Description)) toBeUpdated.Description = muscle.Description;
            _db.Muscles.Update(toBeUpdated);
            await _db.SaveChangesAsync();
            return await Task.FromResult(1);
        }
    }
}
