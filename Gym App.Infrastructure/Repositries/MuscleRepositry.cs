using Gym_App.Domain;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.Interfaces.Repositries;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Gym_App.Infastructure.Repositries
{
    public class MuscleRepositry : BaseRepositry<Muscles>, IMuscleRepositry
    {
        private readonly DbBase _db;
        private readonly DbSet<Muscles> table;
        public MuscleRepositry(DbBase db) : base(db)
        {
            _db = db;
            table = _db.Set<Muscles>();
        }

        public async Task<Muscles> getMuscleByName(string name)
        {
            name = name.ToLower();
            return await table.FirstOrDefaultAsync(m => m.Name.Contains(name));
        }

        public async Task<bool> isMuscleExist(string name)
        {
            return await table.AnyAsync(m=>m.Name.ToLower() == name.ToLower());
        }
        public override IQueryable<Muscles> FilterSortColumn(string columnName, string sortOrder, IQueryable<Muscles> query)
        {
            if (string.IsNullOrEmpty(columnName)) return query;
            Expression<Func<Muscles, object>> keySelector = columnName.ToLower() switch
            {
                "name" or "n" => m => m.Name,
                _ => m => m.Id,
            };
            var orderLower = (sortOrder ?? string.Empty).ToLowerInvariant();
            bool descending = orderLower == "desc" || orderLower == "descending";
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }
         public override IQueryable<Muscles> Search(string searchTerm, IQueryable<Muscles> query)
        {
            if (string.IsNullOrEmpty(searchTerm)) return query;
            searchTerm = searchTerm.ToLower();
            return query.Where(m =>
                m.Name.ToLower().Contains(searchTerm));
        }
    }
}
