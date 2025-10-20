using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gym_App.Service.Functions.The_Applied
{
    public class MuscleService : IMuscleService
    {
        private readonly DbBase _db;
        public MuscleService(DbBase db)
        {
            _db = db;
        }
        public async Task<int> CreateMuscle(MuscleDTO muscle)// 0 == faulty DTO || 1 == muscle alr exists || 2 == success
        {
            if(muscle == null || string.IsNullOrEmpty(muscle.Name)) return await Task.FromResult(0);
            var isMuscleExists = await (from M in _db.Muscles
                                  where M.Name.ToLower() == muscle.Name.ToLower()
                                  select M).FirstOrDefaultAsync();
            if(isMuscleExists != null) return 1;
            var newMuscle = new Muscles
            {
                MusclesID = Guid.NewGuid(),
                Name = muscle.Name,
                Description = muscle.Description
            };
            await _db.Muscles.AddAsync(newMuscle);
            await _db.SaveChangesAsync();
            return 2;
        }
        public async Task<int> UpdateMuscle(MuscleDTO muscle)
        {
            var toBeUpdated = await (from M in _db.Muscles
                               where M.MusclesID == muscle.MusclesID
                               select M).FirstOrDefaultAsync();
            if (toBeUpdated == null) return 0;
            if(!string.IsNullOrEmpty(muscle.Name)) toBeUpdated.Name = muscle.Name;
            if(!string.IsNullOrEmpty(muscle.Description)) toBeUpdated.Description = muscle.Description;
            _db.Muscles.Update(toBeUpdated);
            await _db.SaveChangesAsync();
            return 1;
        }
        public async Task<int> DeleteMuscle(Guid muscleID)
        {
            if(muscleID == Guid.Empty) return 0;
            var isMuscleExists = (from M in _db.Muscles
                                  where M.MusclesID == muscleID
                                  select M).FirstOrDefault();
            if (isMuscleExists == null) return 0;
            _db.Muscles.Remove(isMuscleExists);
            await _db.SaveChangesAsync();
            return 11;
        }
        public async Task<PagedList<MuscleDTO>> GetAllMuscles(int page,int pageSize)
        {
            if (page == 0) page = 1;
            if (pageSize == 0) pageSize = 15;//Returning all the muscles in the database
            var musclesQuery = (from m in _db.Muscles
                          select new MuscleDTO
                          {
                                MusclesID = m.MusclesID,
                                Name = m.Name,
                                Description = m.Description
                          });
            var muscles = await PagedList<MuscleDTO>.CreateAsync(musclesQuery, page, pageSize);
            return muscles;
        }
    }
}
