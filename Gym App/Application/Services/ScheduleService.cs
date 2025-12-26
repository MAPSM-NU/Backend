using Gym_App.Application.Interfaces;
using Gym_App.Domain;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.DTOs.Schedule;
using Gym_App.Infastructure.Transfer_Classes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Gym_App.Application.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly DbBase _db;
        private readonly IAuthorizationService _authorizationService;
        public ScheduleService(DbBase db, IAuthorizationService authorizationService)
        {
            _db = db;
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
            var user = await (from u in _db.Users
                              where u.UserID == userID
                              select u).FirstOrDefaultAsync();
            if (user == null)
                return new SettersResponse { status = 0, msg = "User not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, user.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Creating schedule
            var newSchedule = new Schedule
            {
                ScheduleID = Guid.NewGuid(),
                Name = schedule.Name!,
                Type = schedule.Type!,
                CreatedAt = DateTime.UtcNow,
                //Description = schedule.Description, //Could add a description why not
                User = user
            };

            //Saving to Database
            _db.Schedules.Add(newSchedule);
            await _db.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Schedule created successfully" };
        }
        public async Task<SettersResponse> UpdateSchedule(ClaimsPrincipal User, Guid scheduleID, ScheduleCreationAndEditDTO schedule)
        {
            //Checking for DTO validity
            if (schedule == null)
                return new SettersResponse { status = 0, msg = "Invalid schedule data" };

            //Getting schedule from database
            var existingSchedule = await (from s in _db.Schedules.Include(s => s.User)
                                          where s.ScheduleID == scheduleID
                                          select s).FirstOrDefaultAsync();
            if (existingSchedule == null)
                return new SettersResponse { status = 0, msg = "Schedule not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, existingSchedule.User.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Updating fields
            if (!string.IsNullOrEmpty(schedule.Name)) existingSchedule.Name = schedule.Name;
            if (!string.IsNullOrEmpty(schedule.Type)) existingSchedule.Type = schedule.Type;

            //Saving to Database
            _db.Schedules.Update(existingSchedule);
            await _db.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Schedule updated successfully" };
        }
        public async Task<SettersResponse> DeleteSchedule(ClaimsPrincipal User, Guid scheduleID)
        {
            //Checking for scheduleID validity
            if (scheduleID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid schedule ID" };

            //Getting schedule from database
            var schedule = await (from s in _db.Schedules.Include(s => s.User)
                                  where s.ScheduleID == scheduleID
                                  select s).FirstOrDefaultAsync();
            if (schedule == null)
                return new SettersResponse { status = 0, msg = "Schedule not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, schedule.User.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Saving to Database
            _db.Schedules.Remove(schedule);
            await _db.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Schedule deleted successfully" };
        }
        public async Task<SettersResponse> AddWorkoutsToSchedule(ClaimsPrincipal User, Guid scheduleID, ScheduleWorkoutDTO scheduleWorkout)
        {
            //checking DTO
            if (scheduleWorkout == null)
                return new SettersResponse { status = 0, msg = "Invalid schedule workout data" };

            //Getting schedule from database
            var schedule = await (from s in _db.Schedules.Include(s => s.Workouts).Include(s => s.User)
                                  where s.ScheduleID == scheduleID
                                  select s).FirstOrDefaultAsync();
            if (schedule == null)
                return new SettersResponse { status = 0, msg = "Schedule not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, schedule.User.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Adding workouts
            var workoutIDsToAdd = new HashSet<Guid>(schedule.Workouts!.Select(i => i.WorkoutID));
            var workoutIDs = scheduleWorkout.WorkoutsID?.Where(id => !workoutIDsToAdd.Contains(id)).ToList();
            if (workoutIDs == null || workoutIDs.Count == 0)
                return new SettersResponse { status = 0, msg = "No new workouts to add" };
            var workoutsToAdd = await (from w in _db.Workouts
                                       where workoutIDs.Contains(w.WorkoutID)
                                       select w).ToListAsync();
            if (workoutsToAdd.Count == 0)
                return new SettersResponse { status = 0, msg = "Workouts not found" };
            foreach (var workout in workoutsToAdd)
            {
                schedule.Workouts!.Add(workout);
            }

            //Saving to Database
            _db.Schedules.Update(schedule);
            await _db.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Workouts added successfully" };
        }
        public async Task<SettersResponse> SetWorkoutsOfSchedule(ClaimsPrincipal User, Guid scheduleID, ScheduleWorkoutDTO scheduleWorkout)
        {
            //Checking for DTO validity
            if (scheduleWorkout == null)
                return new SettersResponse { status = 0, msg = "Invalid schedule workout data" };

            //Getting schedule from database    
            var schedule = await (from s in _db.Schedules.Include(s => s.Workouts).Include(s => s.User)
                                  where s.ScheduleID == scheduleID
                                  select s).FirstOrDefaultAsync();
            if (schedule == null)
                return new SettersResponse { status = 0, msg = "Schedule not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, schedule.User.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Setting workouts
            schedule.Workouts?.Clear();
            var workoutsIDs = scheduleWorkout.WorkoutsID.ToList();
            var workoutsToAdd = await (from w in _db.Workouts
                                       where workoutsIDs.Contains(w.WorkoutID)
                                       select w).ToListAsync();
            if (workoutsToAdd.Count == 0)
                return new SettersResponse { status = 0, msg = "No workouts found" };
            foreach (var workout in workoutsToAdd)
            {
                schedule.Workouts!.Add(workout);
            }

            //Saving to Database
            _db.Schedules.Update(schedule);
            await _db.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Workouts added successfully" };
        }
        public async Task<SettersResponse> DeleteWorkoutsFromSchedule(ClaimsPrincipal User, Guid scheduleID, ScheduleWorkoutDTO scheduleWorkout)
        {
            //Checking for DTO validity
            if (scheduleWorkout == null)
                return new SettersResponse { status = 0, msg = "Invalid schedule workout data" };

            //Getting schedule from database
            var schedule = await (from s in _db.Schedules.Include(s => s.Workouts).Include(s => s.User)
                                  where s.ScheduleID == scheduleID
                                  select s).FirstOrDefaultAsync();
            if (schedule == null)
                return new SettersResponse { status = 0, msg = "Schedule not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, schedule.User.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Checking if schedule has workouts
            var existingWorkoutsIDs = new HashSet<Guid>(schedule.Workouts!.Select(w => w.WorkoutID));
            var workoutIDsToRemove = scheduleWorkout.WorkoutsID?.Where(id => existingWorkoutsIDs.Contains(id)).ToList();
            if (workoutIDsToRemove == null || workoutIDsToRemove.Count == 0)
                return new SettersResponse { status = 0, msg = "No workouts to remove" };
            var workoutsToRemove = await (from w in _db.Workouts
                                          where workoutIDsToRemove.Contains(w.WorkoutID)
                                          select w).ToListAsync();
            if (workoutsToRemove.Count == 0)
                return new SettersResponse { status = 0, msg = "Workouts not found" };
            foreach (var workout in workoutsToRemove)
            {
                schedule.Workouts!.Remove(workout);
            }

            //Saving to Database
            _db.Schedules.Update(schedule);
            await _db.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Workouts removed successfully" };
        }

        //I am thinking of letting the Schedules be public so everyone can access eachother's workout schedules
        //In the future if that doesn't pan out, we can make these fucntions have authorization

        //-----------------------------------------------------------------------

        //        *********** Getters ***********

        public async Task<ScheduleViewDTO?> GetScheduleById(Guid scheduleID)//AHHHHHHHHHHHHHHHHH
        {
            //Getting schedule from database and projecting to DTO
            var schedule = await (from s in _db.Schedules
                                  where s.ScheduleID == scheduleID
                                  select new ScheduleViewDTO
                                  {
                                      ScheduleID = s.ScheduleID,
                                      Name = s.Name,
                                      Type = s.Type,
                                      UserID = s.User.UserID,
                                  }).FirstOrDefaultAsync();

            //Returning null if schedule not found
            if (schedule == null) return null;

            return schedule;
        }
        public async Task<ScheduleWorkoutDTO?> GetScheduleWorkouts(Guid scheduleID)
        {
            //Getting the schedule's workouts from database and projecting to DTO
            var schedule = await (from s in _db.Schedules
                                  where s.ScheduleID == scheduleID
                                  select new ScheduleWorkoutDTO
                                  {
                                      WorkoutsID = s.Workouts!.Select(w => w.WorkoutID).ToList()
                                  }).FirstOrDefaultAsync();
            //Returning null if schedule not found
            if (schedule == null) return null;

            return schedule;
        }
        public async Task<GettersResponse<ScheduleViewDTO>> GetSchedulesByOfUser(Guid UserID, string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize)
        {
            //Getting schedules from database
            var schedulesQuery = from s in _db.Schedules
                                 where s.User.UserID == UserID
                                 select s;
            //if no schedules found, return null
            if (schedulesQuery == null || schedulesQuery.Count() == 0)
                return new GettersResponse<ScheduleViewDTO>
                {
                    status = 0,
                    msg = "Schedule(s) not found"
                };

            //filtering by start date and end date
            DateTime validStartDate, validEndDate;
            if (DateTime.TryParse(startDate, out validStartDate))
            {
                schedulesQuery = schedulesQuery.Where(s => s.CreatedAt > validStartDate);
            }
            if (DateTime.TryParse(endDate, out validEndDate))
            {
                schedulesQuery = schedulesQuery.Where(s => s.CreatedAt < validEndDate);
            }

            //filtering by search term
            if (!string.IsNullOrEmpty(searchTerm)) schedulesQuery = schedulesQuery.Where(s => s.Name.Contains(searchTerm));

            //Order by given column
            if (!string.IsNullOrEmpty(sortColumn))
            {
                Expression<Func<Schedule, object>> keySelector = sortColumn.ToLower() switch
                {
                    "date" or "d" => Schedule => Schedule.CreatedAt, // order by date
                    "name" or "n" => Schedule => Schedule.Name, // order by name
                    "type" or "t" => Schedule => Schedule.Type, // order by type
                    _ => Schedule => Schedule.ScheduleID //failsafe: order by ScheduleID
                };
                //If no orderby was inputed, then we sort ascending
                if (!string.IsNullOrEmpty(OrderBy)) schedulesQuery = schedulesQuery.OrderBy(keySelector);

                //else if anything was inputted we sort descending
                else schedulesQuery = schedulesQuery.OrderByDescending(keySelector);
            }
            //Projecting the resultant message queries to messageDTO
            var schedulesResponse = schedulesQuery.Select(s => new ScheduleViewDTO
            {
                UserID = s.User.UserID,
                ScheduleID = s.ScheduleID,
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
            var schedulesQuery = from s in _db.Schedules
                                 select new ScheduleViewDTO
                                 {
                                     UserID = s.User.UserID,
                                     ScheduleID = s.ScheduleID,
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
            var userID = await(from s in _db.Schedules
                          where s.ScheduleID == scheduleID
                          select s.User.UserID).FirstOrDefaultAsync();
            return userID;
        }

    }
}
