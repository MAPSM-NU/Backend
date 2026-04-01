using Gym_App.Infastructure.DTOs.Feedback;
using Gym_App.Infastructure.Transfer_Classes;
using System.Security.Claims;

namespace Gym_App.Infastructure.Interfaces.Services
{
    public interface IFeedbackService
    {
        Task<SettersResponse> CreateFeedback(ClaimsPrincipal User,Guid userID, FeedbackCreationDTO feedbackDTO);
        Task<SettersResponse> UpdateFeedback(ClaimsPrincipal User,Guid feedbackID, FeedbackUpdateDTO feedbackDTO);
        Task<SettersResponse> DeleteFeedback(ClaimsPrincipal User, Guid feedbackId);
        Task<Guid> GetFeedbackUserID(ClaimsPrincipal User, Guid feedbackId);
        Task<GettersResponse<FeedbackViewDTO>> GetFeedbackByID(ClaimsPrincipal User, Guid feedbackId);
        Task<GettersResponse<FeedbackMiniViewDTO>> GetFeedbackOfWorkout(ClaimsPrincipal User, Guid workoutID);
        Task<GettersResponse<FeedbackMiniViewDTO>> GetUserFeedbacks(ClaimsPrincipal User, Guid UserID, string startDate,string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize);
        Task<GettersResponse<FeedbackViewDTO>> GetAllFeedbacks(int page, int pageSize=5);
    }
}
