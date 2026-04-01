using Gym_App.Domain;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infastructure.DTOs.Feedback;
using Gym_App.Infastructure.Interfaces.Repositries;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthorizationService _authorizationService;
        public FeedbackService(IUnitOfWork unitOfWork, IAuthorizationService authorizationService)
        {
            _unitOfWork = unitOfWork;
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
            var user = await _unitOfWork.Users.GetById(userID);
            //if user not found return
            if(user == null)
                return new SettersResponse { status = 0, msg = "User not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, user.Id, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Getting workout from Databse
            var workout = await _unitOfWork.Workouts.GetWorkoutByUserId(userID);
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
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                Title = feedbackDTO.Title,
                Type = feedbackDTO.Type,
                FeedbackText = feedbackDTO.FeedbackText,
                CaloriesBurned = feedbackDTO.CaloriesBurned,
                DurationMinutes = feedbackDTO.DurationMinutes,
                User = user,
                Workout = workout
            };

            //Saving to Database
            await _unitOfWork.Feedbacks.Create(feedback);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Success" };
        }
        public async Task<SettersResponse> UpdateFeedback(ClaimsPrincipal User, Guid feedbackID, FeedbackUpdateDTO feedbackDTO)
        {
            //Checking DTO validity
            if (feedbackDTO == null)
                return new SettersResponse { status = 0, msg = "Invalid DTO" };

            //Getting Feedback from Database
            var feedback = await _unitOfWork.Feedbacks.GetFeedbackwithUser(feedbackID);

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
            await _unitOfWork.Feedbacks.Update(feedback);
            await _unitOfWork.SaveChangesAsync();
            return new SettersResponse { status = 2, msg = "Success" };
        }
        public async Task<SettersResponse> DeleteFeedback(ClaimsPrincipal User, Guid feedbackID)
        {
            //Checking feedbackID validity
            if (feedbackID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid feedback ID" };

            //Getting feedback
            var feedback = await _unitOfWork.Feedbacks.GetFeedbackwithUser(feedbackID);
            //if feedback not found return
            if (feedback == null)
                return new SettersResponse { status = 0, msg = "Feedback not found" };

            //Authorization
            var authResult = await _authorizationService.AuthorizeAsync(User, feedback.User.Id, "SameUserPolicy");
            if (!authResult.Succeeded)
                return new SettersResponse { status = 1, msg = "Unauthorized" };

            //Saving to Database
            await _unitOfWork.Feedbacks.Delete(feedback);
            await _unitOfWork.SaveChangesAsync();   
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
            var Id = (await _unitOfWork.Feedbacks.GetFeedbackwithUser(feedbackID)).Id;

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
            var f = await _unitOfWork.Feedbacks.GetById(feedbackID);
            var feedback = new FeedbackViewDTO
            {
                WorkoutID = f.WorkoutID,
                FeedbackID = f.Id,
                Date = f.CreatedAt,
                CaloriesBurned = f.CaloriesBurned ?? 0,
                DurationMinutes = f.CaloriesBurned ?? 0,
                Title = f.Title,
                Type = f.Type,
                FeedbackText = f.FeedbackText,
                UserID = f.User.Id
            };
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
            var workout = await _unitOfWork.Workouts.GetWorkoutById(workoutID);
            var user = workout.User;

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

            var feedback = new FeedbackMiniViewDTO
            {
                FeedbackID = workout.Feedback.Id,
                WorkoutID = workout.Id,
                FeedbackText = workout.Feedback.FeedbackText,
                CaloriesBurned = (int)workout.Feedback.CaloriesBurned,
                Date = workout.Feedback.CreatedAt,
                DurationMinutes = (int)workout.Feedback.DurationMinutes,
                Title = workout.Feedback.Title,
                Type = workout.Feedback.Type,
            };
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
            var user = await _unitOfWork.Users.GetById(Id);
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

            IQueryable<Feedback> feedbackQuery = await _unitOfWork.Feedbacks.GetFeedbacksOfUser(Id);

            if (feedbackQuery == null || feedbackQuery.Count() == 0)
                return new GettersResponse<FeedbackMiniViewDTO>
                {
                    status = 0,
                    msg = "User has no feedbacks"
                };

            //Filtering by start date and end date
            DateTime validStartDate,validEndDate;
            if (DateTime.TryParse(startDate, out validStartDate) && DateTime.TryParse(endDate, out validEndDate))//Takes Dates after the start Date
                feedbackQuery = _unitOfWork.Feedbacks.FilterDate(validStartDate, validEndDate,feedbackQuery);

            //filtering by search term in title
            if (!string.IsNullOrEmpty(searchTerm)) feedbackQuery = _unitOfWork.Feedbacks.Search(searchTerm, feedbackQuery);

            //Ordering by given column
            if (!string.IsNullOrEmpty(sortColumn))
                feedbackQuery = _unitOfWork.Feedbacks.FilterSortColumn(sortColumn,OrderBy,feedbackQuery);
            //Projecting the resultant feedbacks queries to FeedbackDTO
            var feedbackResponse = feedbackQuery
                                    .Select(f => new FeedbackMiniViewDTO
                                    {
                                        WorkoutID = f.WorkoutID,
                                        FeedbackID = f.Id,
                                        Date = f.CreatedAt,
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
            var feedbacks = _unitOfWork.Feedbacks.GetFeedbacks();

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
                FeedbackID = f.Id,
                Date = f.CreatedAt,
                Title = f.Title,
                Type = f.Type,
                FeedbackText = f.FeedbackText,
                CaloriesBurned = f.CaloriesBurned ?? 0,//For testing purposes only
                DurationMinutes = f.DurationMinutes ?? 0,
                UserID = f.User.Id,
                WorkoutID = f.Workout.Id
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
