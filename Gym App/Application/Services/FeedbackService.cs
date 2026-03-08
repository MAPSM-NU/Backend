using Gym_App.Domain;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.DTOs.Feedback;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infastructure.Transfer_Classes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Gym_App.Application.Services
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

        //        *********** Setters ***********

        //0 == Error(Bad Request) || 1 == Unauthorized (Forbid) || 2 == Success (Ok)

        public async Task<SettersResponse> CreateFeedback(ClaimsPrincipal User, Guid userID, FeedbackCreationDTO feedbackDTO)
        {
            //checking DTO validity
            if (feedbackDTO == null) 
                return new SettersResponse { status = 0, msg = "Invalid DTO" };

            //Getting user from Database
            var user = await (from u in _db.Users
                              where u.Id == userID
                              select u).FirstOrDefaultAsync();
            //if user not found return
            if(user == null)
                return new SettersResponse { status = 0, msg = "User not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, user.Id, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Getting workout from Databse
            var workout = await (from w in _db.Workouts.Include(w => w.Feedback)
                                 where w.WorkoutID == feedbackDTO.WorkoutID
                                 select w).FirstOrDefaultAsync(); ;
            //if workout not found return
            if (workout == null)
                return new SettersResponse { status = 0, msg = "Workout not found" };

            //Checking if the workout belongs to the user
            if (workout.User.Id != user.Id)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Feedback already exists for this workout
            if (workout.Feedback != null)
                return new SettersResponse { status = 0, msg = "Feedback already exists" };

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
            return new SettersResponse { status = 2, msg = "Success" };
        }
        public async Task<SettersResponse> UpdateFeedback(ClaimsPrincipal User, Guid feedbackID, FeedbackUpdateDTO feedbackDTO)
        {
            //Checking DTO validity
            if (feedbackDTO == null)
                return new SettersResponse { status = 0, msg = "Invalid DTO" };

            //Getting Feedback from Database
            var feedback = await (from f in _db.Feedbacks.Include(f => f.User)
                                  where f.FeedbackID == feedbackID
                                  select f).FirstOrDefaultAsync();

            //if feedback not found return
            if (feedback == null)
                return new SettersResponse { status = 0, msg = "Feedback not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, feedback.User.Id, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Updating given features
            if (!string.IsNullOrWhiteSpace(feedback.Title))
                feedback.Title = feedbackDTO.Title;
            
            if (!string.IsNullOrWhiteSpace(feedback.Type))
                feedback.Type = feedbackDTO.Type;
            
            if (!string.IsNullOrWhiteSpace(feedback.FeedbackText)) 
                feedback.FeedbackText = feedbackDTO.FeedbackText;
            
            if (feedbackDTO.CaloriesBurned > 0) 
                feedback.CaloriesBurned = feedbackDTO.CaloriesBurned;
            
            if (feedbackDTO.DurationMinutes > 0) 
                feedback.DurationMinutes = feedbackDTO.DurationMinutes;

            //Saving to Database
            _db.Feedbacks.Update(feedback);
            await _db.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Success" };
        }
        public async Task<SettersResponse> DeleteFeedback(ClaimsPrincipal User, Guid feedbackID)
        {
            //Checking feedbackID validity
            if (feedbackID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid feedback ID" };

            //Getting feedback
            var feedback = await (from f in _db.Feedbacks.Include(f => f.User)
                                  where f.FeedbackID == feedbackID
                                  select f).FirstOrDefaultAsync();
            //if feedback not found return
            if (feedback == null)
                return new SettersResponse { status = 0, msg = "Feedback not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, feedback.User.Id, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Saving to Database
            _db.Feedbacks.Remove(feedback);
            await _db.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Success" };
        }

        //-----------------------------------------------------------------------

        //        *********** Getters ***********

        public async Task<Guid> GetFeedbackId(ClaimsPrincipal User, Guid feedbackID)//Not User anymore
        {
            //Checking feedbackID validity
            if (feedbackID == Guid.Empty)
                return Guid.Empty;

            //Getting Id from Database
            var Id = await (from f in _db.Feedbacks
                                  where f.FeedbackID == feedbackID
                                  select f.User.Id).FirstOrDefaultAsync();

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, Id, "SameUserPolicy");
            if (!authResult.Succeeded)
                return Guid.Empty;

            return Id;
        }
        public async Task<GettersResponse<FeedbackViewDTO>> GetFeedbackByID(ClaimsPrincipal User, Guid feedbackID)
        {
            //Checking feedbackID validity
            if (feedbackID == Guid.Empty)
                return new GettersResponse<FeedbackViewDTO>
                {
                    status = 0,
                    msg = "Faulty ID"
                };

            //Getting feedback from Database
            var feedback = await (from f in _db.Feedbacks
                                  where f.FeedbackID == feedbackID
                                  select new FeedbackViewDTO
                                  {
                                      WorkoutID = f.WorkoutID,
                                      FeedbackID = f.FeedbackID,
                                      Date = f.Date,
                                      CaloriesBurned = f.CaloriesBurned ?? 0,
                                      DurationMinutes = f.CaloriesBurned ?? 0,
                                      Title = f.Title,
                                      Type = f.Type,
                                      FeedbackText = f.FeedbackText,
                                      UserID = f.User.Id
                                  }).FirstOrDefaultAsync();
            if (feedback == null)
                return new GettersResponse<FeedbackViewDTO>
                {
                    status = 0,
                    msg = "not found"
                };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, feedback.UserID, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new GettersResponse<FeedbackViewDTO>
                {
                    status = 1,
                    msg = "Unauthorized"
                };

            return new GettersResponse<FeedbackViewDTO>
            {
                status = 2,
                msg = "Successful",
                Value = feedback
            };
        }
        public async Task<GettersResponse<FeedbackMiniViewDTO>> GetFeedbackOfWorkout(ClaimsPrincipal User, Guid workoutID)
        {
            var user = await (from w in _db.Workouts.Include(w => w.User)
                              where w.WorkoutID == workoutID
                              select w.User).FirstOrDefaultAsync();

            if (user == null)
                return new GettersResponse<FeedbackMiniViewDTO>
                {
                    status = 0,
                    msg = "User not found"
                };

            var authResult = await _authorizationService.AuthorizeAsync(User, user.Id, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new GettersResponse<FeedbackMiniViewDTO>
                {
                    status = 1,
                    msg = "Unauthorized"
                };

            var feedback = await  (from f in _db.Feedbacks.Include(f => f.Workout).Include(f => f.User)
                                   where f.Workout.WorkoutID == workoutID
                                   select new FeedbackMiniViewDTO
                                   {
                                       WorkoutID = f.WorkoutID,
                                       FeedbackID = f.FeedbackID,
                                       Date = f.Date,
                                       Title = f.Title,
                                       Type = f.Type,
                                       FeedbackText = f.FeedbackText,
                                       CaloriesBurned = f.CaloriesBurned ?? 0,//For testing purposes only
                                       DurationMinutes = f.DurationMinutes ?? 0,
                                   }).FirstOrDefaultAsync();
            return new GettersResponse<FeedbackMiniViewDTO>
            {
                status = 2,
                msg = "Successful",
                Value = feedback
            };

        }
        public async Task<GettersResponse<FeedbackMiniViewDTO>> GetUserFeedbacks(ClaimsPrincipal User, Guid Id, string startDate, string endDate,int page, string sortColumn, string OrderBy, string searchTerm, int pageSize)
        {
            //Getting User from Database
            var user = await (from u in _db.Users
                              where u.Id == Id
                              select u).FirstOrDefaultAsync();
            //if user not found return
            if (user == null)
                return new GettersResponse<FeedbackMiniViewDTO>
                {
                    status = 0,
                    msg = "User not found"
                };
            
            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, user.Id, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new GettersResponse<FeedbackMiniViewDTO>
                {
                    status = 1,
                    msg = "Unauthorized"
                };

            IQueryable<Feedback> feedbackQuery = from f in _db.Feedbacks
                                                  where f.User.Id == Id
                                                  select f;

            if (feedbackQuery == null || feedbackQuery.Count() == 0)
                return new GettersResponse<FeedbackMiniViewDTO>
                {
                    status = 0,
                    msg = "User has no feedbacks"
                };

            //Filtering by start date and end date
            DateTime validStartDate,validEndDate;
            if (DateTime.TryParse(startDate, out validStartDate))//Takes Dates after the start Date
            {
                feedbackQuery = feedbackQuery.Where(f=>f.Date > validStartDate);
            }
            if(DateTime.TryParse(endDate, out validEndDate))//Takes Dates before the end date
            {
                feedbackQuery = feedbackQuery.Where(f=>f.Date < validEndDate);
            }

            //filtering by search term in title
            if (!string.IsNullOrEmpty(searchTerm)) feedbackQuery = feedbackQuery.Where(f => f.Title.Contains(searchTerm));

            //Ordering by given column
            if (!string.IsNullOrEmpty(sortColumn))
            {
                Expression<Func<Feedback, object>> keySelector = sortColumn.ToLower() switch // throws error when sortColumn is null
                {
                    "title" => Feedback => Feedback.Title, //order by Title
                    "calories" => Feedback => Feedback.CaloriesBurned!, // order by CaloriesBurned
                    "duration" => Feedback => Feedback.DurationMinutes!, // order by DurationMinutes
                    _ => Feedback => Feedback.FeedbackID//failsafe: order by FeedbackID
                };
                //If no orderby was inputed, then we sort ascending
                if (!string.IsNullOrEmpty(OrderBy)) feedbackQuery = feedbackQuery.OrderBy(keySelector);//If any kind of value is in OrderBy then it is ascending

                //else if anything was inputted we sort descending
                else feedbackQuery = feedbackQuery.OrderByDescending(keySelector);
            }

            //Projecting the resultant feedbacks queries to FeedbackDTO
            var feedbackResponse = feedbackQuery
                                    .Select(f => new FeedbackMiniViewDTO
                                    {
                                        WorkoutID = f.WorkoutID,
                                        FeedbackID = f.FeedbackID,
                                        Date = f.Date,
                                        Title = f.Title,
                                        Type = f.Type,
                                        FeedbackText = f.FeedbackText,
                                        CaloriesBurned = f.CaloriesBurned ?? 0,//For testing purposes only
                                        DurationMinutes = f.DurationMinutes ?? 0,
                                    });

            //Making the result as a paged list
            var feedbacks = await PagedList<FeedbackMiniViewDTO>.CreateAsync(feedbackResponse, page, pageSize);
            return new GettersResponse<FeedbackMiniViewDTO>
            {
                status = 2,
                msg = "Successful",
                Data = feedbacks
            };
        }
        public async Task<GettersResponse<FeedbackViewDTO>> GetAllFeedbacks(int page,int pageSize)
        {                                         
            //Getting all feedbacks from Database
            var feedbacks = from f in _db.Feedbacks.Include(f => f.User).Include(f => f.Workout)
                            select f;

            //if no feedbacks found return
            if (feedbacks == null)
                return new GettersResponse<FeedbackViewDTO>
                {
                    status = 0,
                    msg = "No Feedbacks in Database"
                };

            //Projecting the resultant feedbacks queries to FeedbackDTO
            var feedbackQuery = feedbacks.Select(f => new FeedbackViewDTO
            {
                FeedbackID = f.FeedbackID,
                Date = f.Date,
                Title = f.Title,
                Type = f.Type,
                FeedbackText = f.FeedbackText,
                CaloriesBurned = f.CaloriesBurned ?? 0,//For testing purposes only
                DurationMinutes = f.DurationMinutes ?? 0,
                UserID = f.User.Id,
                WorkoutID = f.Workout.WorkoutID
            });

            //Making the result as a paged list
            var Feedbacks = await PagedList<FeedbackViewDTO>.CreateAsync(feedbackQuery, page, pageSize);
            return new GettersResponse<FeedbackViewDTO>
            {
                status = 2,
                msg = "Successful",
                Data = Feedbacks
            };
        }

        public Task<Guid> GetFeedbackUserID(ClaimsPrincipal User, Guid feedbackId)
        {
            throw new NotImplementedException();
        }
    }
}
