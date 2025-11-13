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
        Task<Guid> GetFeedbackUserID(Guid feedbackId);
        Task<FeedbackDTO> GetFeedbackByID(Guid feedbackId);
        Task<PagedList<FeedbackDTO>?> GetFeedbackByFilter(string startDate,string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize);
        Task<PagedList<FeedbackDTO>?> GetAllFeedbacks(int page, int pageSize=5);
    }
}
