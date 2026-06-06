using Gym_App.Domain;
namespace Gym_App.Infastructure.Interfaces.Repositries
{
    public interface IUserRepositry : IBaseRepositry<User>
    {
        public Task<bool> isUserExist(Guid userID, CancellationToken cancellationToken = default);
        public Task<bool> isUserEmailExist(string email, CancellationToken cancellationToken = default);
        public Task<bool> isUserNameExist(string name, CancellationToken cancellationToken = default);
        public Task<User> GetUserById(Guid userID, bool includeRole, CancellationToken cancellationToken = default);
        public Task<User> GetUserByName(string name, bool includeRole, CancellationToken cancellationToken = default);
        public Task<User> GetUserByEmail(string email, bool includeRole, CancellationToken cancellationToken = default);
        public Task<ICollection<User>> GetUsersByRole(Guid roleId, CancellationToken cancellationToken = default);
        public Task<IQueryable<User>> GetUsersByRoleAsQueryable(Guid roleId, CancellationToken cancellationToken = default);
    }
}
