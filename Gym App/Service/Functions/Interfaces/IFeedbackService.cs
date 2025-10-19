using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;

namespace Gym_App.Service.Functions.Interfaces
{
    public interface IFeedbackService
    {
        Task<int> CreateFeedback(FeedbackDTO feedbackDTO);
        Task<int> UpdateFeedback(FeedbackDTO feedbackDTO);
        Task<int> DeleteFeedback(Guid feedbackId);
        Task<FeedbackDTO> GetFeedbackByID(Guid feedbackId);
        Task<PagedList<FeedbackDTO>?> GetFeedbackByFilter(int page, string sortColumn, string OrderBy, string searchTerm, int pageSize = 5);
        Task<PagedList<FeedbackDTO>?> GetAllFeedbacks(int page, int pageSize=5);
    }
}
