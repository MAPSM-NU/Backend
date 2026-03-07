namespace Gym_App.Infastructure.Interfaces.Repositries
{
    public interface IBaseRepositry<T> where T : class
    {
        Task<IEnumerable<T>> GetAll(int pageNumber = 1, int pageSize = 10);
        Task<T> GetById(int id);
        Task Create(T entity);
        Task Update(T entity);
        Task Delete(int id);
    }
}
