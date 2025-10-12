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
        public async Task<int> AddWorkoutsToSchedule(ScheduleWorkoutDTO scheduleWorkout)//0 == faulty DTO || 1 == schedule not found || 2 == no new workouts added || 3 == success
        {
            if(scheduleWorkout == null) return 0;
            bool AddedAny = false;
            var schedule = await (from s in _db.Schedules.Include(s => s.Workouts)
                            where s.ScheduleID == scheduleWorkout.ScheduleID
                            select s).FirstOrDefaultAsync();
            if (schedule == null) return 1;
            foreach (var workoutID in scheduleWorkout.WorkoutsID)
            {
                var workout = await (from w in _db.Workouts
                               where w.WorkoutID == workoutID
                               select w).FirstOrDefaultAsync();
                if (workout != null && !schedule.Workouts.Contains(workout))
                {
                    schedule.Workouts.Add(workout);
                    AddedAny = true;
                }
            }
            var returnVal = 3;
            if (!AddedAny) returnVal = 2;
            _db.Schedules.Update(schedule);
            await _db.SaveChangesAsync();
            return returnVal;
        }
        
        public async Task<int> SetWorkoutsOfSchedule(ScheduleWorkoutDTO scheduleWorkout)//0 == faulty DTO || 1 == schedule not found || 2 == no changes made || 3 == only removals made || 4 == success
        {
            if(scheduleWorkout == null) return 0;
            bool AddedAny = false;
            bool RemovedAny = false;
            var schedule = await (from s in _db.Schedules.Include(s => s.Workouts)
                            where s.ScheduleID == scheduleWorkout.ScheduleID
                            select s).FirstOrDefaultAsync();
            if (schedule == null) return 1;
            if (schedule.Workouts != null) RemovedAny = true;
            schedule.Workouts.Clear();
            foreach (var workoutID in scheduleWorkout.WorkoutsID)
            {
                var workout = await(from w in _db.Workouts
                               where w.WorkoutID == workoutID
                               select w).FirstOrDefaultAsync();
                if (workout != null && !schedule.Workouts.Contains(workout))
                {
                    schedule.Workouts.Add(workout);
                    AddedAny = true;
                }
            }
            var returnVal = 4;
            if(!RemovedAny && !AddedAny) returnVal=2;
            else if( RemovedAny && !AddedAny) returnVal=3;
            _db.Schedules.Update(schedule);
            await _db.SaveChangesAsync();
            return returnVal;
        }

        public async Task<int> DeleteWorkoutsFromSchedule(ScheduleWorkoutDTO scheduleWorkout)//0 == faulty DTO || 1 == schedule not found || 2 == no workouts removed || 3 == success
        {
            if(scheduleWorkout == null) return 0;
            bool RemovedAny = false;
            var schedule = await (from s in _db.Schedules.Include(s => s.Workouts)
                            where s.ScheduleID == scheduleWorkout.ScheduleID
                            select s).FirstOrDefaultAsync();
            if (schedule == null) return 1;
            foreach (var workoutID in scheduleWorkout.WorkoutsID)
            {
                var workout = await(from w in _db.Workouts
                               where w.WorkoutID == workoutID
                               select w).FirstOrDefaultAsync();
                if (workout != null && schedule.Workouts.Contains(workout))
                {
                    schedule.Workouts.Remove(workout);
                    RemovedAny = true;
                }
            }
            var returnVal = 3;
            if(!RemovedAny) returnVal = 2;
            _db.Schedules.Update(schedule);
            await _db.SaveChangesAsync();
            return returnVal;
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
