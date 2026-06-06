using Gym_App.Domain;

namespace Gym_App.Infastructure.Interfaces.Repositries
{
    public interface IBaseRepositry<T> where T : class
    {
        Task<IEnumerable<T>> GetAll(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        IQueryable<T> GetAll();
        Task<T> GetById(Guid id, CancellationToken cancellationToken = default);
        Task Create(T entity, CancellationToken cancellationToken = default);
        Task Update(T entity, CancellationToken cancellationToken = default);
        Task Delete(Guid id, CancellationToken cancellationToken = default);
        Task Delete(T entity, CancellationToken cancellationToken = default);
        abstract IQueryable<T> Search(string searchTerm, IQueryable<T> query);
        IQueryable<T> FilterDate(DateTime startDate, DateTime endDate, IQueryable<T> query);
        abstract IQueryable<T> FilterSortColumn(string columnName, string sortOrder, IQueryable<T> query);
    }
}
