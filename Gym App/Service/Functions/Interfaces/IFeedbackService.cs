using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using System.Security.Claims;

namespace Gym_App.Service.Functions.Interfaces
{
    public interface IFeedbackService
    {
        Task<int> CreateFeedback(ClaimsPrincipal User, FeedbackDTO feedbackDTO);
        Task<int> UpdateFeedback(ClaimsPrincipal User, FeedbackDTO feedbackDTO);
        Task<int> DeleteFeedback(ClaimsPrincipal User, Guid feedbackId);
        Task<Guid> GetFeedbackUserID(ClaimsPrincipal User, Guid feedbackId);
        Task<FeedbackDTO?> GetFeedbackByID(ClaimsPrincipal User, Guid feedbackId);
        Task<PagedList<FeedbackDTO>?> GetUserFeedbacksByFilter(ClaimsPrincipal User, Guid UserID, string startDate,string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize);
        Task<PagedList<FeedbackDTO>?> GetAllFeedbacks(int page, int pageSize=5);
    }
}
