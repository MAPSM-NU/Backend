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
            var User = (from u in _db.Users
                       where u.UserID == schedule.UserID
                       select u).FirstOrDefault();
            if (User == null) return await Task.FromResult(0);
            var newSchedule = new Domain.Entities.Schedule
            {
                ScheduleID = Guid.NewGuid(),
                Name = schedule.Name,
                Type = schedule.Type,
                //Description = schedule.Description, //Could add a description why not
                User = User
            };
            _db.Schedules.Add(newSchedule);
            await _db.SaveChangesAsync();
            return await Task.FromResult(1);
        }

        public async Task<int> UpdateSchedule(ScheduleDTO schedule)
        {
            var existingSchedule = (from s in _db.Schedules
                                    where s.ScheduleID == schedule.ScheduleID
                                    select s).FirstOrDefault();
            if (existingSchedule == null) return await Task.FromResult(0);
            if(!string.IsNullOrEmpty(schedule.Name)) existingSchedule.Name = schedule.Name;
            if(!string.IsNullOrEmpty(schedule.Type)) existingSchedule.Type = schedule.Type;
            _db.Schedules.Update(existingSchedule);
            await _db.SaveChangesAsync();
            return await Task.FromResult(1);
        }
        public async Task<int> DeleteSchedule(Guid scheduleID)
        {
            var schedule = (from s in _db.Schedules
                            where s.ScheduleID == scheduleID
                            select s).FirstOrDefault();
            if (schedule == null) return await Task.FromResult(0);
            _db.Schedules.Remove(schedule);
            await _db.SaveChangesAsync();
            return await Task.FromResult(1);
        }
        public async Task<int> AddWorkoutsToSchedule(ScheduleWorkoutDTO scheduleWorkout)
        {
            var schedule = (from s in _db.Schedules.Include(s => s.Workouts)
                            where s.ScheduleID == scheduleWorkout.ScheduleID
                            select s).FirstOrDefault();
            if (schedule == null) return await Task.FromResult(0);
            foreach (var workoutID in scheduleWorkout.WorkoutsID)
            {
                var workout = (from w in _db.Workouts
                               where w.WorkoutID == workoutID
                               select w).FirstOrDefault();
                if (workout != null && !schedule.Workouts.Contains(workout))
                {
                    schedule.Workouts.Add(workout);
                }
            }
            _db.Schedules.Update(schedule);
            await _db.SaveChangesAsync();
            return await Task.FromResult(1);
        }
        
        public async Task<int> SetWorkoutsOfSchedule(ScheduleWorkoutDTO scheduleWorkout)
        {
            var schedule = (from s in _db.Schedules.Include(s => s.Workouts)
                            where s.ScheduleID == scheduleWorkout.ScheduleID
                            select s).FirstOrDefault();
            if (schedule == null) return await Task.FromResult(0);
            schedule.Workouts.Clear();
            foreach (var workoutID in scheduleWorkout.WorkoutsID)
            {
                var workout = (from w in _db.Workouts
                               where w.WorkoutID == workoutID
                               select w).FirstOrDefault();
                if (workout != null && !schedule.Workouts.Contains(workout))
                {
                    schedule.Workouts.Add(workout);
                }
            }
            _db.Schedules.Update(schedule);
            await _db.SaveChangesAsync();
            return await Task.FromResult(1);
        }

        public async Task<int> DeleteWorkoutsFromSchedule(ScheduleWorkoutDTO scheduleWorkout)
        {
            var schedule = (from s in _db.Schedules.Include(s => s.Workouts)
                            where s.ScheduleID == scheduleWorkout.ScheduleID
                            select s).FirstOrDefault();
            if (schedule == null) return await Task.FromResult(0);
            foreach (var workoutID in scheduleWorkout.WorkoutsID)
            {
                var workout = (from w in _db.Workouts
                               where w.WorkoutID == workoutID
                               select w).FirstOrDefault();
                if (workout != null && schedule.Workouts.Contains(workout))
                {
                    schedule.Workouts.Remove(workout);
                }
            }
            _db.Schedules.Update(schedule);
            await _db.SaveChangesAsync();
            return await Task.FromResult(1);
        }

        public Task<Schedule?> GetScheduleById(Guid scheduleID)
        {
            var schedule = (from s in _db.Schedules.Include(s => s.User).Include(s => s.Workouts)
                            where s.ScheduleID == scheduleID
                            select s).FirstOrDefault();
            if (schedule == null) return null;
            return Task.FromResult(schedule);
        }

        public Task<IQueryable<Schedule?>> GetSchedulesByOfUser(Guid UserID)
        {
            var schedules = from s in _db.Schedules
                            where s.User.UserID == UserID
                            select s;
            if(schedules == null) return null;
            return Task.FromResult(schedules);
        }

        public Task<IQueryable<Schedule?>> GetAllSchedules()
        {
            var schedules = from s in _db.Schedules
                            select s;
            if (schedules == null) return null;
            return Task.FromResult(schedules);
        }



    }
}
