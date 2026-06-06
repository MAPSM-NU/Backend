using Gym_App.Infastructure.DTOs.Feedback;
using Gym_App.Infastructure.Transfer_Classes;
using System.Security.Claims;
using System.Threading;

namespace Gym_App.Infastructure.Interfaces.Services
{
    public interface IFeedbackService
    {
        Task<SettersResponse> CreateFeedback(Guid userID, FeedbackCreationDTO feedbackDTO, CancellationToken cancellationToken = default);
        Task<SettersResponse> UpdateFeedback(Guid feedbackID, FeedbackUpdateDTO feedbackDTO, CancellationToken cancellationToken = default);
        Task<SettersResponse> DeleteFeedback(Guid feedbackId, CancellationToken cancellationToken = default);
        Task<Guid> GetFeedbackUserID(Guid feedbackId, CancellationToken cancellationToken = default);
        Task<GettersResponse<FeedbackViewDTO>> GetFeedbackByID(Guid feedbackId, CancellationToken cancellationToken = default);
        Task<GettersResponse<FeedbackMiniViewDTO>> GetFeedbackOfWorkout(Guid workoutID, CancellationToken cancellationToken = default);
        Task<GettersResponse<FeedbackMiniViewDTO>> GetUserFeedbacks(Guid UserID, string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize, CancellationToken cancellationToken = default);
        Task<GettersResponse<FeedbackViewDTO>> GetAllFeedbacks(int page, int pageSize = 5, CancellationToken cancellationToken = default);
    }
}
