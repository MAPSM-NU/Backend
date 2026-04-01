using Gym_App.Domain;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.Interfaces.Repositries;
using Microsoft.EntityFrameworkCore;

namespace Gym_App.Infastructure.Repositries
{
    public class SessionRepositry : BaseRepositry<Session>, ISessionRepositry
    {
        private readonly DbBase _db;
        private readonly DbSet<Session> table;
        public SessionRepositry(DbBase db) : base(db)
        {
            _db = db;
            table = _db.Set<Session>();
        }
        public async Task<bool> AddUserToSession(Guid userId, Guid sessionId)
        {
            var session = await this.GetById(sessionId);
            if(session != null)
            {
                var user = await _db.Users.FindAsync(userId);
                if(user != null)
                {
                    session.Users.Add(user);
                    return true;
                }
            }
            return false;
        }

        public async Task<IEnumerable<Session>> GetSessionsByUserId(Guid userId)
        {
            return await table.Where(s => s.Users.Any(u => u.Id == userId)).ToListAsync();
        }
        public async Task<Session> GetSessionWithUsers(Guid sessionId)
        {
            return await table.Include(s => s.Users).FirstOrDefaultAsync(s => s.Id == sessionId);
        }

        public async Task<IEnumerable<User>> GetSessionUsers(Guid sessionId)
        {
           var session = await GetSessionWithUsers(sessionId);
           return session?.Users ?? Enumerable.Empty<User>();
        }

        public async Task<int> GetUserSessionCount(Guid userId)
        {
            return await table.CountAsync(s => s.Users.Any(u => u.Id == userId));
        }

        public async Task<IEnumerable<Session>> GetUserSessions(Guid userId)
        {
            return await table.Where(s => s.Users.Any(u => u.Id == userId)).ToListAsync();
        }

        public async Task<bool> RemoveUserFromSession(Guid userId, Guid sessionId)
        {
            var session = await this.GetById(sessionId);
            if(session != null)
            {
                var user = await _db.Users.FindAsync(userId);
                if(user != null)
                {
                    session.Users.Remove(user);
                    return true;
                }
            }
            return false;
        }
    }
}
