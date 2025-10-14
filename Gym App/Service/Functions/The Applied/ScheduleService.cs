using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.EntityFrameworkCore;

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
        public async Task<List<ScheduleDTO>?> GetSchedulesByOfUser(Guid UserID)
        {
            var schedules = await(from s in _db.Schedules
                            where s.User.UserID == UserID
                            select new ScheduleDTO
                            {
                                ScheduleID = s.ScheduleID,
                                Name = s.Name,
                                Type = s.Type,
                                UserID = s.User.UserID,
                            }).ToListAsync();
            if(schedules == null) return null;
            return schedules;
        }

        public async Task<List<ScheduleDTO>?> GetAllSchedules()
        {
            var schedules = await(from s in _db.Schedules
                            select new ScheduleDTO
                            {
                                ScheduleID = s.ScheduleID,
                                Name = s.Name,
                                Type = s.Type,
                                UserID = s.User.UserID,
                            }).ToListAsync();
            if (schedules == null) return null;
            return schedules;
        }



    }
}
