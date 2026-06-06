using Gym_App.Domain;

namespace Gym_App.Infastructure.Interfaces.Repositries
{
    public interface ISessionRepositry : IBaseRepositry<Session>
    {
        public Task<IEnumerable<Session>> GetUserSessions(Guid userId, CancellationToken cancellationToken = default);
        public Task<IEnumerable<Session>> GetSessionsByUserId(Guid userId, CancellationToken cancellationToken = default);
        public Task<Session> GetSessionWithUsers(Guid sessionId, CancellationToken cancellationToken = default);
        public Task<Session> GetSession(Guid sessionId, int page = 1, int pageSize = 5, CancellationToken cancellationToken = default);
        public Task<IEnumerable<User>> GetSessionUsers(Guid sessionId, CancellationToken cancellationToken = default);
        public Task<int> GetUserSessionCount(Guid userId, CancellationToken cancellationToken = default);
        public Task<bool> AddUserToSession(Guid userId, Guid sessionId, CancellationToken cancellationToken = default);
        public Task<bool> RemoveUserFromSession(Guid userId, Guid sessionId, CancellationToken cancellationToken = default);

    }
}
