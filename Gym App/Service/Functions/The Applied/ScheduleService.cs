using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Gym_App.Service.Functions.The_Applied
{
    public class ScheduleService : IScheduleService
    {
        private readonly DbBase _db;
        private readonly IAuthorizationService _authorizationService;
        public ScheduleService(DbBase db,IAuthorizationService authorizationService)
        {
            _db = db;
            _authorizationService = authorizationService;
        }
        public async Task<int> AddSchedule(ClaimsPrincipal User, ScheduleDTO schedule)//0 == faulty DTO || 1 == user not found || 2 == unauthorized || 3 == success
        {
            //Checking for DTO validity
            if (schedule == null) 
                return 0;

            //Checking if user exists
            var user = await (from u in _db.Users
                       where u.UserID == schedule.UserID
                       select u).FirstOrDefaultAsync();
            if (user == null) 
                return 1;
            
            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User,user.UserID,"SameUserPolicy");
            if (!authResult.Succeeded)
                return 2;

            //Creating schedule
            var newSchedule = new Schedule
            {
                ScheduleID = Guid.NewGuid(),
                Name = schedule.Name,
                Type = schedule.Type,
                CreatedAt = DateTime.UtcNow,
                //Description = schedule.Description, //Could add a description why not
                User = user
            };

            //Saving to Database
            _db.Schedules.Add(newSchedule);
            await _db.SaveChangesAsync();
            return 3;
        }

        public async Task<int> UpdateSchedule(ClaimsPrincipal User, ScheduleDTO schedule)//0 == faulty DTO || 1 == schedule not found || 2 == unauthorized || 3 == success
        {
            //Checking for DTO validity
            if (schedule == null) 
                return 0;

            //Getting schedule from database
            var existingSchedule = await (from s in _db.Schedules.Include(s=>s.User)
                                    where s.ScheduleID == schedule.ScheduleID
                                    select s).FirstOrDefaultAsync();
            if (existingSchedule == null) 
                return 1;

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, existingSchedule.User.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return 2;

            //Updating fields
            if (!string.IsNullOrEmpty(schedule.Name)) existingSchedule.Name = schedule.Name;
            if(!string.IsNullOrEmpty(schedule.Type)) existingSchedule.Type = schedule.Type;

            //Saving to Database
            _db.Schedules.Update(existingSchedule);
            await _db.SaveChangesAsync();
            return 3;
        }
        public async Task<int> DeleteSchedule(ClaimsPrincipal User, Guid scheduleID)//0 == faulty scheduleID || 1 == schedule not found || 2 == unauthorized || 3 == success
        {
            //Checking for scheduleID validity
            if (scheduleID == Guid.Empty) 
                return 0;

            //Getting schedule from database
            var schedule = await (from s in _db.Schedules.Include(s=>s.User)
                            where s.ScheduleID == scheduleID
                            select s).FirstOrDefaultAsync();
            if (schedule == null) 
                return 1;

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, schedule.User.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return 2;

            //Saving to Database
            _db.Schedules.Remove(schedule);
            await _db.SaveChangesAsync();
            return 3;
        }
        public async Task<int> AddWorkoutsToSchedule(ClaimsPrincipal User, ScheduleWorkoutDTO scheduleWorkout)//0 == faulty DTO || 1 == schedule not found || 2 == unauthorized ||
                                                                                                              //3 == no new workouts to add || 4 == workouts not found || 5 == success
        {
            //checking DTO
            if (scheduleWorkout == null) 
                return 0;

            //Getting schedule from database
            var schedule = await(from s in _db.Schedules.Include(s => s.Workouts).Include(s => s.User)
                                 where s.ScheduleID == scheduleWorkout.ScheduleID
                            select s).FirstOrDefaultAsync();
            if (schedule == null) 
                return 1;

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, schedule.User.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return 2;

            //Adding workouts
            var workoutIDsToAdd = new HashSet<Guid>(schedule.Workouts.Select(i=>i.WorkoutID));
            var workoutIDs = scheduleWorkout.WorkoutsID?.Where(id => !workoutIDsToAdd.Contains(id)).ToList();
            if (workoutIDs == null || workoutIDs.Count == 0) 
                return 3;
            var workoutsToAdd = await(from w in _db.Workouts
                               where workoutIDs.Contains(w.WorkoutID)
                               select w).ToListAsync();
            if(workoutsToAdd.Count == 0) 
                return 4;
            foreach (var workout in workoutsToAdd)
            {
                schedule.Workouts.Add(workout);
            }
            
            //Saving to Database
            _db.Schedules.Update(schedule);
            await _db.SaveChangesAsync();
            return 5;
        }
        
        public async Task<int> SetWorkoutsOfSchedule(ClaimsPrincipal User, ScheduleWorkoutDTO scheduleWorkout)//0 == faulty DTO || 1 == schedule not found || 2 == unauthorized ||
                                                                                                              //3 == no workouts found || 4 == success
        {
            //Checking for DTO validity
            if (scheduleWorkout == null)
                return 0;

            //Getting schedule from database    
            var schedule = await (from s in _db.Schedules.Include(s => s.Workouts).Include(s=>s.User)
                                  where s.ScheduleID == scheduleWorkout.ScheduleID
                            select s).FirstOrDefaultAsync();
            if (schedule == null)
                return 1;

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, schedule.User.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return 2;

            //Setting workouts
            schedule.Workouts?.Clear();
            var workoutsIDs = scheduleWorkout.WorkoutsID.ToList();
            var workoutsToAdd = await(from w in _db.Workouts
                               where workoutsIDs.Contains(w.WorkoutID)
                               select w).ToListAsync();
            if (workoutsToAdd.Count == 0) 
                return 3;
            foreach(var workout in workoutsToAdd)
            {
                schedule.Workouts.Add(workout);
            }

            //Saving to Database
            _db.Schedules.Update(schedule);
            await _db.SaveChangesAsync();
            return 4;
        }

        public async Task<int> DeleteWorkoutsFromSchedule(ClaimsPrincipal User, ScheduleWorkoutDTO scheduleWorkout)//0 == faulty DTO || 1 == schedule not found || 2 == unauthorized ||
                                                                                                                   //3 == no workouts to remove || 4 == workouts not found || 5 == success
        {
            //Checking for DTO validity
            if (scheduleWorkout == null)
                return 0;

            //Getting schedule from database
            var schedule = await (from s in _db.Schedules.Include(s => s.Workouts).Include(s => s.User)
                            where s.ScheduleID == scheduleWorkout.ScheduleID
                            select s).FirstOrDefaultAsync();
            if (schedule == null) 
                return 1;

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, schedule.User.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return 2;

            //Checking if schedule has workouts
            var existingWorkoutsIDs = new HashSet<Guid>(schedule.Workouts.Select(w => w.WorkoutID));
            var workoutIDsToRemove = scheduleWorkout.WorkoutsID?.Where(id => existingWorkoutsIDs.Contains(id)).ToList();
            if (workoutIDsToRemove == null || workoutIDsToRemove.Count == 0) 
                return 3;
            var workoutsToRemove = await (from w in _db.Workouts
                                          where workoutIDsToRemove.Contains(w.WorkoutID)
                                          select w).ToListAsync();
            if (workoutsToRemove.Count == 0) 
                return 4;
            foreach (var workout in workoutsToRemove)
            {
                schedule.Workouts.Remove(workout);
            }

            //Saving to Database
            _db.Schedules.Update(schedule);
            await _db.SaveChangesAsync();
            return 5;
        }

        //I am thinking of letting the Schedules be public so everyone can access eachother's workout schedules
        //In the future if that doesn't pan out, we can make these fucntions have authorization

        public async Task<ScheduleDTO?> GetScheduleById(Guid scheduleID)//AHHHHHHHHHHHHHHHHH
        {
            //Getting schedule from database and projecting to DTO
            var schedule = await(from s in _db.Schedules
                            where s.ScheduleID == scheduleID
                            select new ScheduleDTO
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
                                ScheduleID = s.ScheduleID,
                                WorkoutsID = s.Workouts.Select(w => w.WorkoutID).ToList()
                            }).FirstOrDefaultAsync();
            //Returning null if schedule not found
            if (schedule == null) return null;

            return schedule;
        }
        public async Task<PagedList<ScheduleDTO>?> GetSchedulesByOfUser(Guid UserID,string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize)
        {
            //if page and pageSize are 0, set default values
            if (page == 0) page = 1;
            if(pageSize == 0) pageSize = 10;

            //Getting schedules from database
            var schedulesQuery = (from s in _db.Schedules
                                  where s.User.UserID == UserID
                                  select s);
            //if no schedules found, return null
            if (schedulesQuery == null) return null;

            //filtering by start date and end date
            DateTime validStartDate, validEndDate;
            if (DateTime.TryParse(startDate, out validStartDate))
            {
                schedulesQuery = schedulesQuery.Where(s => s.CreatedAt > validStartDate);
            }
            if (DateTime.TryParse(endDate, out validEndDate))
            {
                schedulesQuery = schedulesQuery.Where(s=>s.CreatedAt < validEndDate);
            }

            //filtering by search term
            if (!string.IsNullOrEmpty(searchTerm))schedulesQuery = schedulesQuery.Where(s=>s.Name.Contains(searchTerm));

            //Order by given column
            if (!string.IsNullOrEmpty(sortColumn))
            {
                Expression<Func<Schedule, Object>> keySelector = sortColumn.ToLower() switch
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
            var schedulesResponse = schedulesQuery.Select(s => new ScheduleDTO
                                    {
                                        UserID = s.User.UserID,
                                        ScheduleID = s.ScheduleID,
                                        Name = s.Name,
                                        Type = s.Type,
                                        CreatedAt = s.CreatedAt,
                                    });

            //Making the result as a paged list
            var schedules = await PagedList<ScheduleDTO>.CreateAsync(schedulesResponse,page,pageSize);
            return schedules;
        }

        public async Task<PagedList<ScheduleDTO>?> GetAllSchedules(int page,int pageSize)
        {
            //if page and pageSize are 0, set default values
            if (page == 0)page = 1;
            if (pageSize == 0)pageSize = 10;

            //Getting schedules from database and projecting to DTO
            var schedulesQuery = (from s in _db.Schedules
                            select new ScheduleDTO
                            {
                                UserID = s.User.UserID,
                                ScheduleID = s.ScheduleID,
                                Name = s.Name,
                                Type = s.Type,
                                CreatedAt = s.CreatedAt
                            });

            //If there are no schedules, return null
            if (schedulesQuery == null) return null;

            //Making the result as a paged list
            var schedules = await PagedList<ScheduleDTO>.CreateAsync(schedulesQuery, page, pageSize);
            return schedules;
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
