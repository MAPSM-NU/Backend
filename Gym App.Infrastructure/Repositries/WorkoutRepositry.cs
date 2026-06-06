using Gym_App.Domain;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.Interfaces.Repositries;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Threading;

namespace Gym_App.Infastructure.Repositries
{
    public class WorkoutRepositry : BaseRepositry<Workout>, IWorkoutRepositry
    {
        private readonly DbBase _db;
        private readonly DbSet<Workout> table;
        public WorkoutRepositry(DbBase db) : base(db)
        {
            _db = db;
            table = _db.Set<Workout>();
        }
        public async Task<Workout> GetWorkoutById(Guid workoutID, CancellationToken cancellationToken = default)
        {
            return await table
                        .Include(w => w.User)
                        .Include(w => w.Feedback)
                        .Include(w => w.ExerciseInstances)
                            .ThenInclude(ei => ei.Sets)
                        .Include(w => w.ExerciseInstances)
                            .ThenInclude(ei => ei.Exercise)
                        .AsSplitQuery()
                        .FirstOrDefaultAsync(w => w.Id == workoutID, cancellationToken);
        }
        public async Task<Workout> GetWorkoutByUserId(Guid userId, CancellationToken cancellationToken = default)
        {
            return await table
                        .Include(w => w.User)
                        .Include(w => w.Feedback)
                        .Include(w => w.ExerciseInstances)
                            .ThenInclude(ei => ei.Sets)
                        .Include(w => w.ExerciseInstances)
                            .ThenInclude(ei => ei.Exercise)
                        .AsSplitQuery()
                        .FirstOrDefaultAsync(w => w.User.Id == userId, cancellationToken);
        }
        public async Task<IEnumerable<Workout>> GetWorkoutsByUserId(Guid userID, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            return await table.Where(w => w.User.Id == userID)
                        .Include(w => w.User)
                        .Include(w => w.Feedback)
                        .Include(w => w.ExerciseInstances)
                            .ThenInclude(ei => ei.Sets)
                        .Include(w => w.ExerciseInstances)
                            .ThenInclude(ei => ei.Exercise)
                        .AsSplitQuery()
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync(cancellationToken);
        }

        public async Task<bool> isWorkoutExist(Guid workoutID, CancellationToken cancellationToken = default)
        {
            return await table.AnyAsync(w => w.Id == workoutID, cancellationToken);
        }

        public async Task<bool> isWorkoutNameExist(string name, CancellationToken cancellationToken = default)
        {
            return await table.AnyAsync(w => w.Name == name, cancellationToken);
        }
        public override IQueryable<Workout> FilterSortColumn(string columnName, string sortOrder, IQueryable<Workout> query)
        {
            Expression<Func<Workout, object>> keySelector = columnName.ToLower() switch
            {
                "name" or "n" => Exercise => Exercise.Name, // Sort by name
                "difficulty" or "dif" => Exercise => Exercise.Difficulty!, // sort by difficulty
                "description" or "desc" => Exercise => Exercise.Description!, // sort by description
                _ => Exercise => Exercise.Id // failsafe: sort by ID
            };

            //If no orderby was inputed, then we sort ascending
            if (!string.IsNullOrEmpty(sortOrder)) query = query.OrderBy(keySelector);

            //else if anything was inputted we sort descending
            else query = query.OrderByDescending(keySelector);
            return query;
        }
        public override IQueryable<Workout> Search(string searchTerm, IQueryable<Workout> query)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return query;
            searchTerm = searchTerm.ToLower();
            return query.Where(w => w.Name.ToLower().Contains(searchTerm) || w.Description!.ToLower().Contains(searchTerm));
        }
    }
}
