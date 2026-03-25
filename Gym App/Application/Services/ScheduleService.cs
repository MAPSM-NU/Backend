using Gym_App.Domain;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.DTOs.Schedule;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infastructure.Transfer_Classes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Gym_App.Application.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly IScheduleRepositry _scheduleRepositry;
        private readonly IUserRepositry _userRepositry;
        private readonly IWorkoutRepositry _workoutRepositry;
        private readonly IAuthorizationService _authorizationService;
        public ScheduleService(IScheduleRepositry scheduleRepositry,IUserRepositry userRepositry, IAuthorizationService authorizationService, IWorkoutRepositry workoutRepositry)
        {
            _scheduleRepositry = scheduleRepositry;
            _userRepositry = userRepositry;
            _workoutRepositry = workoutRepositry;
            _authorizationService = authorizationService;
        }

        //        *********** Setters ***********

        //0 == Error(Bad Request) || 1 == Unauthorized (Forbid) || 2 == Success (Ok)

        public async Task<SettersResponse> AddSchedule(ClaimsPrincipal User, Guid userID, ScheduleCreationAndEditDTO schedule)
        {
            //Checking for DTO validity
            if (schedule == null)
                return new SettersResponse { status = 0, msg = "Invalid schedule data" };

            //Checking if user exists
            var user = await _userRepositry.GetById(userID);
            if (user == null)
                return new SettersResponse { status = 0, msg = "User not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, user.Id, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Creating schedule
            var newSchedule = new Schedule
            {
                Id = Guid.NewGuid(),
                Name = schedule.Name!,
                Type = schedule.Type!,
                CreatedAt = DateTime.UtcNow,
                //Description = schedule.Description, //Could add a description why not
                User = user
            };

            //Saving to Database
            await _scheduleRepositry.Create(newSchedule);
            return new SettersResponse { status = 2, msg = "Schedule created successfully" };
        }
        public async Task<SettersResponse> UpdateSchedule(ClaimsPrincipal User, Guid scheduleID, ScheduleCreationAndEditDTO schedule)
        {
            //Checking for DTO validity
            if (schedule == null)
                return new SettersResponse { status = 0, msg = "Invalid schedule data" };

            //Getting schedule from database
            var existingSchedule = await _scheduleRepositry.GetScheduleById(scheduleID);
            if (existingSchedule == null)
                return new SettersResponse { status = 0, msg = "Schedule not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, existingSchedule.User.Id, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Updating fields
            if (!string.IsNullOrEmpty(schedule.Name)) existingSchedule.Name = schedule.Name;
            if (!string.IsNullOrEmpty(schedule.Type)) existingSchedule.Type = schedule.Type;

            //Saving to Database
            await _scheduleRepositry.Update(existingSchedule);
            return new SettersResponse { status = 2, msg = "Schedule updated successfully" };
        }
        public async Task<SettersResponse> DeleteSchedule(ClaimsPrincipal User, Guid scheduleID)
        {
            //Checking for scheduleID validity
            if (scheduleID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid schedule ID" };

            //Getting schedule from database
            var schedule = await _scheduleRepositry.GetScheduleById(scheduleID);
            if (schedule == null)
                return new SettersResponse { status = 0, msg = "Schedule not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, schedule.User.Id, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Saving to Database
            await _scheduleRepositry.Delete(schedule);
            return new SettersResponse { status = 2, msg = "Schedule deleted successfully" };
        }
        public async Task<SettersResponse> AddWorkoutsToSchedule(ClaimsPrincipal User, Guid scheduleID, ScheduleWorkoutDTO scheduleWorkout)
        {
            //checking DTO
            if (scheduleWorkout == null)
                return new SettersResponse { status = 0, msg = "Invalid schedule workout data" };

            //Getting schedule from database
            var schedule = await _scheduleRepositry.GetScheduleById(scheduleID);
            if (schedule == null)
                return new SettersResponse { status = 0, msg = "Schedule not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, schedule.User.Id, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Adding workouts
            var workoutIDsToAdd = new HashSet<Guid>(schedule.Workouts!.Select(i => i.Id));
            var workoutIDs = scheduleWorkout.WorkoutsID?.Where(id => !workoutIDsToAdd.Contains(id)).ToList();
            if (workoutIDs == null || workoutIDs.Count == 0)
                return new SettersResponse { status = 0, msg = "No new workouts to add" };
            var workoutsToAdd = await (from w in _workoutRepositry.GetAll()
                                       where workoutIDs.Contains(w.Id)
                                       select w).ToListAsync();
            if (workoutsToAdd.Count == 0)
                return new SettersResponse { status = 0, msg = "Workouts not found" };
            foreach (var workout in workoutsToAdd)
            {
                schedule.Workouts!.Add(workout);
            }

            //Saving to Database
            await _scheduleRepositry.Update(schedule);
            return new SettersResponse { status = 2, msg = "Workouts added successfully" };
        }
        public async Task<SettersResponse> SetWorkoutsOfSchedule(ClaimsPrincipal User, Guid scheduleID, ScheduleWorkoutDTO scheduleWorkout)
        {
            //Checking for DTO validity
            if (scheduleWorkout == null)
                return new SettersResponse { status = 0, msg = "Invalid schedule workout data" };

            //Getting schedule from database    
            var schedule = await _scheduleRepositry.GetScheduleById(scheduleID);
            if (schedule == null)
                return new SettersResponse { status = 0, msg = "Schedule not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, schedule.User.Id, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Setting workouts
            schedule.Workouts?.Clear();
            var workoutsIDs = scheduleWorkout.WorkoutsID.ToList();
            var workoutsToAdd = await (from w in _workoutRepositry.GetAll()
                                       where workoutsIDs.Contains(w.Id)
                                       select w).ToListAsync();
            if (workoutsToAdd.Count == 0)
                return new SettersResponse { status = 0, msg = "No workouts found" };
            foreach (var workout in workoutsToAdd)
            {
                schedule.Workouts!.Add(workout);
            }

            //Saving to Database
            await _scheduleRepositry.Update(schedule);
            return new SettersResponse { status = 2, msg = "Workouts added successfully" };
        }
        public async Task<SettersResponse> DeleteWorkoutsFromSchedule(ClaimsPrincipal User, Guid scheduleID, ScheduleWorkoutDTO scheduleWorkout)
        {
            //Checking for DTO validity
            if (scheduleWorkout == null)
                return new SettersResponse { status = 0, msg = "Invalid schedule workout data" };

            //Getting schedule from database
            var schedule = await _scheduleRepositry.GetScheduleById(scheduleID);
            if (schedule == null)
                return new SettersResponse { status = 0, msg = "Schedule not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, schedule.User.Id, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Checking if schedule has workouts
            var existingWorkoutsIDs = new HashSet<Guid>(schedule.Workouts!.Select(w => w.Id));
            var workoutIDsToRemove = scheduleWorkout.WorkoutsID?.Where(id => existingWorkoutsIDs.Contains(id)).ToList();
            if (workoutIDsToRemove == null || workoutIDsToRemove.Count == 0)
                return new SettersResponse { status = 0, msg = "No workouts to remove" };
            var workoutsToRemove = await (from w in _workoutRepositry.GetAll()
                                          where workoutIDsToRemove.Contains(w.Id)
                                          select w).ToListAsync();
            if (workoutsToRemove.Count == 0)
                return new SettersResponse { status = 0, msg = "Workouts not found" };
            foreach (var workout in workoutsToRemove)
            {
                schedule.Workouts!.Remove(workout);
            }

            //Saving to Database
            await _scheduleRepositry.Update(schedule);
            return new SettersResponse { status = 2, msg = "Workouts removed successfully" };
        }

        //I am thinking of letting the Schedules be public so everyone can access eachother's workout schedules
        //In the future if that doesn't pan out, we can make these fucntions have authorization

        //-----------------------------------------------------------------------

        //        *********** Getters ***********

        public async Task<GettersResponse<ScheduleViewDTO>> GetScheduleById(Guid scheduleID)//AHHHHHHHHHHHHHHHHH
        {
            //Getting schedule from database and projecting to DTO
            var schedule = await _scheduleRepositry.GetScheduleById(scheduleID);

            var projectedSched = new ScheduleViewDTO
            {
                Name = schedule.Name,
                UserID = schedule.User.Id,
                ScheduleID = scheduleID,
                CreatedAt = schedule.CreatedAt,
                Type = schedule.Type,
            };
            //Returning null if schedule not found
            if (schedule == null)
                return new GettersResponse<ScheduleViewDTO>
                {
                    status = 0,
                    msg = "Schedule not found"
                };

            return new GettersResponse<ScheduleViewDTO>
            {
                status = 2,
                msg = "Successful",
                Value = projectedSched
            };
        }
        public async Task<GettersResponse<ScheduleWorkoutDTO>> GetScheduleWorkouts(Guid scheduleID)
        {
            //Getting the schedule's workouts from database and projecting to DTO
            var schedule = await _scheduleRepositry.GetScheduleById(scheduleID);

            var workouts = schedule.Workouts!.Select(w=>w.Id).ToList();

            var projectedSched = new ScheduleWorkoutDTO
            {
                WorkoutsID = workouts,
            };
            //Returning null if schedule not found
            if (schedule == null)
                return new GettersResponse<ScheduleWorkoutDTO>
                {
                    status = 0,
                    msg = "Schedule not found"
                };

            return new GettersResponse<ScheduleWorkoutDTO>
            {
                status = 2,
                msg = "Successful",
                Value = projectedSched
            };
        }
        public async Task<GettersResponse<ScheduleViewDTO>> GetSchedulesByOfUser(Guid UserID, string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize)
        {
            //Getting schedules from database
            var schedulesQuery = _scheduleRepositry.GetUserSchedulesQueryable(UserID);
            //if no schedules found, return null
            if (schedulesQuery == null || schedulesQuery.Count() == 0)
                return new GettersResponse<ScheduleViewDTO>
                {
                    status = 0,
                    msg = "Schedule(s) not found"
                };

            //filtering by start date and end date
            DateTime validStartDate, validEndDate;
            if (DateTime.TryParse(startDate, out validStartDate) && DateTime.TryParse(endDate, out validEndDate))
                schedulesQuery = _scheduleRepositry.FilterDate(validStartDate, validEndDate, schedulesQuery);

            //filtering by search term
            if (!string.IsNullOrEmpty(searchTerm)) schedulesQuery = _scheduleRepositry.Search(searchTerm, schedulesQuery);

            //Order by given column
            if (!string.IsNullOrEmpty(sortColumn))
                schedulesQuery = _scheduleRepositry.FilterSortColumn(sortColumn, OrderBy, schedulesQuery);
            //Projecting the resultant message queries to messageDTO
            var schedulesResponse = schedulesQuery.Select(s => new ScheduleViewDTO
            {
                UserID = s.User.Id,
                ScheduleID = s.Id,
                Name = s.Name,
                Type = s.Type,
                CreatedAt = s.CreatedAt,
            });

            //Making the result as a paged list
            var schedules = await PagedList<ScheduleViewDTO>.CreateAsync(schedulesResponse, page, pageSize);
            return new GettersResponse<ScheduleViewDTO>
            {
                status = 2,
                msg = "Successful",
                Data = schedules
            };
        }
        public async Task<GettersResponse<ScheduleViewDTO>> GetAllSchedules(int page, int pageSize)
        {
            //Getting schedules from database and projecting to DTO
            var schedulesQuery = from s in _scheduleRepositry.GetAll()
                                 select new ScheduleViewDTO
                                 {
                                     UserID = s.User.Id,
                                     ScheduleID = s.Id,
                                     Name = s.Name,
                                     Type = s.Type,
                                     CreatedAt = s.CreatedAt
                                 };

            //If there are no schedules, return null
            if (schedulesQuery == null || schedulesQuery.Count() == 0)
                return new GettersResponse<ScheduleViewDTO>
                {
                    status = 0,
                    msg = "no Schedules in Database"
                };

            //Making the result as a paged list
            var schedules = await PagedList<ScheduleViewDTO>.CreateAsync(schedulesQuery, page, pageSize);
            return new GettersResponse<ScheduleViewDTO>
            {
                status = 2,
                msg = "Successful",
                Data = schedules
            };
        }
        public async Task<Guid> GetScheduleUserID(Guid scheduleID) 
        {
            //Getting userID from database
            var userID = await(from s in _scheduleRepositry.GetAll()
                          where s.Id == scheduleID
                          select s.User.Id).FirstOrDefaultAsync();
            return userID;
        }

    }
}
