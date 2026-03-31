using Gym_App.Domain;
namespace Gym_App.Infastructure.Interfaces.Repositries
{
    public interface IUserRepositry : IBaseRepositry<User>
    {
        public Task<bool> isUserExist(Guid userID);
        public Task<bool> isUserEmailExist(string email);
        public Task<bool> isUserNameExist(string name);
        public Task<User> GetUserById(Guid userID,bool includeRole);
        public Task<User> GetUserByName(string name,bool includeRole);
        public Task<User> GetUserByEmail(string email,bool includeRole);
        public Task<ICollection<User>> GetUsersByRole(Guid roleId);
        public Task<IQueryable<User>> GetUsersByRoleAsQueryable(Guid roleId);
    }
}
