using Gym_App.Domain;

namespace Gym_App.Infastructure.Interfaces.Repositries
{
    public interface IFeedbackRepositry : IBaseRepositry<Feedback>
    {
        public Task<Feedback> GetFeedbackwithWorkout(Guid feedbackId);
        public Task<Feedback> GetFeedbackwithUser(Guid feedbackId);
        public Task<IQueryable<Feedback>> GetFeedbacksOfUser(Guid userId);
        public IQueryable<Feedback> GetFeedbacks();
    }
}
