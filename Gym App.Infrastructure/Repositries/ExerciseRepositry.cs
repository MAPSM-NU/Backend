using Gym_App.Domain;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.Interfaces.Repositries;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Gym_App.Infastructure.Repositries
{
    public class ExerciseRepositry : BaseRepositry<Exercise>, IExerciseRepositry
    {
        private readonly DbBase _db;
        private readonly DbSet<Exercise> table;
        public ExerciseRepositry(DbBase db) : base(db)
        {
            _db = db;
            table = _db.Set<Exercise>();
        }

        public async Task<bool> isExerciseExist(Guid exerciseID)
        {
            return await table.AnyAsync(e => e.Id == exerciseID);
        }

        public async Task<bool> isExerciseNameExist(string name)
        {
            return await table.AnyAsync(e => e.Name == name);
        }
        public override IQueryable<Exercise> FilterSortColumn(string columnName, string sortOrder, IQueryable<Exercise> query)
        {
            if (string.IsNullOrEmpty(columnName)) return query;
            Expression<Func<Exercise, object>> keySelector = columnName.ToLower() switch
            {
                "name" or "n" => e => e.Name,
                "difficulty" or "dif" => e => e.Difficulty!,
                "description" or "desc" => e => e.Description!,
                "category" or "cat" => e => e.Category!,
                _ => e => e.Id,
            };
            var orderLower = (sortOrder ?? string.Empty).ToLowerInvariant();
            bool descending = orderLower == "desc" || orderLower == "descending";
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }
        public override IQueryable<Exercise> Search(string searchTerm, IQueryable<Exercise> query)
        {
            if (string.IsNullOrEmpty(searchTerm)) return query;
            searchTerm = searchTerm.ToLower();
            return query.Where(e =>
                e.Name.ToLower().Contains(searchTerm) ||
                e.Description!.ToLower().Contains(searchTerm) ||
                e.Category!.ToLower().Contains(searchTerm) ||
                e.Difficulty!.ToLower().Contains(searchTerm));
        }

        public async Task<IEnumerable<Exercise>> GetExercisesByIds(List<Guid> exerciseIDsToRemove)
        {
            return await table.Where(e => exerciseIDsToRemove.Contains(e.Id)).ToListAsync();
        }
    }
}
