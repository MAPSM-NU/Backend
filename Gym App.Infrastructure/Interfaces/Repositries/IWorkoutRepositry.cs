using Gym_App.Domain;

namespace Gym_App.Infastructure.Interfaces.Repositries
{
    public interface IWorkoutRepositry : IBaseRepositry<Workout>
    {
        public Task<bool> isWorkoutExist(Guid workoutID, CancellationToken cancellationToken = default);
        public Task<bool> isWorkoutNameExist(string name, CancellationToken cancellationToken = default);
        public Task<Workout> GetWorkoutByUserId(Guid userId, CancellationToken cancellationToken = default);
        public Task<IEnumerable<Workout>> GetWorkoutsByUserId(Guid userID, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        public Task<Workout> GetWorkoutById(Guid workoutID, CancellationToken cancellationToken = default);

    }
}
