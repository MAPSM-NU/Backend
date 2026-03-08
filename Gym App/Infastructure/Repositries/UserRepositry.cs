using Gym_App.Domain;
using Gym_App.Infastructure.Interfaces.Repositries;
using Microsoft.EntityFrameworkCore;

namespace Gym_App.Infastructure.Repositries
{
    public class UserRepositry : BaseRepositry<User>, IUserRepositry
    {
        public UserRepositry(DbContext db) : base(db)
        {
        }

        public Task<User> GetUserById(Guid userID)
        {
            throw new NotImplementedException();
        }

        public bool isUserEmailExist(string email)
        {
            throw new NotImplementedException();
        }

        public Task<bool> isUserExist(Guid userID)
        {
            throw new NotImplementedException();
        }

        public Task<bool> isUserNameExist(string name)
        {
            throw new NotImplementedException();
        }
    }
}
