using Gym_App.Domain;
namespace Gym_App.Infastructure.Interfaces.Repositries
{
    public interface IUserRepositry : IBaseRepositry<User>
    {
        public Task<bool> isUserExist(Guid userID);
        public bool isUserEmailExist(string email);
        public Task<bool> isUserNameExist(string name);
        public Task<User> GetUserById(Guid userID);
    }
}
