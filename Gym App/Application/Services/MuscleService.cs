using Gym_App.Application.Interfaces;
using Gym_App.Domain;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.DTOs.Muscle;
using Gym_App.Infastructure.Transfer_Classes;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;

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

        //0 == Error(Bad Request) || 1 == Unauthorized (Forbid) || 2 == Success (Ok)

        public async Task<SettersResponse> CreateMuscle(MuscleCreationAndEditDTO muscle)
        {
            if(muscle == null || string.IsNullOrEmpty(muscle.Name))
                return new SettersResponse { status = 0, msg = "Faulty DTO" };

            if(await isMuscleExist(muscle.Name))
                return new SettersResponse { status = 0, msg = "Muscle name already used" };

            var newMuscle = new Muscles
            {
                MusclesID = Guid.NewGuid(),
                Name = muscle.Name,
                Description = muscle.Description
            };

            await _db.Muscles.AddAsync(newMuscle);
            await _db.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Success" };
        }
        public async Task<SettersResponse> UpdateMuscle(Guid muscleID,MuscleCreationAndEditDTO muscle)
        {
            //Validating DTO
            if (muscle == null || muscleID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Faulty DTO" };
            //Getting muscle from database
            var toBeUpdated = await (from M in _db.Muscles
                                     where M.MusclesID == muscleID
                                     select M).FirstOrDefaultAsync();

            //If muscle doesn't exist
            if (toBeUpdated == null)
                return new SettersResponse { status = 0, msg = "Muscle not found" };

             //Updating muscle
            if (!string.IsNullOrEmpty(muscle.Name))
                toBeUpdated.Name = muscle.Name;

            if (!string.IsNullOrEmpty(muscle.Description))
                toBeUpdated.Description = muscle.Description;

            _db.Muscles.Update(toBeUpdated);
            await _db.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Success" };
                
        }
        public async Task<SettersResponse> DeleteMuscle(Guid muscleID)
        {
            if(muscleID == Guid.Empty) 
                return new SettersResponse { status = 0, msg = "Faulty GUID" };
            var isMuscleExists = (from M in _db.Muscles
                                  where M.MusclesID == muscleID
                                  select M).FirstOrDefault();

            if (isMuscleExists == null) 
                return new SettersResponse { status = 0,msg = "Muscle not found" };

            _db.Muscles.Remove(isMuscleExists);
            await _db.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Success" };
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
