using Azure;
using Gym_App.Domain;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.Interfaces.Repositries;
using Microsoft.EntityFrameworkCore;
using System.Threading;

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
        public async Task<bool> AddUserToSession(Guid userId, Guid sessionId, CancellationToken cancellationToken = default)
        {
            var session = await this.GetById(sessionId, cancellationToken);
            if (session != null)
            {
                var user = await _db.Users.FindAsync(new object[] { userId }, cancellationToken: cancellationToken);
                if (user != null)
                {
                    session.Users.Add(user);
                    return true;
                }
            }
            return false;
        }
        public async Task<Session> GetSession(Guid sessionId, int page = 1, int pageSize = 5, CancellationToken cancellationToken = default)
        {
            var session = await table
                .Include(u => u.Users)
                .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);

            if (session != null)
            {
                var paginatedMessages = await _db.Set<Message>()
                    .Where(m => m.Session.Id == sessionId)
                    .OrderByDescending(m => m.CreatedAt)
                    .Skip(pageSize * (page - 1))
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                session.Messages = paginatedMessages;
            }

            return session;
        }
        public async Task<IEnumerable<Session>> GetSessionsByUserId(Guid userId, CancellationToken cancellationToken = default)
        {
            return await table.Where(s => s.Users.Any(u => u.Id == userId)).ToListAsync(cancellationToken);
        }
        public async Task<Session> GetSessionWithUsers(Guid sessionId, CancellationToken cancellationToken = default)
        {
            return await table.Include(s => s.Users).FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
        }

        public async Task<IEnumerable<User>> GetSessionUsers(Guid sessionId, CancellationToken cancellationToken = default)
        {
            var session = await GetSessionWithUsers(sessionId, cancellationToken);
            return session?.Users ?? Enumerable.Empty<User>();
        }

        public async Task<int> GetUserSessionCount(Guid userId, CancellationToken cancellationToken = default)
        {
            return await table.CountAsync(s => s.Users.Any(u => u.Id == userId), cancellationToken);
        }

        public async Task<IEnumerable<Session>> GetUserSessions(Guid userId, CancellationToken cancellationToken = default)
        {
            return await table.Where(s => s.Users.Any(u => u.Id == userId)).ToListAsync(cancellationToken);
        }

        public async Task<bool> RemoveUserFromSession(Guid userId, Guid sessionId, CancellationToken cancellationToken = default)
        {
            var session = await this.GetById(sessionId, cancellationToken);
            if (session != null)
            {
                var user = await _db.Users.FindAsync(new object[] { userId }, cancellationToken: cancellationToken);
                if (user != null)
                {
                    session.Users.Remove(user);
                    return true;
                }
            }
            return false;
        }
    }
}
