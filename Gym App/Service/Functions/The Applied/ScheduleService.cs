using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Gym_App.Service.Functions.The_Applied
{
    public class ScheduleService : IScheduleService
    {
        private readonly DbBase _db;
        public ScheduleService(DbBase db)
        {
            _db = db;
        }
        public async Task<int> AddSchedule(ScheduleDTO schedule)
        {
            var User = await (from u in _db.Users
                       where u.UserID == schedule.UserID
                       select u).FirstOrDefaultAsync();
            if (User == null) return 0;
            var newSchedule = new Schedule
            {
                ScheduleID = Guid.NewGuid(),
                Name = schedule.Name,
                Type = schedule.Type,
                CreatedAt = DateTime.UtcNow,
                //Description = schedule.Description, //Could add a description why not
                User = User
            };
            _db.Schedules.Add(newSchedule);
            await _db.SaveChangesAsync();
            return 1;
        }

        public async Task<int> UpdateSchedule(ScheduleDTO schedule)
        {
            var existingSchedule = await (from s in _db.Schedules
                                    where s.ScheduleID == schedule.ScheduleID
                                    select s).FirstOrDefaultAsync();
            if (existingSchedule == null) return 0;
            if(!string.IsNullOrEmpty(schedule.Name)) existingSchedule.Name = schedule.Name;
            if(!string.IsNullOrEmpty(schedule.Type)) existingSchedule.Type = schedule.Type;
            _db.Schedules.Update(existingSchedule);
            await _db.SaveChangesAsync();
            return 1;
        }
        public async Task<int> DeleteSchedule(Guid scheduleID)
        {
            var schedule = await (from s in _db.Schedules
                            where s.ScheduleID == scheduleID
                            select s).FirstOrDefaultAsync();
            if (schedule == null) return 0;
            _db.Schedules.Remove(schedule);
            await _db.SaveChangesAsync();
            return 1;
        }
        public async Task<int> AddWorkoutsToSchedule(ScheduleWorkoutDTO scheduleWorkout)//0 == faulty DTO || 1 == schedule not found || 2 == Workouts already in schedule || 3 == wrong IDs || 4 == success
        {
            if(scheduleWorkout == null) return 0;
            var schedule = await(from s in _db.Schedules.Include(s => s.Workouts)
                            where s.ScheduleID == scheduleWorkout.ScheduleID
                            select s).FirstOrDefaultAsync();
            if (schedule == null) return 1;
            var workoutIDsToAdd = new HashSet<Guid>(schedule.Workouts.Select(i=>i.WorkoutID));
            var workoutIDs = scheduleWorkout.WorkoutsID?.Where(id => !workoutIDsToAdd.Contains(id)).ToList();
            if (workoutIDs == null || workoutIDs.Count == 0) return 2;
            var workoutsToAdd = await(from w in _db.Workouts
                               where workoutIDs.Contains(w.WorkoutID)
                               select w).ToListAsync();
            if(workoutsToAdd.Count == 0) return 3;
            foreach (var workout in workoutsToAdd)
            {
                schedule.Workouts.Add(workout);
            }
            _db.Schedules.Update(schedule);
            await _db.SaveChangesAsync();
            return 4;
        }
        
        public async Task<int> SetWorkoutsOfSchedule(ScheduleWorkoutDTO scheduleWorkout)//0 == faulty DTO || 1 == schedule not found || 2 == Wrong IDs || 3 == success
        {
            if(scheduleWorkout == null) return 0;
            var schedule = await (from s in _db.Schedules.Include(s => s.Workouts)
                            where s.ScheduleID == scheduleWorkout.ScheduleID
                            select s).FirstOrDefaultAsync();
            if (schedule == null) return 1;
            schedule.Workouts?.Clear();
            var workoutsIDs = scheduleWorkout.WorkoutsID.ToList();
            var workoutsToAdd = await(from w in _db.Workouts
                               where workoutsIDs.Contains(w.WorkoutID)
                               select w).ToListAsync();
            if (workoutsToAdd.Count == 0) return 2;
            foreach(var workout in workoutsToAdd)
            {
                schedule.Workouts.Add(workout);
            }
            _db.Schedules.Update(schedule);
            await _db.SaveChangesAsync();
            return 3;
        }

        public async Task<int> DeleteWorkoutsFromSchedule(ScheduleWorkoutDTO scheduleWorkout)//0 == faulty DTO || 1 == schedule not found || 2 == no workouts in said schedule || 3 == wrong IDs || 4 = success
        {
            if(scheduleWorkout == null) return 0;
            var schedule = await (from s in _db.Schedules.Include(s => s.Workouts)
                            where s.ScheduleID == scheduleWorkout.ScheduleID
                            select s).FirstOrDefaultAsync();
            if (schedule == null) return 1;
            var existingWorkoutsIDs = new HashSet<Guid>(schedule.Workouts.Select(w => w.WorkoutID));
            var workoutIDsToRemove = scheduleWorkout.WorkoutsID?.Where(id => existingWorkoutsIDs.Contains(id)).ToList();
            if (workoutIDsToRemove == null || workoutIDsToRemove.Count == 0) return 2;
            var workoutsToRemove = await (from w in _db.Workouts
                                          where workoutIDsToRemove.Contains(w.WorkoutID)
                                          select w).ToListAsync();
            if (workoutsToRemove.Count == 0) return 3;
            foreach (var workout in workoutsToRemove)
            {
                schedule.Workouts.Remove(workout);
            }
            _db.Schedules.Update(schedule);
            await _db.SaveChangesAsync();
            return 4;
        }

        public async Task<ScheduleDTO?> GetScheduleById(Guid scheduleID)//AHHHHHHHHHHHHHHHHH
        {
            var schedule = await(from s in _db.Schedules
                            where s.ScheduleID == scheduleID
                            select new ScheduleDTO
                            {
                                ScheduleID = s.ScheduleID,
                                Name = s.Name,
                                Type = s.Type,
                                UserID = s.User.UserID,
                            }).FirstOrDefaultAsync();
            if (schedule == null) return null;
            return schedule;
        }
        public async Task<ScheduleWorkoutDTO?> GetScheduleWorkouts(Guid scheduleID)
        {
            var schedule = await (from s in _db.Schedules
                            where s.ScheduleID == scheduleID
                            select new ScheduleWorkoutDTO
                            {
                                ScheduleID = s.ScheduleID,
                                WorkoutsID = s.Workouts.Select(w => w.WorkoutID).ToList()
                            }).FirstOrDefaultAsync();
            if (schedule == null) return null;
            return schedule;
        }
        public async Task<PagedList<ScheduleDTO>?> GetSchedulesByOfUser(Guid UserID,string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize)
        {
            if (page == 0) page = 1;
            if(pageSize == 0) pageSize = 10;
            var schedulesQuery = (from s in _db.Schedules
                            where s.User.UserID == UserID
                            select s);
            if(schedulesQuery == null) return null;
            DateTime validStartDate, validEndDate;
            if (DateTime.TryParse(startDate, out validStartDate))
            {
                schedulesQuery = schedulesQuery.Where(s => s.CreatedAt > validStartDate);
            }
            if (DateTime.TryParse(endDate, out validEndDate))
            {
                schedulesQuery = schedulesQuery.Where(s=>s.CreatedAt < validEndDate);
            }
            if(!string.IsNullOrEmpty(searchTerm))schedulesQuery = schedulesQuery.Where(s=>s.Name.Contains(searchTerm));
            if (!string.IsNullOrEmpty(sortColumn))
            {
                Expression<Func<Schedule, Object>> keySelector = sortColumn.ToLower() switch
                {
                    "date" or "d" => Schedule => Schedule.CreatedAt,
                    "name" or "n" => Schedule => Schedule.Name,
                    "type" or "t" => Schedule => Schedule.Type,
                    _ => Schedule => Schedule.ScheduleID
                };
                if(!string.IsNullOrEmpty(OrderBy)) schedulesQuery = schedulesQuery.OrderBy(keySelector);
                else schedulesQuery = schedulesQuery.OrderByDescending(keySelector);
            }
            var schedulesResponse = schedulesQuery.Select(s => new ScheduleDTO
                                    {
                                        UserID = s.User.UserID,
                                        ScheduleID = s.ScheduleID,
                                        Name = s.Name,
                                        Type = s.Type,
                                        CreatedAt = s.CreatedAt,
                                    });
            var schedules = await PagedList<ScheduleDTO>.CreateAsync(schedulesResponse,page,pageSize);
            return schedules;
        }

        public async Task<PagedList<ScheduleDTO>?> GetAllSchedules(int page,int pageSize)
        {
            if (page == 0)page = 1;
            if (pageSize == 0)pageSize = 10;
            var schedulesQuery = (from s in _db.Schedules
                            select new ScheduleDTO
                            {
                                UserID = s.User.UserID,
                                ScheduleID = s.ScheduleID,
                                Name = s.Name,
                                Type = s.Type,
                                CreatedAt = s.CreatedAt
                            });
            if (schedulesQuery == null) return null;
            var schedules = await PagedList<ScheduleDTO>.CreateAsync(schedulesQuery, page, pageSize);
            return schedules;
        }

        public async Task<Guid> GetScheduleUserID(Guid scheduleID) 
        {
            var userID = await(from s in _db.Schedules
                          where s.ScheduleID == scheduleID
                          select s.User.UserID).FirstOrDefaultAsync();
            return userID;
        }

    }
}
