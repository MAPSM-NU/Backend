using Gym_App.Application.Interfaces;
using Gym_App.Domain;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.DTOs.Muscle;
using Microsoft.EntityFrameworkCore;

namespace Gym_App.Application.Services
{
    public class MuscleService : IMuscleService
    {
        private readonly DbBase _db;
        public MuscleService(DbBase db)
        {
            _db = db;
        }

        //        *********** Setters ***********
        public async Task<int> CreateMuscle(MuscleCreationAndEditDTO muscle)// 0 == faulty DTO || 1 == muscle alr exists || 2 == success
        {
            if(muscle == null || string.IsNullOrEmpty(muscle.Name))
                return 0;

            if(await isMuscleExist(muscle.Name))
                return 1;

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
        public async Task<int> UpdateMuscle(Guid muscleID,MuscleCreationAndEditDTO muscle)//0 == faulty DTO || 1 == muscle not found || 2 == success
        {
            //Validating DTO
            if (muscle == null || muscleID == Guid.Empty)
                return 0;
            //Getting muscle from database
            var toBeUpdated = await (from M in _db.Muscles
                                     where M.MusclesID == muscleID
                                     select M).FirstOrDefaultAsync();

            //If muscle doesn't exist
            if (toBeUpdated == null)
                return 1;

             //Updating muscle
            if (!string.IsNullOrEmpty(muscle.Name))
                toBeUpdated.Name = muscle.Name;

            if (!string.IsNullOrEmpty(muscle.Description))
                toBeUpdated.Description = muscle.Description;

            _db.Muscles.Update(toBeUpdated);
            await _db.SaveChangesAsync();
            return 2;
                
        }
        public async Task<int> DeleteMuscle(Guid muscleID)//0 == faulty GUID || 1 == muscle not found || 2 == success
        {
            if(muscleID == Guid.Empty) 
                return 0;
            var isMuscleExists = (from M in _db.Muscles
                                  where M.MusclesID == muscleID
                                  select M).FirstOrDefault();

            if (isMuscleExists == null) 
                return 1;

            _db.Muscles.Remove(isMuscleExists);
            await _db.SaveChangesAsync();
            return 2;
        }
        //-----------------------------------------------------------------------


        //        *********** Extra Validation Function ***********

        public async Task<bool> isMuscleExist(string name)
        {
            var isMuscleExists = await _db.Muscles.AnyAsync(m => m.Name == name);
            return isMuscleExists;
        }

        //-----------------------------------------------------------------------

        //        *********** Getters ***********

        public async Task<PagedList<MuscleViewDTO>> GetAllMuscles(int page,int pageSize)
        {
            if (page == 0) page = 1;
            if (pageSize == 0) pageSize = 15;//Returning all the muscles in the database
            var musclesQuery = from m in _db.Muscles
                          select new MuscleViewDTO
                          {
                                MusclesID = m.MusclesID,
                                Name = m.Name,
                                Description = m.Description
                          };
            var muscles = await PagedList<MuscleViewDTO>.CreateAsync(musclesQuery, page, pageSize);
            return muscles;
        }
    }
}
