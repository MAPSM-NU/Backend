using Gym_App.Domain;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.DTOs.Muscle;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infastructure.Transfer_Classes;
using Microsoft.EntityFrameworkCore;

namespace Gym_App.Application.Services
{
    public class MuscleService : IMuscleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MuscleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<SettersResponse> CreateMuscle(MuscleCreationAndEditDTO muscle)
        {
            if (muscle == null || string.IsNullOrEmpty(muscle.Name))
                return new SettersResponse { status = 0, msg = "Faulty DTO" };

            if (await _unitOfWork.Muscles.isMuscleExist(muscle.Name))
                return new SettersResponse { status = 0, msg = "Muscle name already used" };

            var newMuscle = new Muscles
            {
                Id = Guid.NewGuid(),
                Name = muscle.Name,
                Description = muscle.Description
            };

            await _unitOfWork.Muscles.Create(newMuscle);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Success" };
        }

        public async Task<SettersResponse> UpdateMuscle(Guid muscleID, MuscleCreationAndEditDTO muscle)
        {
            if (muscle == null || muscleID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Faulty DTO" };

            var toBeUpdated = await (from M in _unitOfWork.Muscles.GetAll()
                                     where M.Id == muscleID
                                     select M).FirstOrDefaultAsync();

            if (toBeUpdated == null)
                return new SettersResponse { status = 0, msg = "Muscle not found" };

            if (!string.IsNullOrEmpty(muscle.Name))
                toBeUpdated.Name = muscle.Name;

            if (!string.IsNullOrEmpty(muscle.Description))
                toBeUpdated.Description = muscle.Description;

            await _unitOfWork.Muscles.Update(toBeUpdated);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Success" };
        }

        public async Task<SettersResponse> DeleteMuscle(Guid muscleID)
        {
            if (muscleID == Guid.Empty) 
                return new SettersResponse { status = 0, msg = "Faulty GUID" };

            var isMuscleExists = (from M in _unitOfWork.Muscles.GetAll()
                                  where M.Id == muscleID
                                  select M).FirstOrDefault();

            if (isMuscleExists == null) 
                return new SettersResponse { status = 0, msg = "Muscle not found" };

            await _unitOfWork.Muscles.Delete(isMuscleExists);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Success" };
        }

        public async Task<GettersResponse<MuscleViewDTO>> GetAllMuscles(int page, int pageSize)
        {
            var musclesQuery = from m in _unitOfWork.Muscles.GetAll()
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
