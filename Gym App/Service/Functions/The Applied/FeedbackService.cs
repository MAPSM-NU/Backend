using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Gym_App.Service.Functions.The_Applied
{
    public class FeedbackService : IFeedbackService
    {
        private readonly DbBase _db;
        public FeedbackService(DbBase db)
        {
            _db = db;
        }
        public async Task<int> CreateFeedback(FeedbackDTO feedbackDTO)
        {
            if (feedbackDTO == null) return await Task.FromResult(0);
            var user = (from u in _db.Users
                       where u.UserID == feedbackDTO.UserID
                       select u).FirstOrDefault();
            var workout = (from w in _db.Workouts
                           where w.WorkoutID == feedbackDTO.WorkoutID
                           select w).FirstOrDefault(); ;
            if (user == null || workout == null) return await Task.FromResult(0);
            var feedback = new Feedback
            {
                FeedbackID = Guid.NewGuid(),
                Date = DateTime.UtcNow,
                Title = feedbackDTO.Title,
                Type = feedbackDTO.Type,
                FeedbackText = feedbackDTO.FeedbackText,
                CaloriesBurned = feedbackDTO.CaloriesBurned,
                DurationMinutes = feedbackDTO.DurationMinutes,
                User = user,
                Workout = workout
            };
            _db.Feedbacks.Add(feedback);
            await  _db.SaveChangesAsync();
            return await Task.FromResult(1);
        }
        public async Task<int> UpdateFeedback(FeedbackDTO feedbackDTO)//We really need to set a way to manage updates or modifications on the data
        {
            if (feedbackDTO == null) return await Task.FromResult(0);
            var feedback = _db.Feedbacks.FirstOrDefault(f => f.FeedbackID == feedbackDTO.FeedbackID);
            if (feedback == null) return await Task.FromResult(0);
            var user = _db.Users.Find(feedbackDTO.UserID);
            var workout = _db.Workouts.Find(feedbackDTO.WorkoutID);
            if (user == null || workout == null) return await Task.FromResult(0);
            //feedback.Date = feedbackDTO.Date; //why would you update the date? left for later
            if (!string.IsNullOrWhiteSpace(feedback.Title)) feedback.Title = feedbackDTO.Title;
            if (!string.IsNullOrWhiteSpace(feedback.Type)) feedback.Type = feedbackDTO.Type;
            if (!string.IsNullOrWhiteSpace(feedback.FeedbackText)) feedback.FeedbackText = feedbackDTO.FeedbackText;
            if (feedbackDTO.CaloriesBurned > 0) feedback.CaloriesBurned = feedbackDTO.CaloriesBurned;
            if (feedbackDTO.DurationMinutes > 0) feedback.DurationMinutes = feedbackDTO.DurationMinutes;
            feedback.User = user;
            feedback.Workout = workout;
            _db.Feedbacks.Update(feedback);
            await _db.SaveChangesAsync();
            return await Task.FromResult(1);
        }
        public async Task<int> DeleteFeedback(Guid feedbackId)
        {
            var feedback = _db.Feedbacks.FirstOrDefault(f => f.FeedbackID == feedbackId);
            if (feedback == null) return await Task.FromResult(0);
            _db.Feedbacks.Remove(feedback);
            await _db.SaveChangesAsync();
            return await Task.FromResult(1);
        }
        public async Task<Feedback>? GetFeedbackByID(Guid feedbackId) //Might change it since it returns the DTO not the actual enitity(For Testing purposes)
        {
            var feedback = _db.Feedbacks.FirstOrDefault(f => f.FeedbackID == feedbackId);
            if (feedback == null) return await Task.FromResult(feedback);
            return await Task.FromResult(feedback);
        }
        public async Task<IEnumerable<FeedbackDTO>> GetAllFeedbacks() //Might change it since it returns the DTO not the actual enitity(For Testing purposes)
        {                                                             //Dont FORGET to include entities bru
            var feedbacks = from f in _db.Feedbacks.Include(f => f.User).Include(f => f.Workout)
                            select f;
            if (feedbacks == null || feedbacks.IsNullOrEmpty()) return await Task.FromResult(new List<FeedbackDTO>());
            var feedbackDTOs = feedbacks.Select(f => new FeedbackDTO //DTO returning can work. I know this is the same as returning the entity but just checking how things work
            {
                FeedbackID = f.FeedbackID,
                Date = f.Date,
                Title = f.Title,
                Type = f.Type,
                FeedbackText = f.FeedbackText,
                CaloriesBurned = f.CaloriesBurned ?? 0,//For testing purposes only
                DurationMinutes = f.DurationMinutes ?? 0,
                UserID = f.User.UserID,
                WorkoutID = f.Workout.WorkoutID
            }).ToList();
            return await Task.FromResult(feedbackDTOs);
        }
    }
}
