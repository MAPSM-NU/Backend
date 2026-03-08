using Gym_App.Domain;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.DTOs.Muscle;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infastructure.Transfer_Classes;
using Microsoft.EntityFrameworkCore;

namespace Gym_App.Application.Services
{
    public class MuscleService : IMuscleService
    {
        private readonly IMuscleRepositry _muscleRepositry;
        public MuscleService(IMuscleRepositry muscleRepositry)
        {
            _muscleRepositry = muscleRepositry;
        }

        //        *********** Setters ***********

        //0 == Error(Bad Request) || 1 == Unauthorized (Forbid) || 2 == Success (Ok)

        public async Task<SettersResponse> CreateMuscle(MuscleCreationAndEditDTO muscle)
        {
            if(muscle == null || string.IsNullOrEmpty(muscle.Name))
                return new SettersResponse { status = 0, msg = "Faulty DTO" };

            if(await _muscleRepositry.isMuscleExist(muscle.Name))
                return new SettersResponse { status = 0, msg = "Muscle name already used" };

            var newMuscle = new Muscles
            {
                Id = Guid.NewGuid(),
                Name = muscle.Name,
                Description = muscle.Description
            };

            await _muscleRepositry.Create(newMuscle);
            return new SettersResponse { status = 2, msg = "Success" };
        }
        public async Task<SettersResponse> UpdateMuscle(Guid muscleID,MuscleCreationAndEditDTO muscle)
        {
            //Validating DTO
            if (muscle == null || muscleID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Faulty DTO" };
            //Getting muscle from database
            var toBeUpdated = await (from M in _muscleRepositry.GetAll()
                                     where M.Id == muscleID
                                     select M).FirstOrDefaultAsync();

            //If muscle doesn't exist
            if (toBeUpdated == null)
                return new SettersResponse { status = 0, msg = "Muscle not found" };

             //Updating muscle
            if (!string.IsNullOrEmpty(muscle.Name))
                toBeUpdated.Name = muscle.Name;

            if (!string.IsNullOrEmpty(muscle.Description))
                toBeUpdated.Description = muscle.Description;

            await _muscleRepositry.Update(toBeUpdated);
            return new SettersResponse { status = 2, msg = "Success" };
                
        }
        public async Task<SettersResponse> DeleteMuscle(Guid muscleID)
        {
            if(muscleID == Guid.Empty) 
                return new SettersResponse { status = 0, msg = "Faulty GUID" };
            var isMuscleExists = (from M in _muscleRepositry.GetAll()
                                  where M.Id == muscleID
                                  select M).FirstOrDefault();

            if (isMuscleExists == null) 
                return new SettersResponse { status = 0,msg = "Muscle not found" };

            await _muscleRepositry.Delete(isMuscleExists);
            return new SettersResponse { status = 2, msg = "Success" };
        }
        //-----------------------------------------------------------------------

        //-----------------------------------------------------------------------

        //        *********** Getters ***********

        public async Task<GettersResponse<MuscleViewDTO>> GetAllMuscles(int page,int pageSize)
        {
            var musclesQuery = from m in _muscleRepositry.GetAll()
                               select new MuscleViewDTO
                          {
                                MusclesID = m.Id,
                                Name = m.Name,
                                Description = m.Description
                          };
            var muscles = await PagedList<MuscleViewDTO>.CreateAsync(musclesQuery, page, pageSize);
            return new GettersResponse<MuscleViewDTO>
            {
                status = 2,
                msg = "Successful",
                Data = muscles
            };
        }
    }
}
