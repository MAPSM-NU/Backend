using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;

namespace Gym_App.Service.Functions.Interfaces
{
    public interface IFeedbackService
    {
        Task<int> CreateFeedback(FeedbackDTO feedbackDTO);
        Task<int> UpdateFeedback(FeedbackDTO feedbackDTO);
        Task<int> DeleteFeedback(Guid feedbackId);
        Task<FeedbackDTO> GetFeedbackByID(Guid feedbackId);
        Task<List<FeedbackDTO>> GetAllFeedbacks();
    }
}
