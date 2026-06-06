using Gym_App.Domain;

namespace Gym_App.Infastructure.Interfaces.Repositries
{
    public interface IFeedbackRepositry : IBaseRepositry<Feedback>
    {
        public Task<Feedback> GetFeedbackwithWorkout(Guid feedbackId, CancellationToken cancellationToken = default);
        public Task<Feedback> GetFeedbackwithUser(Guid feedbackId, CancellationToken cancellationToken = default);
        public Task<IQueryable<Feedback>> GetFeedbacksOfUser(Guid userId, CancellationToken cancellationToken = default);
        public IQueryable<Feedback> GetFeedbacks();
    }
}
