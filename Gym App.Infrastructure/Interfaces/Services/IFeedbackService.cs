using Gym_App.Infastructure.DTOs.Feedback;
using Gym_App.Infastructure.Transfer_Classes;
using System.Security.Claims;

namespace Gym_App.Infastructure.Interfaces.Services
{
    public interface IFeedbackService
    {
        Task<SettersResponse> CreateFeedback(Guid userID, FeedbackCreationDTO feedbackDTO);
        Task<SettersResponse> UpdateFeedback(Guid feedbackID, FeedbackUpdateDTO feedbackDTO);
        Task<SettersResponse> DeleteFeedback(Guid feedbackId);
        Task<Guid> GetFeedbackUserID(Guid feedbackId);
        Task<GettersResponse<FeedbackViewDTO>> GetFeedbackByID(Guid feedbackId);
        Task<GettersResponse<FeedbackMiniViewDTO>> GetFeedbackOfWorkout(Guid workoutID);
        Task<GettersResponse<FeedbackMiniViewDTO>> GetUserFeedbacks(Guid UserID, string startDate,string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize);
        Task<GettersResponse<FeedbackViewDTO>> GetAllFeedbacks(int page, int pageSize=5);
    }
}
