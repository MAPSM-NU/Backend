using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Service.Controllers;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Linq.Expressions;

namespace Gym_App.Service.Functions.The_Applied
{
    public class FeedbackService : IFeedbackService
    {
        private readonly DbBase _db;
        public FeedbackService(DbBase db)
        {
            _db = db;
        }
        public async Task<int> CreateFeedback(FeedbackDTO feedbackDTO)//0 = Null DTO, 1 = User or Workout not found, 2 = Success
        {
            if (feedbackDTO == null) return 0;
            var user = await (from u in _db.Users
                              where u.UserID == feedbackDTO.UserID
                              select u).FirstOrDefaultAsync();
            var workout = await (from w in _db.Workouts
                                 where w.WorkoutID == feedbackDTO.WorkoutID
                                 select w).FirstOrDefaultAsync(); ;
            if (user == null || workout == null) return 1;
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
            await _db.Feedbacks.AddAsync(feedback);
            await _db.SaveChangesAsync();
            return 2;
        }
        public async Task<int> UpdateFeedback(FeedbackDTO feedbackDTO)//We really need to set a way to manage updates or modifications on the data
        {                                                             //0 = Null DTO, 1 = Feedback not found, 2 = Success
            if (feedbackDTO == null) return 0;
            var feedback = await _db.Feedbacks.FirstOrDefaultAsync(f => f.FeedbackID == feedbackDTO.FeedbackID);
            if (feedback == null) return 1;
            //This part is for if we want to change the user and workout asisgned to the feedback which is think wouldn't happen much if never so I am gonna leave it like this
            //var user = await _db.Users.FirstOrDefaultAsync(u=>u.UserID == feedbackDTO.UserID);
            //var workout = await _db.Workouts.FirstOrDefaultAsync(w=>w.WorkoutID==feedbackDTO.WorkoutID);
            //if (user == null || workout == null) return await Task.FromResult(1);
            //feedback.Date = feedbackDTO.Date; //why would you update the date? left for later
            //feedback.User = user;
            //feedback.Workout = workout;
            if (!string.IsNullOrWhiteSpace(feedback.Title)) feedback.Title = feedbackDTO.Title;
            if (!string.IsNullOrWhiteSpace(feedback.Type)) feedback.Type = feedbackDTO.Type;
            if (!string.IsNullOrWhiteSpace(feedback.FeedbackText)) feedback.FeedbackText = feedbackDTO.FeedbackText;
            if (feedbackDTO.CaloriesBurned > 0) feedback.CaloriesBurned = feedbackDTO.CaloriesBurned;
            if (feedbackDTO.DurationMinutes > 0) feedback.DurationMinutes = feedbackDTO.DurationMinutes;
            _db.Feedbacks.Update(feedback);
            await _db.SaveChangesAsync();
            return 2;
        }
        public async Task<int> DeleteFeedback(Guid feedbackId)
        {
            var feedback = await _db.Feedbacks.FirstOrDefaultAsync(f => f.FeedbackID == feedbackId);
            if (feedback == null) return 0;
            _db.Feedbacks.Remove(feedback);
            await _db.SaveChangesAsync();
            return 1;
        }
        public async Task<Guid> GetFeedbackUserID(Guid feedbackID)
        {
            var userID = await (from f in _db.Feedbacks
                                  where f.FeedbackID == feedbackID
                                  select f.User.UserID).FirstOrDefaultAsync();
            return userID;
        }
        public async Task<FeedbackDTO>? GetFeedbackByID(Guid feedbackID) //Might change it since it returns the DTO not the actual enitity(For Testing purposes)
        {
            var feedback = await (from f in _db.Feedbacks
                                  where f.FeedbackID == feedbackID
                                  select new FeedbackDTO
                                  {
                                      FeedbackID = f.FeedbackID,
                                      Date = f.Date,
                                      CaloriesBurned = f.CaloriesBurned ?? 0,
                                      DurationMinutes = f.CaloriesBurned ?? 0,
                                      Title = f.Title,
                                      UserID = f.User.UserID
                                  }).FirstOrDefaultAsync();
            if (feedback == null) return feedback;
            return feedback;
        }
        public async Task<PagedList<FeedbackDTO>?> GetFeedbackByFilter(string startDate, string endDate,int page, string sortColumn, string OrderBy, string searchTerm, int pageSize)
        {
            if (page == 0) page = 1;
            if(pageSize == 0)pageSize = 10;
            IQueryable<Feedback> feedbackQuery = _db.Feedbacks;
            DateTime validStartDate,validEndDate;
            if (DateTime.TryParse(startDate, out validStartDate))//Takes Dates after the start Date
            {
                feedbackQuery = feedbackQuery.Where(f=>f.Date > validStartDate);
            }
            if(DateTime.TryParse(endDate, out validEndDate))//Takes Dates before the end date
            {
                feedbackQuery = feedbackQuery.Where(f=>f.Date < validEndDate);
            }
            if (!string.IsNullOrEmpty(searchTerm)) feedbackQuery = feedbackQuery.Where(f => f.Title.Contains(searchTerm));
            if (!string.IsNullOrEmpty(sortColumn))
            {
                Expression<Func<Feedback, Object>> keySelector = sortColumn.ToLower() switch // throws error when sortColumn is null
                {
                    "title" => Feedback => Feedback.Title,
                    "calories" => Feedback => Feedback.CaloriesBurned,
                    "duration" => Feedback => Feedback.DurationMinutes,
                    _ => Feedback => Feedback.FeedbackID
                };
            if (!string.IsNullOrEmpty(OrderBy)) feedbackQuery = feedbackQuery.OrderBy(keySelector);//If any kind of value is in OrderBy then it is ascending
            else feedbackQuery = feedbackQuery.OrderByDescending(keySelector);
            }
            var feedbackResponse = feedbackQuery
                                    .Select(f => new FeedbackDTO
                                    {
                                        FeedbackID = f.FeedbackID,
                                        Date = f.Date,
                                        Title = f.Title,
                                        Type = f.Type,
                                        FeedbackText = f.FeedbackText,
                                        CaloriesBurned = f.CaloriesBurned ?? 0,//For testing purposes only
                                        DurationMinutes = f.DurationMinutes ?? 0,
                                    });
            var feedbacks = await PagedList<FeedbackDTO>.CreateAsync(feedbackResponse, page, pageSize);
            return feedbacks;
        }
        public async Task<PagedList<FeedbackDTO>?> GetAllFeedbacks(int page,int pageSize)
        {                                                                                   //Dont FORGET to include entities bru
            if (page == 0) page = 1;
            if(pageSize == 0) pageSize = 10;
            var feedbacks = (from f in _db.Feedbacks.Include(f => f.User).Include(f => f.Workout)
                            select f);
            if (feedbacks == null || feedbacks.IsNullOrEmpty()) return null;
            var feedbackQuery =feedbacks.Select(f => new FeedbackDTO
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
            });
            var Feedbacks = await PagedList<FeedbackDTO>.CreateAsync(feedbackQuery, page, pageSize);
            return Feedbacks;
        }
    }
}
