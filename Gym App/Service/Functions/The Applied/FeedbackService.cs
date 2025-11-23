using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Service.Controllers;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Gym_App.Service.Functions.The_Applied
{
    public class FeedbackService : IFeedbackService
    {
        private readonly DbBase _db;
        private readonly IAuthorizationService _authorizationService;
        public FeedbackService(DbBase db,IAuthorizationService authorizationService)
        {
            _db = db;
            _authorizationService = authorizationService;
        }
        public async Task<int> CreateFeedback(ClaimsPrincipal User, FeedbackDTO feedbackDTO)//0 == faulty DTO || 1 == User not found || 2 == Unauthorized || 3 == wokrout not found || 4 == Succesful
        {
            //checking DTO validity
            if (feedbackDTO == null) 
                return 0;

            //Getting user from Database
            var user = await (from u in _db.Users
                              where u.UserID == feedbackDTO.UserID
                              select u).FirstOrDefaultAsync();
            //if user not found return
            if(user == null)
                return 1;

            //Authorization
            var authResult = await  _authorizationService.AuthorizeAsync(User, user.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return 2;

            //Getting workout from Databse
            var workout = await (from w in _db.Workouts
                                 where w.WorkoutID == feedbackDTO.WorkoutID
                                 select w).FirstOrDefaultAsync(); ;
            //if workout not found return
            if (workout == null)
                return 3;

            //Creating Feedback
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

            //Saving to Database
            await _db.Feedbacks.AddAsync(feedback);
            await _db.SaveChangesAsync();
            return 4;
        }
        public async Task<int> UpdateFeedback(ClaimsPrincipal User, FeedbackDTO feedbackDTO)//0 == Faulty DTO || 1 == Feedback not found || 2 == unauthorized || 3 == Succesful
        {
            //Checking DTO validity
            if (feedbackDTO == null) 
                return 0;

            //Getting Feedback from Database
            var feedback = await (from f in _db.Feedbacks.Include(f => f.User)
                                  where f.FeedbackID == feedbackDTO.FeedbackID
                                  select f).FirstOrDefaultAsync();

            //if feedback not found return
            if (feedback == null) 
                return 1;

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, feedback.User.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return 2;

            //Updating given features
            if (!string.IsNullOrWhiteSpace(feedback.Title)) feedback.Title = feedbackDTO.Title;
            if (!string.IsNullOrWhiteSpace(feedback.Type)) feedback.Type = feedbackDTO.Type;
            if (!string.IsNullOrWhiteSpace(feedback.FeedbackText)) feedback.FeedbackText = feedbackDTO.FeedbackText;
            if (feedbackDTO.CaloriesBurned > 0) feedback.CaloriesBurned = feedbackDTO.CaloriesBurned;
            if (feedbackDTO.DurationMinutes > 0) feedback.DurationMinutes = feedbackDTO.DurationMinutes;

            //Saving to Database
            _db.Feedbacks.Update(feedback);
            await _db.SaveChangesAsync();
            return 3;
        }
        public async Task<int> DeleteFeedback(ClaimsPrincipal User, Guid feedbackID)
        {
            //Checking feedbackID validity
            if (feedbackID == Guid.Empty)
                return 0;

            //Getting feedback
            var feedback = await (from f in _db.Feedbacks.Include(f => f.User)
                                  where f.FeedbackID == feedbackID
                                  select f).FirstOrDefaultAsync();
            //if feedback not found return
            if (feedback == null)
                return 1;

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, feedback.User.UserID, "SameUserPolicy");
            if(!authResult.Succeeded)
                return 2;

            //Saving to Database
            _db.Feedbacks.Remove(feedback);
            await _db.SaveChangesAsync();
            return 3;
        }
        public async Task<Guid> GetFeedbackUserID(Guid feedbackID)//Not User anymore
        {
            var userID = await (from f in _db.Feedbacks
                                  where f.FeedbackID == feedbackID
                                  select f.User.UserID).FirstOrDefaultAsync();
            return userID;
        }
        public async Task<FeedbackDTO?> GetFeedbackByID(Guid feedbackID)
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
