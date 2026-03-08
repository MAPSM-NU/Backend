using Gym_App.Domain;

namespace Gym_App.Infastructure.Interfaces.Repositries
{
    public interface IBaseRepositry<T> where T : class
    {
        Task<IEnumerable<T>> GetAll(int pageNumber = 1, int pageSize = 10);
        Task<T> GetById(Guid id);
        Task Create(T entity);
        Task Update(T entity);
        Task Delete(Guid id);
        IQueryable<T> Search(string searchTerm, IQueryable<T> query);
        IQueryable<T> FilterDate(DateTime startDate, DateTime endDate, IQueryable<T> query);
        IQueryable<T> FilterSortColumn(string columnName, string sortOrder, IQueryable<T> query);
    }
}
