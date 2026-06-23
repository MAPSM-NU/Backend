using Gym_App.Domain;

namespace Gym_App.Infastructure.Interfaces.Repositries
{
    public interface IWorkoutRepositry : IBaseRepositry<Workout>
    {
        public Task<bool> isWorkoutExist(Guid workoutID);
        public Task<bool> isWorkoutNameExist(string name);
        public IQueryable<Workout> FilterByStartDate(IQueryable<Workout> query, DateTime startDate);
        public IQueryable<Workout> FilterByEndDate(IQueryable<Workout> query, DateTime endDate);
        public Task<Workout> GetWorkoutByUserId(Guid userId);
        public Task<IEnumerable<Workout>> GetWorkoutsByUserId(Guid userID, int pageNumber = 1, int pageSize = 10);
        public Task<Workout> GetWorkoutById(Guid workoutID);

    }
}
