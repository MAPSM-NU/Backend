using Azure;
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
        public async Task<Session> GetSession(Guid sessionId, int page = 1, int pageSize = 5)
        {
            var session = await table
                .Include(u => u.Users)
                .FirstOrDefaultAsync(s => s.Id == sessionId);
            
            if (session != null)
            {
                var paginatedMessages = await _db.Set<Message>()
                    .Where(m => m.Session.Id == sessionId)
                    .OrderByDescending(m => m.CreatedAt)
                    .Skip(pageSize * (page - 1))
                    .Take(pageSize)
                    .ToListAsync();
                
                session.Messages = paginatedMessages;
            }
            
            return session;
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
