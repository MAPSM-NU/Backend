using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gym_App.Domain;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.Interfaces.Repositries;
using Microsoft.EntityFrameworkCore;

namespace Gym_App.Infastructure.Repositries
{
    public class BaseRepositry<T> : IBaseRepositry<T> where T : BaseEntity
    {
        private readonly DbBase _db;
        private readonly DbSet<T> table;

        public BaseRepositry(DbBase db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            table = _db.Set<T>();
        }

        public async Task Create(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            table.Add(entity);
            await _db.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            var entity = await table.FindAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Entity with id {id} not found.");
            table.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public IQueryable<T> FilterDate(DateTime startDate, DateTime endDate, IQueryable<T> query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));
            return table.Where(e => e.CreatedAt >= startDate && e.CreatedAt <= endDate);
        }

        // sorting should be implemented in the derived class as it depends on the specific properties of T
        public virtual IQueryable<T> FilterSortColumn(string columnName, string sortOrder, IQueryable<T> query)
        {
            // Default implementation: return the query as-is
            return query;
        }

        public async Task<IEnumerable<T>> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            return await table.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<T> GetById(Guid id)
        {
            return await table.FindAsync(id);
        }

        // search should be implemented in the derived class as it depends on the specific properties of T
        public virtual IQueryable<T> Search(string searchTerm, IQueryable<T> query)
        {
            // Default implementation: return the query as-is
            return query;
        }

        public async Task Update(T entity)
        {
            table.Update(entity);
            await _db.SaveChangesAsync();
        }
    }
}
